using System.Globalization;
using Bogus;

namespace RoP.Tests.Integration.Primitives;

public sealed class ResultPipelineTests
{
    private sealed record AgeDto(int Years);

    public static IEnumerable<object[]> BogusValidAgeStrings()
    {
        Faker faker = new();
        for (int i = 0; i < 10; i++)
        {
            int value = faker.Random.Int(0, 150);
            yield return new object[] { value.ToString(CultureInfo.InvariantCulture) };
        }
    }

    [Theory]
    [MemberData(nameof(BogusNonIntegerAgeStrings))]
    public void ParseAge_NonIntegerStrings_ReturnsParseFailure(string input)
    {
        Result<int> parsed = ParseAge(input);

        parsed.IsFailure.ShouldBeTrue();
        parsed.Error.Code.ShouldBe("age.parse");
    }

    public static IEnumerable<object[]> BogusNonIntegerAgeStrings()
    {
        Faker faker = new();
        yield return new object[] { faker.Lorem.Word() };
        yield return new object[] { string.Empty };
        yield return new object[] { "   " };
    }

    [Fact]
    public void FullPipeline_ValidInput_ReachesTapAndMatchSuccess()
    {
        List<string> log = [];
        int validateCalls = 0;
        int mapCalls = 0;
        int tapCalls = 0;

        string outcome = ParseAge("42")
            .Bind(age =>
            {
                validateCalls++;
                return ValidateAge(age);
            })
            .Map(age =>
            {
                mapCalls++;
                return new AgeDto(age);
            })
            .Tap(dto =>
            {
                tapCalls++;
                log.Add($"Age={dto.Years}");
            })
            .Match(
                dto => $"OK:{dto.Years}",
                err => $"ERR:{err.Code}");

        outcome.ShouldBe("OK:42");
        validateCalls.ShouldBe(1);
        mapCalls.ShouldBe(1);
        tapCalls.ShouldBe(1);
        log.Single().ShouldBe("Age=42");
    }

    [Theory]
    [MemberData(nameof(BogusValidAgeStrings))]
    public void FullPipeline_BogusValidAgeStrings_AlwaysSucceeds(string input)
    {
        string outcome = ParseAge(input)
            .Bind(ValidateAge)
            .Map(age => new AgeDto(age))
            .Tap(_ => { })
            .Match(dto => $"OK:{dto.Years}", err => $"ERR:{err.Code}");

        outcome.ShouldStartWith("OK:");
    }

    [Fact]
    public void FullPipeline_InvalidParse_ShortCircuitsLaterSteps()
    {
        int validateCalls = 0;
        int mapCalls = 0;
        int tapCalls = 0;

        string outcome = ParseAge("not-a-number")
            .Bind(age =>
            {
                validateCalls++;
                return ValidateAge(age);
            })
            .Map(age =>
            {
                mapCalls++;
                return new AgeDto(age);
            })
            .Tap(_ => tapCalls++)
            .Match(dto => $"OK:{dto.Years}", err => err.Code);

        outcome.ShouldBe("age.parse");
        validateCalls.ShouldBe(0);
        mapCalls.ShouldBe(0);
        tapCalls.ShouldBe(0);
    }

    [Fact]
    public void FullPipeline_ParseOkButValidationFails_ShortCircuitsMapAndTap()
    {
        int mapCalls = 0;
        int tapCalls = 0;

        string outcome = ParseAge("200")
            .Bind(ValidateAge)
            .Map(age =>
            {
                mapCalls++;
                return new AgeDto(age);
            })
            .Tap(_ => tapCalls++)
            .Match(dto => $"OK:{dto.Years}", err => err.Code);

        outcome.ShouldBe("age.range");
        mapCalls.ShouldBe(0);
        tapCalls.ShouldBe(0);
    }

    [Fact]
    public async Task AsyncPipeline_MapAsync_OnFailure_DoesNotRunMapper()
    {
        int asyncMapCalls = 0;

        Result<string> mapped = await ParseAge("x")
            .Bind(ValidateAge)
            .MapAsync(async age =>
            {
                asyncMapCalls++;
                await Task.Yield();
                return new AgeDto(age).Years.ToString(CultureInfo.InvariantCulture);
            });

        asyncMapCalls.ShouldBe(0);
        mapped.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task AsyncPipeline_MapAsync_OnSuccess_RunsMapper()
    {
        Result<string> mapped = await ParseAge("10")
            .Bind(ValidateAge)
            .MapAsync(async age =>
            {
                await Task.Yield();
                return new AgeDto(age).Years.ToString(CultureInfo.InvariantCulture);
            });

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("10");
    }

    [Fact]
    public void Pipeline_BindReturnsImplicitError_FromValidation()
    {
        Result<int> result = ParseAge("5")
            .Bind(_ => (Result<int>)Error.Validation("blocked", "No fives"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("blocked");
    }

    [Fact]
    public void Pipeline_BindReturnsImplicitSuccess_FromValue()
    {
        Result<int> result = ParseAge("7")
            .Bind(age => (Result<int>)(age + 1));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(8);
    }

    private static Result<int> ParseAge(string raw)
    {
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
            ? Result.Success(n)
            : Result.Failure<int>(Error.Failure("age.parse", "Not an integer"));
    }

    private static Result<int> ValidateAge(int age)
    {
        return age is >= 0 and <= 150
            ? Result.Success(age)
            : Result.Failure<int>(Error.Validation("age.range", "Age must be between 0 and 150"));
    }
}

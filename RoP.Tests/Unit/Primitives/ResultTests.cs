namespace RoP.Tests.Unit.Primitives;

public sealed class ResultTests
{
    [Fact]
    public void Success_IsSuccessWithNoneError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_IsFailureWithGivenError()
    {
        var err = Error.Failure("x", "y");

        var result = Result.Failure(err);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(err);
    }

    [Fact]
    public void Constructor_SuccessWithNone_DoesNotThrow()
    {
        Result result = new(true, Error.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Constructor_FailureWithNonNone_DoesNotThrow()
    {
        var err = Error.Failure("x", "y");

        Result result = new(false, err);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(err);
    }

    [Theory]
    [InlineData(true, "bad", "still bad")]
    [InlineData(false, "none", "No error")]
    public void Constructor_InvalidCombination_ThrowsArgumentException(
        bool isSuccess,
        string code,
        string message)
    {
        Error error = isSuccess ? Error.Failure(code, message) : Error.None;

        Should.Throw<ArgumentException>(() => _ = new Result(isSuccess, error));
    }

    [Fact]
    public void SuccessOfT_ReturnsSuccessWithValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void FailureOfT_ReturnsFailureWithError()
    {
        var err = Error.Validation("v", "msg");
        var result = Result.Failure<int>(err);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(err);
    }
}

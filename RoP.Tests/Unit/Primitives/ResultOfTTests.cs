namespace RoP.Tests.Unit.Primitives;

public sealed class ResultOfTTests
{
    [Fact]
    public void Value_OnSuccess_ReturnsValue()
    {
        var result = Result.Success(7);

        result.Value.ShouldBe(7);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(Error.Failure("c", "m"));

        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Map_OnSuccess_ReturnsMappedSuccess()
    {
        var source = Result.Success(3);

        Result<string> mapped = source.Map(x => (x * 2).ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("6");
    }

    [Fact]
    public void Map_OnFailure_DoesNotInvokeMapper()
    {
        var err = Error.Failure("c", "m");
        var source = Result.Failure<int>(err);
        int callCount = 0;

        Result<string> mapped = source.Map(_ =>
        {
            callCount++;
            return "x";
        });

        callCount.ShouldBe(0);
        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBe(err);
    }

    [Fact]
    public void Map_NullMapper_ThrowsArgumentNullException()
    {
        var source = Result.Success(1);

        Should.Throw<ArgumentNullException>(() => source.Map<string>(null!));
    }

    [Fact]
    public void Map_OnSuccess_WhenMapperThrows_PropagatesException()
    {
        var source = Result.Success(1);

        Should.Throw<InvalidOperationException>(() => source.Map<string>(_ => throw new InvalidOperationException("boom")));
    }

    [Fact]
    public async Task MapAsync_OnSuccess_ReturnsMappedSuccess()
    {
        var source = Result.Success(4);

        Result<string> mapped = await source.MapAsync(async x =>
        {
            await Task.Yield();
            return x.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("4");
    }

    [Fact]
    public async Task MapAsync_OnFailure_DoesNotInvokeMapper()
    {
        var err = Error.Failure("c", "m");
        var source = Result.Failure<int>(err);
        int callCount = 0;

        Result<string> mapped = await source.MapAsync(async _ =>
        {
            callCount++;
            await Task.Yield();
            return "x";
        });

        callCount.ShouldBe(0);
        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBe(err);
    }

    [Fact]
    public async Task MapAsync_NullMapper_ThrowsArgumentNullException()
    {
        var source = Result.Success(1);

        await Should.ThrowAsync<ArgumentNullException>(async () => await source.MapAsync<string>(null!));
    }

    [Fact]
    public async Task MapAsync_OnSuccess_WhenMapperThrows_PropagatesException()
    {
        var source = Result.Success(1);

        await Should.ThrowAsync<InvalidOperationException>(async () => await source.MapAsync<string>(async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }));
    }

    [Fact]
    public void Bind_OnSuccess_WhenBinderReturnsSuccess_ReturnsSuccess()
    {
        var source = Result.Success(2);

        Result<string> next = source.Bind(x => Result.Success(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        next.IsSuccess.ShouldBeTrue();
        next.Value.ShouldBe("2");
    }

    [Fact]
    public void Bind_OnSuccess_WhenBinderReturnsFailure_ReturnsFailure()
    {
        var source = Result.Success(2);
        var err = Error.Validation("v", "bad");

        Result<string> next = source.Bind(_ => Result.Failure<string>(err));

        next.IsFailure.ShouldBeTrue();
        next.Error.ShouldBe(err);
    }

    [Fact]
    public void Bind_OnFailure_DoesNotInvokeBinder()
    {
        var err = Error.Failure("c", "m");
        var source = Result.Failure<int>(err);
        int callCount = 0;

        Result<string> next = source.Bind(_ =>
        {
            callCount++;
            return Result.Success("x");
        });

        callCount.ShouldBe(0);
        next.IsFailure.ShouldBeTrue();
        next.Error.ShouldBe(err);
    }

    [Fact]
    public void Bind_NullBinder_ThrowsArgumentNullException()
    {
        var source = Result.Success(1);

        Should.Throw<ArgumentNullException>(() => source.Bind<string>(null!));
    }

    [Fact]
    public void Match_OnSuccess_InvokesOnlyOnSuccess()
    {
        var source = Result.Success(9);
        int successCalls = 0;
        int failureCalls = 0;

        string text = source.Match(
            v =>
            {
                successCalls++;
                return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            },
            _ =>
            {
                failureCalls++;
                return "fail";
            });

        successCalls.ShouldBe(1);
        failureCalls.ShouldBe(0);
        text.ShouldBe("9");
    }

    [Fact]
    public void Match_OnFailure_InvokesOnlyOnFailure()
    {
        var err = Error.Failure("c", "m");
        var source = Result.Failure<int>(err);
        int successCalls = 0;
        int failureCalls = 0;

        string text = source.Match(
            _ =>
            {
                successCalls++;
                return "ok";
            },
            e =>
            {
                failureCalls++;
                return e.Code;
            });

        successCalls.ShouldBe(0);
        failureCalls.ShouldBe(1);
        text.ShouldBe("c");
    }

    [Fact]
    public void Match_NullOnSuccess_ThrowsArgumentNullException()
    {
        var source = Result.Success(1);

        Should.Throw<ArgumentNullException>(() => source.Match<string>(null!, _ => "x"));
    }

    [Fact]
    public void Match_NullOnFailure_ThrowsArgumentNullException()
    {
        var source = Result.Failure<int>(Error.Failure("c", "m"));

        Should.Throw<ArgumentNullException>(() => source.Match<string>(_ => "x", null!));
    }

    [Fact]
    public void Tap_OnSuccess_InvokesActionOnceAndReturnsSameInstance()
    {
        var source = Result.Success(5);
        int callCount = 0;
        int last = 0;

        Result<int> returned = source.Tap(v =>
        {
            callCount++;
            last = v;
        });

        callCount.ShouldBe(1);
        last.ShouldBe(5);
        returned.ShouldBeSameAs(source);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotInvokeActionAndReturnsSameInstance()
    {
        var source = Result.Failure<int>(Error.Failure("c", "m"));
        int callCount = 0;

        Result<int> returned = source.Tap(_ => callCount++);

        callCount.ShouldBe(0);
        returned.ShouldBeSameAs(source);
    }

    [Fact]
    public void Tap_NullAction_ThrowsArgumentNullException()
    {
        var source = Result.Success(1);

        Should.Throw<ArgumentNullException>(() => source.Tap(null!));
    }

    [Fact]
    public void Implicit_FromNonNullReference_ReturnsSuccess()
    {
        string value = "hello";

        Result<string> result = value;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void Implicit_FromNullReference_ReturnsFailureWithNullValueError()
    {
        string? value = null;

        Result<string> result = value;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Error.NullValue);
    }

    [Fact]
    public void Implicit_FromError_ReturnsFailure()
    {
        var err = Error.Problem("p", "msg");

        Result<int> result = err;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(err);
    }

    [Fact]
    public void Implicit_FromValueTypeDefault_ReturnsSuccessWithZero()
    {
        int value = default;

        Result<int> result = value;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
    }

    [Fact]
    public void ValidationFailure_BehavesSameAsFailure()
    {
        var err = Error.Validation("v", "msg");
        var viaValidation = Result<int>.ValidationFailure(err);
        var viaFailure = Result<int>.Failure(err);

        viaValidation.IsFailure.ShouldBeTrue();
        viaFailure.IsFailure.ShouldBeTrue();
        viaValidation.Error.ShouldBe(viaFailure.Error);
    }
}

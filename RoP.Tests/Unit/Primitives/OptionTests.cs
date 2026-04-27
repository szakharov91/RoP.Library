namespace RoP.Tests.Unit.Primitives;

public sealed class OptionTests
{
    [Fact]
    public void Some_WithValue_IsSomeWithValue()
    {
        var option = Option<string>.Some("a");

        option.IsSome.ShouldBeTrue();
        option.IsNone.ShouldBeFalse();
        option.Value.ShouldBe("a");
    }

    [Fact]
    public void Some_WithNullReference_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void None_IsNoneAndThrowsOnValue()
    {
        var option = Option<string>.None();

        option.IsNone.ShouldBeTrue();
        option.IsSome.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => _ = option.Value);
    }

    [Fact]
    public void DefaultStruct_BehavesAsNone()
    {
        Option<string> option = default;

        option.IsNone.ShouldBeTrue();
        option.IsSome.ShouldBeFalse();
    }

    [Fact]
    public void Map_OnSome_ReturnsSomeWithMappedValue()
    {
        var option = Option<int>.Some(2);

        Option<string> mapped = option.Map(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsSome.ShouldBeTrue();
        mapped.Value.ShouldBe("2");
    }

    [Fact]
    public void Map_OnNone_ReturnsNone()
    {
        var option = Option<int>.None();

        Option<string> mapped = option.Map(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture));

        mapped.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Map_NullMapper_ThrowsArgumentNullException()
    {
        var option = Option<int>.Some(1);

        Should.Throw<ArgumentNullException>(() => option.Map<string>(null!));
    }

    [Fact]
    public void Bind_OnSome_WhenBinderReturnsSome_ReturnsSome()
    {
        var option = Option<int>.Some(3);

        Option<string> next = option.Bind(x => Option<string>.Some(x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        next.IsSome.ShouldBeTrue();
        next.Value.ShouldBe("3");
    }

    [Fact]
    public void Bind_OnSome_WhenBinderReturnsNone_ReturnsNone()
    {
        var option = Option<int>.Some(3);

        Option<string> next = option.Bind(_ => Option<string>.None());

        next.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Bind_OnNone_DoesNotInvokeBinder()
    {
        var option = Option<int>.None();
        int callCount = 0;

        Option<string> next = option.Bind(_ =>
        {
            callCount++;
            return Option<string>.Some("x");
        });

        callCount.ShouldBe(0);
        next.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void Bind_NullBinder_ThrowsArgumentNullException()
    {
        var option = Option<int>.Some(1);

        Should.Throw<ArgumentNullException>(() => option.Bind<string>(null!));
    }

    [Fact]
    public void Match_OnSome_InvokesSomeBranch()
    {
        var option = Option<int>.Some(4);
        int someCalls = 0;
        int noneCalls = 0;

        string text = option.Match(
            v =>
            {
                someCalls++;
                return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            },
            () =>
            {
                noneCalls++;
                return "none";
            });

        someCalls.ShouldBe(1);
        noneCalls.ShouldBe(0);
        text.ShouldBe("4");
    }

    [Fact]
    public void Match_OnNone_InvokesNoneBranch()
    {
        var option = Option<int>.None();
        int someCalls = 0;
        int noneCalls = 0;

        string text = option.Match(
            _ =>
            {
                someCalls++;
                return "some";
            },
            () =>
            {
                noneCalls++;
                return "n";
            });

        someCalls.ShouldBe(0);
        noneCalls.ShouldBe(1);
        text.ShouldBe("n");
    }

    [Fact]
    public void Match_NullSome_ThrowsArgumentNullException()
    {
        var option = Option<int>.Some(1);

        Should.Throw<ArgumentNullException>(() => option.Match<string>(null!, () => "x"));
    }

    [Fact]
    public void Match_NullNone_ThrowsArgumentNullException()
    {
        var option = Option<int>.None();

        Should.Throw<ArgumentNullException>(() => option.Match<string>(_ => "x", null!));
    }

    [Fact]
    public void ToResult_WithoutArg_OnSome_ReturnsSuccess()
    {
        var option = Option<string>.Some("ok");

        var result = option.ToResult();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("ok");
    }

    [Fact]
    public void ToResult_WithoutArg_OnNone_ReturnsFailureWithDefaultError()
    {
        var option = Option<string>.None();

        var result = option.ToResult();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("option.none");
        result.Error.Message.ShouldBe("Option has no value");
    }

    [Fact]
    public void ToResult_WithCustomError_OnNone_ReturnsFailureWithCustomError()
    {
        var option = Option<string>.None();
        var custom = Error.NotFound("nf", "missing");

        var result = option.ToResult(custom);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(custom);
    }

    [Fact]
    public void ToResult_WithCustomError_OnSome_IgnoresCustomError()
    {
        var option = Option<string>.Some("x");
        var custom = Error.NotFound("nf", "missing");

        var result = option.ToResult(custom);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("x");
    }

    [Fact]
    public void Implicit_FromNonNull_ReturnsSome()
    {
        string value = "z";

        Option<string> option = value;

        option.IsSome.ShouldBeTrue();
        option.Value.ShouldBe("z");
    }

    [Fact]
    public void Implicit_FromNull_ReturnsNone()
    {
        string? value = null;

#pragma warning disable CS8604 // Implicit conversion accepts null at runtime; nullable analysis only.
        Option<string> option = value;
#pragma warning restore CS8604

        option.IsNone.ShouldBeTrue();
    }
}

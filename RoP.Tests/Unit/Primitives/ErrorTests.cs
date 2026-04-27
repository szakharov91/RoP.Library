namespace RoP.Tests.Unit.Primitives;

public sealed class ErrorTests
{
    [Theory]
    [InlineData("code", "msg")]
    public void Failure_SetsErrorTypeAndNullException(string code, string message)
    {
        var error = Error.Failure(code, message);

        error.Code.ShouldBe(code);
        error.Message.ShouldBe(message);
        error.ErrorType.ShouldBe(ErrorType.Failure);
        error.Exception.ShouldBeNull();
    }

    [Theory]
    [InlineData("v.code", "validation msg")]
    public void Validation_SetsErrorTypeAndNullException(string code, string message)
    {
        var error = Error.Validation(code, message);

        error.ErrorType.ShouldBe(ErrorType.Validation);
        error.Exception.ShouldBeNull();
    }

    [Theory]
    [InlineData("p.code", "problem msg")]
    public void Problem_SetsErrorTypeAndNullException(string code, string message)
    {
        var error = Error.Problem(code, message);

        error.ErrorType.ShouldBe(ErrorType.Problem);
        error.Exception.ShouldBeNull();
    }

    [Theory]
    [InlineData("nf.code", "not found msg")]
    public void NotFound_SetsErrorTypeAndNullException(string code, string message)
    {
        var error = Error.NotFound(code, message);

        error.ErrorType.ShouldBe(ErrorType.NotFound);
        error.Exception.ShouldBeNull();
    }

    [Theory]
    [InlineData("cf.code", "conflict msg")]
    public void Conflict_SetsErrorTypeAndNullException(string code, string message)
    {
        var error = Error.Conflict(code, message);

        error.ErrorType.ShouldBe(ErrorType.Conflict);
        error.Exception.ShouldBeNull();
    }

    [Fact]
    public void None_HasNoneErrorType()
    {
        Error.None.ErrorType.ShouldBe(ErrorType.None);
        Error.None.Code.ShouldBe("none");
        Error.None.Message.ShouldBe("No error");
    }

    [Fact]
    public void NullValue_HasExpectedCode()
    {
        Error.NullValue.Code.ShouldBe("null.value");
        Error.NullValue.Message.ShouldBe("Value cannot be null");
        Error.NullValue.ErrorType.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void Unexpected_HasExpectedCode()
    {
        Error.Unexpected.Code.ShouldBe("unexpected");
        Error.Unexpected.Message.ShouldBe("An unexpected error occurred");
        Error.Unexpected.ErrorType.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void None_And_NullValue_AreNotEqual()
    {
        (Error.None == Error.NullValue).ShouldBeFalse();
        (Error.None != Error.NullValue).ShouldBeTrue();
    }

    [Fact]
    public void PrimaryConstructor_PreservesException()
    {
        InvalidOperationException ex = new("boom");

        Error error = new("c", "m", ErrorType.Failure, ex);

        error.Exception.ShouldBeSameAs(ex);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = Error.Failure("x", "y");
        var b = Error.Failure("x", "y");

        (a == b).ShouldBeTrue();
        a.Equals(b).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(GetInequalityCases))]
    public void RecordEquality_DifferentValues_AreNotEqual(Error left, Error right)
    {
        (left == right).ShouldBeFalse();
        left.Equals(right).ShouldBeFalse();
    }

    public static IEnumerable<object[]> GetInequalityCases()
    {
        var baseline = Error.Failure("a", "b");
        yield return new object[] { baseline, Error.Failure("z", "b") };
        yield return new object[] { baseline, Error.Failure("a", "z") };
        yield return new object[] { baseline, Error.Validation("a", "b") };
        InvalidOperationException ex = new("x");
        yield return new object[]
        {
            baseline,
            new Error("a", "b", ErrorType.Failure, ex),
        };
    }

    [Theory]
    [InlineData(ErrorType.None, -1)]
    [InlineData(ErrorType.Failure, 0)]
    [InlineData(ErrorType.Validation, 1)]
    [InlineData(ErrorType.Problem, 2)]
    [InlineData(ErrorType.NotFound, 3)]
    [InlineData(ErrorType.Conflict, 4)]
    public void ErrorType_HasExpectedNumericValue(ErrorType errorType, int expected)
    {
        ((int)errorType).ShouldBe(expected);
    }

    [Fact]
    public void ErrorType_ToString_IsStableForAssertions()
    {
        string text = ErrorType.Validation.ToString();

        text.ShouldBe(nameof(ErrorType.Validation));
    }
}

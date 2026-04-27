namespace RoP.Tests.Integration.Primitives;

public sealed class OptionPipelineTests
{
    [Fact]
    public void OptionMapBind_ToResult_ThenResultChain_PropagatesSuccess()
    {
        Option<string> found = Lookup("alice");

        Result<int> result = found
            .Map(s => s.Length)
            .Bind(len => len > 0 ? Option<int>.Some(len * 2) : Option<int>.None())
            .ToResult()
            .Bind(n => n > 4 ? Result.Success(n) : Result.Failure<int>(Error.Failure("small", "Too small")));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(10);
    }

    [Fact]
    public void OptionNone_ToResult_NotFound_PropagatesThroughResultBind()
    {
        var notFound = Error.NotFound("user", "User not found");

        Result<int> result = Lookup("missing")
            .Map(s => s.Length)
            .ToResult(notFound)
            .Bind(len => Result.Success(len));

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(notFound);
    }

    [Fact]
    public void ImplicitLookup_FromNullableString_MirrorsRepositorySemantics()
    {
        Result<string> r1 = LookupAsResult(null);
        Result<string> r2 = LookupAsResult("alice");

        r1.IsFailure.ShouldBeTrue();
        r1.Error.Code.ShouldBe("lookup.none");
        r2.IsSuccess.ShouldBeTrue();
        r2.Value.ShouldBe("abcde");
    }

    private static Option<string> Lookup(string key)
    {
        Dictionary<string, string> db = new(StringComparer.Ordinal)
        {
            ["alice"] = "abcde",
        };

        return db.TryGetValue(key, out string? value)
            ? Option<string>.Some(value)
            : Option<string>.None();
    }

    private static Result<string> LookupAsResult(string? key)
    {
        if (key is null)
        {
            return Option<string>.None().ToResult(Error.Failure("lookup.none", "Key was null"));
        }

        return Lookup(key).ToResult(Error.NotFound("user", "Not in db"));
    }
}

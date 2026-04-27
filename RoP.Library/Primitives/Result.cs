using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoP.Library.Primitives;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new ArgumentException("Invalid error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error): base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary> Convert to Result </summary>
    public Result<TOther> Map<TOther>(Func<TValue, TOther> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess
            ? Result<TOther>.Success(mapper(Value))
            : Result<TOther>.Failure(Error);
    }

    /// <summary> Convert to Result (async version) </summary>
    public async Task<Result<TOther>> MapAsync<TOther>(Func<TValue, Task<TOther>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess
            ? Result<TOther>.Success(await mapper(Value))
            : Result<TOther>.Failure(Error);
    }

    /// <summary> Bind - for functions which are returning Result TOther </summary>
    public Result<TOther> Bind<TOther>(Func<TValue, Result<TOther>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSuccess ? binder(Value) : Result<TOther>.Failure(Error);
    }

    /// <summary> Match – handling both cases </summary>
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    /// <summary> Tap – a side action (e.g. logging) </summary>
    public Result<TValue> Tap(Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (IsSuccess)
            action(Value);

        return this;
    }

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    public static new Result<TValue> Failure(Error error) => new(default, false, error);

    public static Result<TValue> ValidationFailure(Error error) => new(default, false, error);
}

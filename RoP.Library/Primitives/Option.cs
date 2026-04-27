using System;
using System.Collections.Generic;

namespace RoP.Library.Primitives;

/// <summary>
/// Опциональное значение: Some (есть) или None (нет).
/// </summary>
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;

    private Option(T value)
    {
        IsSome = true;
        _value = value;
    }

    public bool IsSome { get; }
    public bool IsNone => !IsSome;

    public T Value => IsSome
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a None option.");

    public static Option<T> Some(T value) =>
        value is null ? throw new ArgumentNullException(nameof(value)) : new Option<T>(value);
    public static Option<T> None() => default;

    public Option<TOther> Map<TOther>(Func<T, TOther> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSome ? Option<TOther>.Some(mapper(Value)) : Option<TOther>.None();
    }

    public Option<TOther> Bind<TOther>(Func<T, Option<TOther>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSome ? binder(Value) : Option<TOther>.None();
    }

    public TOut Match<TOut>(Func<T, TOut> some, Func<TOut> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return IsSome ? some(Value) : none();
    }

    public Result<T> ToResult(Error? noneError = null)
    {
        return IsSome
            ? Result<T>.Success(Value)
            : Result<T>.Failure(noneError ?? new Error("option.none", "Option has no value"));
    }

    public static implicit operator Option<T>(T value) => value is null ? None() : Some(value);

    public bool Equals(Option<T> other)
    {
        if (!IsSome && !other.IsSome)
        {
            return true;
        }

        if (IsSome != other.IsSome)
        {
            return false;
        }

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public override int GetHashCode() => IsSome ? HashCode.Combine(true, _value) : HashCode.Combine(false, 0);

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

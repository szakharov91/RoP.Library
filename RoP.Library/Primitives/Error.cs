using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoP.Library.Primitives;

/// <summary> Provide error with code and description and optional exception </summary>
public sealed record Error(string Code, string Message, ErrorType ErrorType = ErrorType.Failure, Exception? Exception = null)
{
    public static readonly Error None = new("none", "No error", ErrorType.None);
    public static readonly Error NullValue = new("null.value", "Value cannot be null");
    public static readonly Error Unexpected = new("unexpected", "An unexpected error occurred");

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}

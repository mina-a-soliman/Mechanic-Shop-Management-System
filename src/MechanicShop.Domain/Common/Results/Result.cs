
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using MechanicShop.Domain.Common.Results.Abstractions;

namespace MechanicShop.Domain.Common.Results;

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Deleted;
public readonly record struct Updated;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
}

public sealed class Result<TValue> : IResult<TValue>
{
    private readonly TValue? _value = default;
    private List<Error>? _errors = null;
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public List<Error> Errors => IsError ? _errors! : [];

    public TValue Value => IsSuccess ? _value! : default!;

    public Error TopError => (_errors?.Count > 0) ? _errors[0] : default;


    private Result(Error error)
    {
        _errors = [error];
    }
    private Result(List<Error> errors)
    {
        if (errors is null || errors.Count == 0)
        {
            throw new ArgumentException("Cannot create an ErrorOr<TValue> from an empty collection of errors. Provide at least one error", nameof(errors));
        }

        _errors = errors;
        IsSuccess = false;
    }
    private Result(TValue Value)
    {
        if (Value is null)
            throw new ArgumentNullException(nameof(Value));

        _value = Value;
        IsSuccess = true;
    }

    // Executes the appropriate function based on the result state.
    // If successful, invokes onValue with the value.
    // Otherwise, invokes onError with the list of errors.
    public TNextValue Match<TNextValue>(Func<TValue, TNextValue> onValue, Func<List<Error>, TNextValue> onError)
        => IsSuccess ? onValue(Value!) : onError(Errors);

    // Allows returning a value directly instead of explicitly creating a Result<TValue>.
    // e.g. return value; instead of return new Result<TValue>(value);
    public static implicit operator Result<TValue>(TValue value)
        => new(value);

    // Allows returning a single Error directly.
    // e.g. return Error.NotFound(...); instead of return new Result<TValue>(error);
    public static implicit operator Result<TValue>(Error error)
        => new(error);

    public static implicit operator Result<TValue>(List<Error> errors)
        => new(errors);



    // Tells the serializer to use this constructor for deserializing json 
    // e.g.: {"value": 42,"errors": [],"isSuccess": true } => new Result<int>(42, new List<Error>(), true);
    [JsonConstructor]

    // Tells IDEs like Visual Studio to hide this constructor from IntelliSense. (No Auto Complete but still usable if developer typed it by himself)
    [EditorBrowsable(EditorBrowsableState.Never)]

    // [Tells the IDE to produce a compile-time error if this constructor used.
    // [Obsolete("message", isError)]:
    // - message: appears to the developer
    // - isError : if false = Warning, true = Compiler Error
    [Obsolete("For serializer only.", true)]

    public Result(TValue? value, List<Error>? errors, bool isSuccess)
    {
        if (isSuccess)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _errors = [];
            IsSuccess = true;
        }
        else
        {
            if (errors is null || errors.Count == 0)
                throw new ArgumentException("Provide at least one error.", nameof(errors));

            _errors = errors;
            _value = default;
            IsSuccess = false;
        }
    }

}
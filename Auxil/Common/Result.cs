using System.Diagnostics.CodeAnalysis;

namespace Auxil.Common;

/// <summary>Represents the outcome of an operation without a return value.</summary>
[PublicAPI]
public sealed class Result
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }
    /// <summary>Whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;
    /// <summary>The error message when the operation failed.</summary>
    public string ErrorMessage { get; }

    private Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Ok() => new(true, string.Empty);

    /// <summary>Creates a failed result with the given error message.</summary>
    /// <param name="errorMessage">The error message.</param>
    public static Result Error(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);

        return new Result(false, errorMessage);
    }

    /// <summary>Runs the action and captures any thrown exception as an error result.</summary>
    /// <param name="action">The action to run.</param>
    public static Result Catch(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            return Ok();
        }
        catch (Exception exception)
        {
            return Error(exception.Message);
        }
    }

    /// <summary>Evaluates one of the two functions depending on the result state.</summary>
    /// <typeparam name="TResult">The result type of both branches.</typeparam>
    /// <param name="onSuccess">Called with no arguments on success.</param>
    /// <param name="onError">Called with the error message on failure.</param>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        return IsSuccess ? onSuccess() : onError(ErrorMessage);
    }

    /// <summary>Runs one of the two actions depending on the result state.</summary>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called with the error message on failure.</param>
    public void Match(Action onSuccess, Action<string> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        if (IsSuccess)
        {
            onSuccess();
        }

        else
        {
            onError(ErrorMessage);
        }
    }

    /// <summary>Deconstructs the result into its state and error message.</summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="errorMessage">The error message when the operation failed.</param>
    public void Deconstruct(out bool isSuccess, out string errorMessage)
    {
        isSuccess = IsSuccess;
        errorMessage = ErrorMessage;
    }
}

/// <summary>Represents the outcome of an operation that produces a value.</summary>
/// <typeparam name="T">The type of the produced value.</typeparam>
[PublicAPI]
public sealed class Result<T>
{
    /// <summary>Whether the operation succeeded.</summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }
    /// <summary>Whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;
    /// <summary>The error message when the operation failed.</summary>
    public string ErrorMessage { get; }

    /// <summary>The produced value when the operation succeeded; otherwise the default value.</summary>
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a successful result carrying the given value.</summary>
    /// <param name="value">The produced value.</param>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Creates a failed result with the given error message.</summary>
    /// <param name="errorMessage">The error message.</param>
    public static Result<T> Error(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);

        return new Result<T>(false, default, errorMessage);
    }

    /// <summary>Runs the function and captures any thrown exception as an error result.</summary>
    /// <param name="action">The function to run.</param>
    public static Result<T> Catch(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            return Ok(action());
        }
        catch (Exception exception)
        {
            return Error(exception.Message);
        }
    }

    /// <summary>Returns the value on success, or the given default on failure.</summary>
    /// <param name="defaultValue">The value to return on failure.</param>
    public T? GetValueOrDefault(T defaultValue) => IsSuccess ? Value : defaultValue;

    /// <summary>Evaluates one of the two functions depending on the result state.</summary>
    /// <typeparam name="TResult">The result type of both branches.</typeparam>
    /// <param name="onSuccess">Called with the value on success.</param>
    /// <param name="onError">Called with the error message on failure.</param>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        return IsSuccess ? onSuccess(Value) : onError(ErrorMessage);
    }

    /// <summary>Runs one of the two actions depending on the result state.</summary>
    /// <param name="onSuccess">Called with the value on success.</param>
    /// <param name="onError">Called with the error message on failure.</param>
    public void Match(Action<T> onSuccess, Action<string> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        if (IsSuccess)
        {
            onSuccess(Value);
        }

        else
        {
            onError(ErrorMessage);
        }
    }

    /// <summary>Transforms the value on success, preserving the error on failure.</summary>
    /// <typeparam name="TNew">The new value type.</typeparam>
    /// <param name="mapper">The transform to apply to the value.</param>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess
            ? Result<TNew>.Ok(mapper(Value))
            : Result<TNew>.Error(ErrorMessage);
    }

    /// <summary>Chains another result-producing operation on success, short-circuiting on failure.</summary>
    /// <typeparam name="TNew">The new value type.</typeparam>
    /// <param name="binder">The operation to run with the value.</param>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsSuccess
            ? binder(Value)
            : Result<TNew>.Error(ErrorMessage);
    }

    /// <summary>Deconstructs the result into its state and value.</summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="value">The produced value when the operation succeeded.</param>
    public void Deconstruct(out bool isSuccess, out T? value)
    {
        isSuccess = IsSuccess;
        value = Value;
    }
}

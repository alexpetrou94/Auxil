namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with <see cref="DateTime"/> values.</summary>
[PublicAPI]
public static class DateTimeExtensions
{
    /// <summary>Returns the date component of the value.</summary>
    public static DateOnly GetDateOnly(this DateTime dateTime)
        => DateOnly.FromDateTime(dateTime);

    /// <summary>Returns the time component of the value.</summary>
    public static TimeOnly GetTimeOnly(this DateTime dateTime)
        => TimeOnly.FromDateTime(dateTime);
}

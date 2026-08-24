namespace Auxil.Common.Notifications;

/// <summary>Defines the presentation-independent severity of a notification.</summary>
[PublicAPI]
public enum NotificationSeverity
{
    /// <summary>Informational notification.</summary>
    Information,

    /// <summary>Successful operation notification.</summary>
    Success,

    /// <summary>Notification that requires attention but is not an error.</summary>
    Warning,

    /// <summary>Notification describing a failed operation or error.</summary>
    Error,
}

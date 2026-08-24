namespace Auxil.Common.Notifications;

/// <summary>Describes a data-only notification action selected by a consumer.</summary>
[PublicAPI]
public sealed class NotificationActionInvokedEventArgs : EventArgs
{
    internal NotificationActionInvokedEventArgs(Notification notification, NotificationAction action)
    {
        Notification = notification;
        Action = action;
    }

    /// <summary>The immutable notification snapshot associated with the action.</summary>
    public Notification Notification { get; }

    /// <summary>The selected notification action.</summary>
    public NotificationAction Action { get; }
}

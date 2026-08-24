namespace Auxil.Common.Notifications;

/// <summary>Describes a notification state change.</summary>
[PublicAPI]
public sealed class NotificationChangedEventArgs : EventArgs
{
    internal NotificationChangedEventArgs(NotificationChangeKind kind, Notification notification)
    {
        Kind = kind;
        Notification = notification;
    }

    /// <summary>The kind of state change.</summary>
    public NotificationChangeKind Kind { get; }

    /// <summary>The immutable notification snapshot after the change.</summary>
    public Notification Notification { get; }
}

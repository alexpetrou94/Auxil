namespace Auxil.Common.Notifications;

/// <summary>Identifies a state change raised by <see cref="NotificationCenter"/>.</summary>
[PublicAPI]
public enum NotificationChangeKind
{
    /// <summary>A notification was published.</summary>
    Added,

    /// <summary>A notification's content was updated.</summary>
    Updated,

    /// <summary>A notification's read state changed.</summary>
    ReadStateChanged,

    /// <summary>A notification moved to dismissed history.</summary>
    Dismissed,

    /// <summary>A notification moved to archived history.</summary>
    Archived,

    /// <summary>A notification was removed from the center.</summary>
    Removed
}

namespace Auxil.Common.Notifications;

/// <summary>Describes an action exposed by a notification without containing executable UI behavior.</summary>
[PublicAPI]
public sealed class NotificationAction
{
    /// <summary>Creates an action with a stable identifier and display label.</summary>
    /// <param name="id">The stable identifier used when the action is selected.</param>
    /// <param name="label">The display label supplied to a consumer.</param>
    /// <param name="isDestructive">Whether the action represents a destructive operation.</param>
    public NotificationAction(string id, string label, bool isDestructive = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        Id = id;
        Label = label;
        IsDestructive = isDestructive;
    }

    /// <summary>The stable action identifier.</summary>
    public string Id { get; }

    /// <summary>The label a consumer may display.</summary>
    public string Label { get; }

    /// <summary>Whether the action represents a destructive operation.</summary>
    public bool IsDestructive { get; }
}

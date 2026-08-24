namespace Auxil.Common.Notifications;

/// <summary>Configures a <see cref="NotificationCenter"/>.</summary>
[PublicAPI]
public sealed class NotificationCenterOptions
{
    /// <summary>The number of dismissed or archived records retained in history; zero means unlimited.</summary>
    public uint HistoryLimit { get; init; } = 100;

    /// <summary>The clock used for notification timestamps and expiration checks.</summary>
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;
}

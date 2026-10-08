using Microsoft.EntityFrameworkCore;
using TNO.Entities;

namespace TNO.DAL.Extensions;

/// <summary>
/// SubscriptionExtensions static class, provides optimistic concurrency checks for subscriptions submitted by a client.
/// A client submits the subscriptions it loaded, each with the version it loaded.
/// When one of them has changed since, or a subscription exists that the client never loaded, the client's copy is stale.
/// The save is rejected instead of overwriting the change.
/// </summary>
public static class SubscriptionExtensions
{
    #region Variables
    private const string Refresh = "Refresh your data and reapply your changes.";
    #endregion

    #region Methods
    /// <summary>
    /// Throw a concurrency error if the stored 'current' subscription is not the one the client loaded.
    /// A null 'expectedVersion' means the client is adding a subscription it never loaded, which is not checked.
    /// </summary>
    /// <param name="current">The stored subscription, or null if it does not exist.</param>
    /// <param name="expectedVersion">The version of the subscription the client loaded.</param>
    /// <param name="description">Identifies the subscription in the error.</param>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    public static void ThrowIfStale(this AuditColumns? current, long? expectedVersion, string description)
    {
        if (expectedVersion == null) return;
        if (current == null)
            throw new DbUpdateConcurrencyException($"{description} no longer exists.  {Refresh}");
        if (current.Version != expectedVersion)
            throw new DbUpdateConcurrencyException($"{description} has been modified since it was loaded.  {Refresh}");
    }

    /// <summary>
    /// Throw a concurrency error because the stored subscription was not submitted by the client.
    /// Used where the client submits every subscription it loaded, so a missing one was added after it loaded.
    /// </summary>
    /// <param name="description">Identifies the subscription in the error.</param>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    public static DbUpdateConcurrencyException NotLoaded(string description)
    {
        return new DbUpdateConcurrencyException($"{description} was added after the subscribers were loaded.  {Refresh}");
    }
    #endregion
}

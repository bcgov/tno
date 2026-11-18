using System.Collections.Concurrent;

namespace TNO.Core.Http;

/// <summary>
/// A token issued by the OpenIdConnect server, with the times it can be used until.
/// </summary>
/// <param name="AccessToken">The access token.</param>
/// <param name="AccessExpiresOn">When the access token expires (UTC).</param>
/// <param name="RefreshToken">The refresh token, if one was issued.</param>
/// <param name="RefreshExpiresOn">When the refresh token expires (UTC).</param>
public record OpenIdConnectCachedToken(string AccessToken, DateTime AccessExpiresOn, string? RefreshToken, DateTime? RefreshExpiresOn);

/// <summary>
/// OpenIdConnectTokenCache class, holds access tokens so that they can be reused until they expire.
/// Register it as a singleton to share a token between clients that are created per request.
/// </summary>
public class OpenIdConnectTokenCache
{
    #region Variables
    /// <summary>
    /// A token is renewed this long before it expires, so a request is not sent with a token that
    /// expires on the way to the server.
    /// </summary>
    public static readonly TimeSpan RenewBeforeExpiry = TimeSpan.FromSeconds(30);

    private sealed class Entry
    {
        public OpenIdConnectCachedToken? Token { get; set; }
        public Task<OpenIdConnectCachedToken>? Renewal { get; set; }
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    #endregion

    #region Methods
    /// <summary>
    /// Get the access token for the specified 'key', renewing it when it is close to expiring.
    /// Concurrent callers share a single renewal, and share its failure, so an unavailable server
    /// does not make them wait for each other in turn.
    /// A failed renewal keeps the current token while it has not expired.
    /// </summary>
    /// <param name="key">Identifies the credentials the token belongs to.</param>
    /// <param name="renew">Fetches a new token. It is passed the refresh token when one can be used.</param>
    /// <returns>The access token.</returns>
    public async Task<string> GetAccessTokenAsync(string key, Func<string?, Task<OpenIdConnectCachedToken>> renew)
    {
        var entry = _entries.GetOrAdd(key, _ => new Entry());
        Task<OpenIdConnectCachedToken> renewal;
        lock (entry)
        {
            if (entry.Token != null && DateTime.UtcNow < entry.Token.AccessExpiresOn - RenewBeforeExpiry)
                return entry.Token.AccessToken;
            // Task.Run ensures the renewal cannot finish before it is assigned.
            renewal = entry.Renewal ??= Task.Run(() => RenewAsync(entry, renew));
        }
        return (await renewal).AccessToken;
    }

    /// <summary>
    /// Remove the specified 'accessToken' because the server rejected it. A token that has since
    /// replaced it is kept.
    /// </summary>
    /// <param name="key">Identifies the credentials the token belongs to.</param>
    /// <param name="accessToken">The access token that was rejected.</param>
    public void Invalidate(string key, string accessToken)
    {
        if (!_entries.TryGetValue(key, out var entry)) return;
        lock (entry)
        {
            if (entry.Token?.AccessToken == accessToken) entry.Token = null;
        }
    }

    private static async Task<OpenIdConnectCachedToken> RenewAsync(Entry entry, Func<string?, Task<OpenIdConnectCachedToken>> renew)
    {
        OpenIdConnectCachedToken? current;
        lock (entry) current = entry.Token;
        try
        {
            var refreshToken = current?.RefreshExpiresOn > DateTime.UtcNow ? current.RefreshToken : null;
            var token = await renew(refreshToken);
            lock (entry) entry.Token = token;
            return token;
        }
        catch when (current != null && DateTime.UtcNow < current.AccessExpiresOn)
        {
            return current;
        }
        finally
        {
            lock (entry) entry.Renewal = null;
        }
    }
    #endregion
}

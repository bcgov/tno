using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Core.Http.Models;
using TNO.CSS.Config;

namespace TNO.CSS;

/// <summary>
/// CssClient class, provides a way to ensure every request includes an authorization token.
/// </summary>
public class CssClient : HttpRequestClient, ICssClient
{
    #region Variables
    // private const string TOKEN_URL = "/api/v1/token";
    // private const string TOKEN_URL = "/auth/realms/standard/protocol/openid-connect/token";
    private TokenModel? _accessToken = null;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    #endregion

    #region Properties
    /// <summary>
    /// get - The configuration options.
    /// </summary>
    public CssOptions ClientOptions { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CssClient object, initializes with specified parameters.
    /// </summary>
    /// <param name="clientFactory"></param>
    /// <param name="tokenHandler"></param>
    /// <param name="options"></param>
    /// <param name="cssOptions"></param>
    /// <param name="logger"></param>
    public CssClient(IHttpClientFactory clientFactory,
        JwtSecurityTokenHandler tokenHandler,
        IOptionsMonitor<JsonSerializerOptions> options,
        IOptionsMonitor<CssOptions> cssOptions,
        ILogger<HttpRequestClient> logger) : base(clientFactory, options, logger)
    {
        this.ClientOptions = cssOptions.CurrentValue;
        _tokenHandler = tokenHandler;
    }
    #endregion

    #region  Methods
    #region  Token
    /// <summary>
    /// Request an access token that can be used to authorize a HTTP request.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="HttpClientRequestException"></exception>
    protected async Task<string> RequestAccessTokenAsync()
    {
        HttpResponseMessage response;
        var now = DateTime.UtcNow;
        var accessToken = _accessToken?.AccessToken != null ? _tokenHandler.ReadJwtToken(_accessToken.AccessToken) : null;
        var refreshToken = _accessToken?.RefreshToken != null ? _tokenHandler.ReadJwtToken(_accessToken.RefreshToken) : null;
        if (accessToken == null ||
            (accessToken?.ValidTo <= now && (refreshToken == null || refreshToken?.ValidTo <= now))
        )
        {
            // If there is no access token, or the refresh token has expired.
            response = await RequestTokenAsync();
        }
        else if (accessToken?.ValidTo <= now && refreshToken?.ValidTo > now)
        {
            // If the access token has expired, but not the refresh token has not expired.
            response = await RefreshTokenAsync(refreshToken.RawData);
        }
        else if (accessToken != null)
        {
            // We have a valid token, keep on using it.
            return $"Bearer {accessToken.RawData}";
        }
        else return String.Empty;

        // Extract the JWT token to use when making the request.
        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            _accessToken = await responseStream.DeserializeAsync<TokenModel>();
            return $"Bearer {_accessToken?.AccessToken}";
        }
        else
        {
            throw new HttpClientRequestException(response);
        }
    }

    /// <summary>
    /// Make an HTTP request for a new token.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public async Task<HttpResponseMessage> RequestTokenAsync()
    {
        var authority = this.ClientOptions.Authority ??
            throw new ConfigurationException($"Configuration 'CSS:Authority' is missing or invalid.");
        var clientId = this.ClientOptions.ClientId ??
            throw new ConfigurationException($"Configuration 'CSS:ClientId' is missing or invalid.");
        var secret = this.ClientOptions.Secret ??
            throw new ConfigurationException($"Configuration 'CSS:Secret' is missing or invalid.");

        // Use the configuration settings if available, or make a request to Keycloak for the appropriate endpoint URL.
        var authentication = $"{clientId}:{secret}";
        var base64 = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authentication));

        var tokenUrl = new Uri(authority, this.ClientOptions.TokenPath);
        using var tokenMessage = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        var p = new Dictionary<string, string>
                { { "grant_type", "client_credentials" } };
        var form = new FormUrlEncodedContent(p);
        form.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        tokenMessage.Content = form;
        tokenMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
        return await this.Client.SendAsync(tokenMessage);
    }

    /// <summary>
    /// CSS Doesn't currently support refreshing tokens, so this will just make a request for a new one.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> RefreshTokenAsync(string refreshToken)
    {
        return await RequestTokenAsync();
    }
    #endregion

    #region Send Methods
    /// <summary>
    /// Make a request to the specified 'url', with the specified HTTP 'method', with the specified 'content'.
    /// Make a request to the open id connect server for an authentication token if required.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public override async Task<HttpResponseMessage> SendAsync(string url, HttpMethod? method, HttpContent? content = null)
    {
        return await SendAsync(url, method, new HttpRequestMessage().Headers, content);
    }

    /// <summary>
    /// Make a request to the specified 'url', with the specified HTTP 'method', with the specified 'content'.
    /// Make a request to the open id connect server for an authentication token if required.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="headers"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public override async Task<HttpResponseMessage> SendAsync(string url, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null,
            string? etag = null)
    {
        if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException($"Argument '{nameof(url)}' must be a valid URL.");
        method ??= HttpMethod.Get;
        headers ??= new HttpRequestMessage().Headers;
        headers.Add("User-Agent", GetType().FullName);

        var token = await RequestAccessTokenAsync();
        if (!String.IsNullOrWhiteSpace(token)) headers.Add("Authorization", token.ToString());

        return await base.SendAsync(url, method, headers, content, etag);

    }
    #endregion
    #endregion
}

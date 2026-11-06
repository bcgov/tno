using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Http.Configuration;

namespace TNO.Core.Http
{
    /// <summary>
    /// OpenIdConnectRequestClient class, provides a common structure to make requests to an OpenIdConnect server.
    /// </summary>
    public class OpenIdConnectRequestClient : HttpRequestClient, IOpenIdConnectRequestClient
    {
        #region Variables
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly OpenIdConnectTokenCache _tokenCache;
        private readonly ILogger<OpenIdConnectRequestClient> _logger;
        private const string BearerPrefix = "Bearer ";
        #endregion

        #region Properties
        /// <summary>
        /// get - Identifies the credentials a cached token belongs to.
        /// </summary>
        private string TokenCacheKey => $"{this.AuthClientOptions.Authority}|{this.AuthClientOptions.Audience}";

        /// <summary>
        /// get - The configuration options.
        /// </summary>
        public AuthClientOptions AuthClientOptions { get; }

        /// <summary>
        /// get - The configuration options.
        /// </summary>
        public OpenIdConnectOptions OpenIdConnectOptions { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a KeycloakRequestClient class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="tokenHandler"></param>
        /// <param name="authClientOptions"></param>
        /// <param name="openIdConnectOptions"></param>
        /// <param name="serializerOptions"></param>
        /// <param name="logger"></param>
        /// <param name="tokenCache">Shares the token with other clients. Without it this client keeps its own token.</param>
        public OpenIdConnectRequestClient(
            IHttpClientFactory clientFactory,
            JwtSecurityTokenHandler tokenHandler,
            IOptionsMonitor<AuthClientOptions> authClientOptions,
            IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions,
            IOptionsMonitor<JsonSerializerOptions> serializerOptions,
            ILogger<OpenIdConnectRequestClient> logger,
            OpenIdConnectTokenCache? tokenCache = null)
            : base(clientFactory, serializerOptions, logger)
        {
            this.AuthClientOptions = authClientOptions.CurrentValue;
            this.OpenIdConnectOptions = openIdConnectOptions.CurrentValue;
            _tokenHandler = tokenHandler;
            _tokenCache = tokenCache ?? new OpenIdConnectTokenCache();
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Make a request to fetch the OpenIdConnect endpoints.
        /// </summary>
        /// <returns></returns>
        public async Task<Models.OpenIdConnectModel?> GetOpenIdConnectEndpoints()
        {
            var authority = this.AuthClientOptions.Authority ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Authority' is missing or invalid.");

            var response = await this.Client.GetAsync($"{authority}/.well-known/openid-configuration");

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Models.OpenIdConnectModel>(body, _serializeOptions);
            }
            else
            {
                throw new HttpClientRequestException(response);
            }
        }

        /// <summary>
        /// Make a request to the OpenIdConnect token endpoint as the Service Account to fetch the 'access_token'.
        /// This requires configuration of the following keys.
        ///     OpenIdConnect:Authority
        ///     OpenIdConnect:Client
        ///     OpenIdConnect:Secret
        ///     OpenIdConnect:Audience
        /// </summary>
        /// <exception type="Helpers.Exceptions.HttpClientRequestException">If the request fails.</exception>
        /// <returns></returns>
        public async Task<string> RequestAccessToken()
        {
            return $"{BearerPrefix}{await _tokenCache.GetAccessTokenAsync(TokenCacheKey, RenewTokenAsync)}";
        }

        /// <summary>
        /// Fetch a new token, with the 'refreshToken' when there is one. A rejected refresh token,
        /// such as one from a session the server has ended, falls back to the service account.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private async Task<OpenIdConnectCachedToken> RenewTokenAsync(string? refreshToken)
        {
            try
            {
                if (refreshToken != null)
                {
                    using var refreshResponse = await RefreshToken(refreshToken);
                    if (refreshResponse.IsSuccessStatusCode) return await ReadTokenAsync(refreshResponse);
                    _logger.LogWarning("Refreshing the access token failed with status {status}. Requesting a new token.", refreshResponse.StatusCode);
                }

                var response = await RequestToken();
                if (!response.IsSuccessStatusCode) throw new HttpClientRequestException(response);
                using (response) return await ReadTokenAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to renew the access token from {authority}.", this.AuthClientOptions.Authority);
                throw;
            }
        }

        /// <summary>
        /// Read the token and its expiry from the specified 'response'.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<OpenIdConnectCachedToken> ReadTokenAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Models.TokenModel>(body, _serializeOptions);
            if (String.IsNullOrWhiteSpace(token?.AccessToken))
                throw new InvalidOperationException("The token response does not include an access token.");

            // Only a refresh token with a known expiry can be reused.
            var refreshToken = token.RefreshToken != null && _tokenHandler.CanReadToken(token.RefreshToken) ? token.RefreshToken : null;
            return new OpenIdConnectCachedToken(
                token.AccessToken,
                _tokenHandler.ReadJwtToken(token.AccessToken).ValidTo,
                refreshToken,
                refreshToken != null ? _tokenHandler.ReadJwtToken(refreshToken).ValidTo : null);
        }

        #region Service Account Requests
        /// <summary>
        /// Make a request to the OpenIdConnect token endpoint as the Service Account.
        /// This requires configuration of the following keys.
        ///     OpenIdConnect:Authority
        ///     OpenIdConnect:Client
        ///     OpenIdConnect:Secret
        ///     OpenIdConnect:Audience
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> RequestToken()
        {
            var authority = this.AuthClientOptions.Authority ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Authority' is missing or invalid.");
            var audience = this.AuthClientOptions.Audience ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Audience' is missing or invalid.");
            var secret = this.AuthClientOptions.Secret ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Secret' is missing or invalid.");

            // Use the configuration settings if available, or make a request to Keycloak for the appropriate endpoint URL.
            var keycloakTokenUrl = this.OpenIdConnectOptions.Token;
            if (String.IsNullOrWhiteSpace(keycloakTokenUrl))
            {
                var endpoints = await GetOpenIdConnectEndpoints();
                keycloakTokenUrl = endpoints?.Token_endpoint;
            }
            else if (!keycloakTokenUrl.StartsWith("http"))
            {
                keycloakTokenUrl = $"{authority}{keycloakTokenUrl}";
            }

            using var tokenMessage = new HttpRequestMessage(HttpMethod.Post, keycloakTokenUrl);
            // var authentication = $"{audience}:{secret}";
            // var base64 = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authentication));
            // tokenMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            var p = new Dictionary<string, string>
                { { "client_id", audience },
                    { "grant_type", "client_credentials" },
                    { "client_secret", secret },
                    { "audience", audience }
                };
            var form = new FormUrlEncodedContent(p);
            form.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            tokenMessage.Content = form;
            return await this.Client.SendAsync(tokenMessage);
        }

        /// <summary>
        /// Refresh the access token via the specified 'refreshToken'.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> RefreshToken(string refreshToken)
        {
            var authority = this.AuthClientOptions.Authority ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Authority' is missing or invalid.");
            var audience = this.AuthClientOptions.Audience ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Audience' is missing or invalid.");
            var secret = this.AuthClientOptions.Secret ??
                throw new ConfigurationException($"Configuration 'OpenIdConnect:Secret' is missing or invalid.");

            // Use the configuration settings if available, or make a request to Keycloak for the appropriate endpoint URL.
            var keycloakTokenUrl = this.OpenIdConnectOptions.Token;
            if (String.IsNullOrWhiteSpace(keycloakTokenUrl))
            {
                var endpoints = await GetOpenIdConnectEndpoints();
                keycloakTokenUrl = endpoints?.Token_endpoint;
            }
            else if (!keycloakTokenUrl.StartsWith("http"))
            {
                keycloakTokenUrl = $"{authority}{keycloakTokenUrl}";
            }

            using var tokenMessage = new HttpRequestMessage(HttpMethod.Post, keycloakTokenUrl);
            var p = new Dictionary<string, string>
                { { "client_id", audience },
                    { "grant_type", "refresh_token" },
                    { "client_secret", secret },
                    { "refresh_token", refreshToken }
                };
            var form = new FormUrlEncodedContent(p);
            form.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            tokenMessage.Content = form;
            return await this.Client.SendAsync(tokenMessage);
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
        /// <param name="etag"></param>
        /// <returns></returns>
        public override async Task<HttpResponseMessage> SendAsync(string url, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null,
                string? etag = null)
        {
            if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException($"Argument '{nameof(url)}' must be a valid URL.");
            method ??= HttpMethod.Get;
            headers ??= new HttpRequestMessage().Headers;

            var token = await RequestAccessToken();

            if (!String.IsNullOrWhiteSpace(token))
            {
                if (!headers.TryGetValues("Authorization", out _))
                    headers.Add("Authorization", token.ToString());
                else
                    headers.Authorization = new AuthenticationHeaderValue(token.ToString());
            }

            var response = await base.SendAsync(url, method, headers, content, etag);

            // The server can stop accepting a token before it expires, the next request needs a new one.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                _tokenCache.Invalidate(TokenCacheKey, token[BearerPrefix.Length..]);

            return response;
        }
        #endregion
        #endregion
    }
}

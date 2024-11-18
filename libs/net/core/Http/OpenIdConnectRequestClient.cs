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
        public Models.TokenModel? _accessToken = null;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        #endregion

        #region Properties
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
        public OpenIdConnectRequestClient(
            IHttpClientFactory clientFactory,
            JwtSecurityTokenHandler tokenHandler,
            IOptionsMonitor<AuthClientOptions> authClientOptions,
            IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions,
            IOptionsMonitor<JsonSerializerOptions> serializerOptions,
            ILogger<OpenIdConnectRequestClient> logger)
            : base(clientFactory, serializerOptions, logger)
        {
            this.AuthClientOptions = authClientOptions.CurrentValue;
            this.OpenIdConnectOptions = openIdConnectOptions.CurrentValue;
            _tokenHandler = tokenHandler;
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
            HttpResponseMessage response;
            var now = DateTime.UtcNow;
            var accessToken = _accessToken?.AccessToken != null ? _tokenHandler.ReadJwtToken(_accessToken.AccessToken) : null;
            var refreshToken = _accessToken?.RefreshToken != null ? _tokenHandler.ReadJwtToken(_accessToken.RefreshToken) : null;
            if (accessToken == null ||
                (accessToken?.ValidTo <= now && (refreshToken == null || refreshToken?.ValidTo <= now))
            )
            {
                // If there is no access token, or the refresh token has expired.
                response = await RequestToken();
            }
            else if (accessToken?.ValidTo <= now && refreshToken?.ValidTo > now)
            {
                // If the access token has expired, but not the refresh token has not expired.
                response = await RefreshToken(refreshToken.RawData);
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
                var body = await response.Content.ReadAsStringAsync();
                _accessToken = JsonSerializer.Deserialize<Models.TokenModel>(body, _serializeOptions);
                return $"Bearer {_accessToken?.AccessToken}";
            }
            else
            {
                throw new HttpClientRequestException(response);
            }
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

            return await base.SendAsync(url, method, headers, content, etag);

        }
        #endregion
        #endregion
    }
}

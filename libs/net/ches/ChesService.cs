using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Core.Http.Models;

namespace TNO.Ches
{
    /// <summary>
    /// ChesService class, provides a service for integration with Ches API services.
    /// </summary>
    public partial class ChesService : IChesService
    {
        #region Variables

        [GeneratedRegex("src=\\\"data:(image\\/[a-zA-Z]*);base64,([^\\\"]*)\\\"", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-CA")]
        private static partial Regex Base64InlineImageRegex();
        private readonly ClaimsPrincipal _user;
        private TokenModel? _token = null;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly ILogger<IChesService> _logger;

        #endregion

        #region Properties
        protected IHttpRequestClient Client { get; }
        public ChesOptions Options { get; }
        #endregion
        private static readonly Regex ExtractBase64InlineImageRegex = Base64InlineImageRegex();

        #region Constructors
        /// <summary>
        /// Creates a new instance of a ChesService, initializes with specified arguments.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="user"></param>
        /// <param name="client"></param>
        /// <param name="tokenHandler"></param>
        /// <param name="logger"></param>
        public ChesService(IOptions<ChesOptions> options, ClaimsPrincipal user, IHttpRequestClient client, JwtSecurityTokenHandler tokenHandler, ILogger<IChesService> logger)
        {
            this.Options = options.Value;
            _user = user;
            this.Client = client;
            _tokenHandler = tokenHandler;
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates the full URL including the host.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="outputFormat"></param>
        /// <returns></returns>
        private string GenerateUrl(string endpoint)
        {
            return $"{this.Options.HostUri}{endpoint}";
        }

        /// <summary>
        /// Ensure we have an active access token.
        /// Make an HTTP request if one is needed.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshAccessTokenAsync()
        {
            // Check if token has expired.  If it has refresh it.
            if (_token == null || String.IsNullOrWhiteSpace(_token.AccessToken) || _tokenHandler.ReadJwtToken(_token.AccessToken).ValidTo <= DateTime.UtcNow)
            {
                _token = await GetTokenAsync();
            }
        }

        /// <summary>
        /// Send a request to the specified endpoint.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<string> SendAsync(string endpoint, HttpMethod method)
        {
            await RefreshAccessTokenAsync();

            var url = GenerateUrl(endpoint);

            var headers = new HttpRequestMessage().Headers;
            headers.Add("Authorization", $"Bearer {_token?.AccessToken}");

            try
            {
                var response = await this.Client.SendAsync(url, method, headers);
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpClientRequestException ex)
            {
                _logger.LogError(ex, "Failed to send/receive request: {status} {url}", ex.StatusCode, url);
                if (ex.Response != null)
                {
                    var response = await this.Client.DeserializeAsync<Ches.Models.ErrorResponseModel>(ex.Response);
                    throw new ChesException(ex, this.Client, response);
                }
                throw new ChesException("Failed to send message to CHES", ex);
            }
        }

        /// <summary>
        /// Send a request to the specified endpoint.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<TR?> SendAsync<TR>(string endpoint, HttpMethod method)
        {
            await RefreshAccessTokenAsync();

            var url = GenerateUrl(endpoint);

            var headers = new HttpRequestMessage().Headers;
            headers.Add("Authorization", $"Bearer {_token?.AccessToken}");

            try
            {
                return await this.Client.SendAsync<TR>(url, method, headers);
            }
            catch (HttpClientRequestException ex)
            {
                _logger.LogError(ex, "Failed to send/receive request: {status} {url}", ex.StatusCode, url);
                if (ex.Response != null)
                {
                    var response = await this.Client.DeserializeAsync<Ches.Models.ErrorResponseModel>(ex.Response);
                    throw new ChesException(ex, this.Client, response);
                }
                throw new ChesException("Failed to send message to CHES", ex);
            }
        }

        /// <summary>
        /// Send a request to the specified endpoint.
        /// Make a request to get an access token if required.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TD"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<TR?> SendAsync<TR, TD>(string endpoint, HttpMethod method, TD data)
            where TD : class
        {
            await RefreshAccessTokenAsync();

            var url = GenerateUrl(endpoint);

            var headers = new HttpRequestMessage().Headers;
            headers.Add("Authorization", $"Bearer {_token?.AccessToken}");

            try
            {
                return await this.Client.SendJsonAsync<TR, TD>(url, method, headers, data);
            }
            catch (HttpClientRequestException ex)
            {
                if (ex.Response != null)
                {
                    var response = await this.Client.DeserializeAsync<Ches.Models.ErrorResponseModel>(ex.Response);
                    var error = String.Join(Environment.NewLine, response?.Errors.Select(e => e.Message) ?? []);
                    _logger.LogError(ex, "Failed to send/receive request: {code} {url}.  Error: {error}", ex.StatusCode, url, error);
                    throw new ChesException(ex, this.Client, response);
                }
                _logger.LogError(ex, "Failed to send/receive request: {code} {url}.  Error: {error}", ex.StatusCode, url, ex.GetAllMessages());
                throw new ChesException("Failed to send message to CHES", ex);
            }
        }

        /// <summary>
        /// Make an HTTP request to CHES to get an access token for the specified 'username' and 'password'.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<TokenModel?> GetTokenAsync(string? username = null, string? password = null)
        {
            var headers = new HttpRequestMessage().Headers;
            var creds = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username ?? this.Options.Username}:{password ?? this.Options.Password}"));
            headers.Add("Authorization", $"Basic {creds}");
            headers.Add("ContentType", "application/x-www-form-urlencoded");

            var form = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials")
            };
            var content = new FormUrlEncodedContent(form);

            try
            {
                return await this.Client.SendAsync<TokenModel>(this.Options.AuthUrl, HttpMethod.Post, headers, content);
            }
            catch (HttpClientRequestException ex)
            {
                _logger.LogError(ex, "Failed to send/receive request: {code} {url}", ex.StatusCode, this.Options.AuthUrl);
                if (ex.Response != null)
                {
                    var response = await this.Client.DeserializeAsync<Ches.Models.ErrorResponseModel>(ex.Response);
                    throw new ChesException(ex, this.Client, response);
                }
                throw new ChesException("Failed to send message to CHES", ex);
            }
        }

        /// <summary>
        /// Send an HTTP request to CHES to send the specified 'email'.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<EmailResponseModel> SendEmailAsync(IEmail email)
        {
            ArgumentNullException.ThrowIfNull(email);

            email.From = this.Options.From ?? email.From;

            if (this.Options.BccUser)
            {
                email.Bcc = new[] { _user.GetEmail() }.Concat(email.Bcc?.Any() ?? false ? email.Bcc : []).NotNullOrWhiteSpace();
            }
            if (!String.IsNullOrWhiteSpace(this.Options.AlwaysBcc))
            {
                email.Bcc = this.Options.AlwaysBcc.Split(";").Select(e => e?.Trim()).Concat(email.Bcc?.Any() ?? false ? email.Bcc : []).NotNullOrWhiteSpace();
            }
            if (!String.IsNullOrWhiteSpace(this.Options.OverrideTo) || !this.Options.EmailAuthorized)
            {
                email.To = !String.IsNullOrWhiteSpace(this.Options.OverrideTo)
                    ? this.Options.OverrideTo.Split(";").NotNullOrWhiteSpace().Select(e => e.Trim())
                    : new[] { _user.GetEmail() }.Where(e => !String.IsNullOrWhiteSpace(e)).Select(e => e!);
                email.Cc = email.Cc.Any() ? new[] { _user.GetEmail() }.NotNullOrWhiteSpace() : [];
                email.Bcc = [];
            }
            if (this.Options.AlwaysDelay.HasValue)
            {
                email.SendOn = email.SendOn.AddSeconds(this.Options.AlwaysDelay.Value);
            }

            // Make sure there are no blank CC or BCC;
            email.To = email.To.NotNullOrWhiteSpace();
            email.Cc = email.Cc?.NotNullOrWhiteSpace() ?? [];
            email.Bcc = email.Bcc?.NotNullOrWhiteSpace() ?? [];

            // convert any embedded base64 images into attachments
            Dictionary<string, AttachmentModel> inlineImageMatches = GetImagesFromEmailBody(email.Body);
            if (inlineImageMatches.Count != 0)
            {
                foreach (KeyValuePair<string, AttachmentModel> m in inlineImageMatches)
                {
                    email.Body = email.Body.Replace(m.Key, $"src=\"cid:{m.Value.Filename}\"");
                }
                email.Attachments = email.Attachments.Any()
                    ? email.Attachments.AppendRange([.. inlineImageMatches.Values])
                    : [.. inlineImageMatches.Values];
            }

            if (this.Options.EmailEnabled)
                return await SendAsync<EmailResponseModel, IEmail>("/email", HttpMethod.Post, email) ?? new EmailResponseModel();

            return new EmailResponseModel();
        }

        /// <summary>
        /// parses email markup string checking for base64 encoded images
        /// </summary>
        /// <param name="emailBody">email body as html markup - possibly containing base64 encoded images</param>
        /// <returns>dictionary of the images as attachments and the 'key' to use to search and replace them in the markup</returns>
        private static Dictionary<string, AttachmentModel> GetImagesFromEmailBody(string emailBody)
        {
            var imageDictionary = new Dictionary<string, AttachmentModel>();

            var inlineImageMatches = ExtractBase64InlineImageRegex.Matches(emailBody);
            if (inlineImageMatches.Count != 0)
            {
                foreach (var m in inlineImageMatches.Cast<Match>())
                {
                    var imageMediaType = m.Groups[1].Value;
                    var base64Image = m.Groups[2].Value;
                    var attachment = new AttachmentModel
                    {
                        ContentType = imageMediaType,
                        Encoding = "base64",
                        Filename = Guid.NewGuid().ToString(),
                        Content = base64Image
                    };
                    imageDictionary.TryAdd(m.Value, attachment);
                }
            }
            return imageDictionary;
        }

        /// <summary>
        /// Send an HTTP request to CHES to send the specified 'email'.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<EmailResponseModel> SendEmailAsync(IEmailMerge email)
        {
            ArgumentNullException.ThrowIfNull(email);

            email.From = this.Options.From ?? email.From;

            if (this.Options.BccUser)
            {
                var address = new[] { _user.GetEmail() };
                email.Contexts.ForEach(c =>
                {
                    c.Bcc = address.Concat(c.Bcc?.Any() ?? false ? c.Bcc : []).NotNullOrWhiteSpace();
                });
            }
            if (!String.IsNullOrWhiteSpace(this.Options.AlwaysBcc))
            {
                email.Contexts.ForEach(c =>
                {
                    c.Bcc = this.Options.AlwaysBcc.Split(";").Select(e => e?.Trim()).Concat(c.Bcc?.Any() ?? false ? c.Bcc : []).NotNullOrWhiteSpace();
                });
            }
            if (!String.IsNullOrWhiteSpace(this.Options.OverrideTo) || !this.Options.EmailAuthorized)
            {
                var address = !String.IsNullOrWhiteSpace(this.Options.OverrideTo)
                    ? this.Options.OverrideTo?.Split(";").NotNullOrWhiteSpace().Select(e => e.Trim()) ?? []
                    : new[] { _user.GetEmail() }.NotNullOrWhiteSpace();
                email.Contexts.ForEach(c =>
                {
                    c.To = address;
                    c.Cc = [];
                    c.Bcc = [];
                });
            }
            if (this.Options.AlwaysDelay.HasValue)
            {
                email.Contexts.ForEach(c =>
                    c.SendOn = c.SendOn.AddSeconds(this.Options.AlwaysDelay.Value));
            }

            // Make sure there are no blank CC or BCC;
            email.Contexts.ForEach(c =>
            {
                c.To = c.To.NotNullOrWhiteSpace();
                c.Cc = c.Cc.NotNullOrWhiteSpace();
                c.Bcc = c.Bcc.NotNullOrWhiteSpace();
            });

            // convert any embedded base64 images into attachments
            Dictionary<string, AttachmentModel> inlineImageMatches = GetImagesFromEmailBody(email.Body);
            if (inlineImageMatches.Count != 0)
            {
                foreach (KeyValuePair<string, AttachmentModel> m in inlineImageMatches)
                {
                    email.Body = email.Body.Replace(m.Key, $"src=\"cid:{m.Value.Filename}\"");
                }
                email.Attachments = email.Attachments.Any()
                    ? email.Attachments.AppendRange([.. inlineImageMatches.Values])
                    : [.. inlineImageMatches.Values];
            }

            if (this.Options.EmailEnabled)
                return await SendAsync<EmailResponseModel, IEmailMerge>("/emailMerge", HttpMethod.Post, email) ?? new EmailResponseModel();

            return new EmailResponseModel();
        }

        /// <summary>
        /// Send an HTTP request to get the current status of the message for the specified 'messageId'.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<StatusResponseModel> GetStatusAsync(Guid messageId)
        {
            return await SendAsync<StatusResponseModel>($"/status/{messageId}", HttpMethod.Get) ?? new StatusResponseModel();
        }

        /// <summary>
        /// Send an HTTP request to get the current status of the message(s) for the specified 'filter'.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StatusResponseModel>> GetStatusAsync(StatusModel filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (!filter.MessageId.HasValue && !filter.TransactionId.HasValue && String.IsNullOrWhiteSpace(filter.Status) && String.IsNullOrWhiteSpace(filter.Tag)) throw new ArgumentException("At least one parameter must be specified.");

            var query = HttpUtility.ParseQueryString(String.Empty);
            if (filter.MessageId.HasValue) query.Add("msgId", $"{filter.MessageId}");
            if (!String.IsNullOrEmpty(filter.Status)) query.Add("status", $"{filter.Status}");
            if (!String.IsNullOrEmpty(filter.Tag)) query.Add("tag", $"{filter.Tag}");
            if (filter.TransactionId.HasValue) query.Add("txId", $"{filter.TransactionId}");

            return await SendAsync<IEnumerable<StatusResponseModel>>($"/status?{query}", HttpMethod.Get) ?? [];
        }

        /// <summary>
        /// Send a cancel HTTP request to CHES for the specified 'messageId'.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<StatusResponseModel> CancelEmailAsync(Guid messageId)
        {
            // Need to determine if we can cancel the email.
            var response = await GetStatusAsync(messageId);
            if (response.Status == "accepted" || response.Status == "pending")
            {
                await SendAsync($"/cancel/{messageId}", HttpMethod.Delete);
                response.Status = "cancelled";
            }
            return response;
        }

        /// <summary>
        /// Send a cancel HTTP request to CHES for the specified 'filter'.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StatusResponseModel>> CancelEmailAsync(StatusModel filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (!filter.MessageId.HasValue && !filter.TransactionId.HasValue && String.IsNullOrWhiteSpace(filter.Status) && String.IsNullOrWhiteSpace(filter.Tag)) throw new ArgumentException("At least one parameter must be specified.");

            var query = HttpUtility.ParseQueryString(String.Empty);
            if (filter.MessageId.HasValue) query.Add("msgId", $"{filter.MessageId}");
            if (!String.IsNullOrEmpty(filter.Status)) query.Add("status", $"{filter.Status}");
            if (!String.IsNullOrEmpty(filter.Tag)) query.Add("tag", $"{filter.Tag}");
            if (filter.TransactionId.HasValue) query.Add("txId", $"{filter.TransactionId}");

            // TODO: This will probably not work as CHES currently doesn't like if you attempt to cancel a message that can't be cancelled.
            // Additionally CHES fails (times-out) if you make a request for the status of a cancelled message.
            await SendAsync($"/cancel?{query}", HttpMethod.Delete);
            return await GetStatusAsync(filter);
        }

        /// <summary>
        /// Send an HTTP request to promote the message for the specified 'messageId'.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task PromoteAsync(Guid messageId)
        {
            await SendAsync($"/promote/{messageId}", HttpMethod.Post);
        }
        #endregion
    }
}

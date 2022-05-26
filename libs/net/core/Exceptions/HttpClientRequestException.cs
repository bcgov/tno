using System.Net;

namespace TNO.Core.Exceptions
{
    /// <summary>
    /// HttpClientRequestException class, provides a way to express HTTP request exceptions that occur.
    /// </summary>
    public class HttpClientRequestException : HttpRequestException
    {
        #region Properties
        /// <summary>
        /// get - The HTTP response message.
        /// </summary>
        /// <value></value>
        public HttpResponseMessage? Response { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an HttpClientRequestException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public HttpClientRequestException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, null, statusCode)
        {
        }

        /// <summary>
        /// Creates a new instance of an HttpClientRequestException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public HttpClientRequestException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, innerException, statusCode)
        {
            if (innerException is HttpClientRequestException)
            {
                this.Response = ((HttpClientRequestException)innerException).Response;
            }
        }

        /// <summary>
        /// Creates a new instance of an HttpClientRequestException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public HttpClientRequestException(HttpResponseMessage response) : base($"HTTP Request '{response?.RequestMessage?.RequestUri}' failed", null, response?.StatusCode)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response)); // NOSONAR
            var body = response.Content.ReadAsStringAsync().Result;
            this.Data["Body"] = body;
        }

        /// <summary>
        /// Creates a new instance of an HttpClientRequestException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public HttpClientRequestException(HttpResponseMessage response, string message) : base(message, null, response?.StatusCode)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response)); // NOSONAR
            var body = response.Content.ReadAsStringAsync().Result;
            this.Data["Body"] = body;
        }

        /// <summary>
        /// Creates a new instance of an HttpClientRequestException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public HttpClientRequestException(HttpResponseMessage response, Exception innerException) : base($"HTTP Request '{response?.RequestMessage?.RequestUri}' failed", innerException, response?.StatusCode)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response)); // NOSONAR
            var body = response.Content.ReadAsStringAsync().Result;
            this.Data["Body"] = body;
        }
        #endregion
    }
}

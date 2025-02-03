using System.Net;
using TNO.Ches.Models;
using TNO.Core.Http;

namespace TNO.Core.Exceptions
{
    /// <summary>
    /// ChesException class, provides a way to express HTTP request exceptions that occur.
    /// </summary>
    public class ChesException : HttpClientRequestException
    {
        #region Properties
        /// <summary>
        /// get - Additional detail on the error.
        /// </summary>
        public string? Detail { get; }

        /// <summary>
        /// get - The HTTP request client the exception originated from.
        /// </summary>
        public IHttpRequestClient? Client { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="client"></param>
        /// <param name="model"></param>
        public ChesException(HttpClientRequestException exception, IHttpRequestClient client, ErrorResponseModel? model)
            : this($"{exception.Message}{Environment.NewLine}", exception, exception?.StatusCode ?? HttpStatusCode.InternalServerError)
        {
            this.Client = client;
            this.Detail = model != null
                ? $"{model.Title}{Environment.NewLine}{model.Detail}{Environment.NewLine}{model.Type}{Environment.NewLine}{String.Join(Environment.NewLine, model.Errors.Select(e => $"\t{e.Message}"))}"
                : null;
            this.Data.Add("error", model);
        }

        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public ChesException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }

        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public ChesException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, innerException, statusCode)
        {
        }

        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public ChesException(HttpResponseMessage response) : base(response)
        {
        }

        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public ChesException(HttpResponseMessage response, Exception innerException) : base(response, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of an ChesException class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ChesException(HttpResponseMessage response, ErrorResponseModel model) : base(response)
        {
            this.Detail = $"{model.Title}{Environment.NewLine}{model.Detail}{Environment.NewLine}{model.Type}{Environment.NewLine}{String.Join(Environment.NewLine, model.Errors.Select(e => $"\t{e.Message}"))}";
            this.Data.Add("error", model);
        }
        #endregion
    }
}

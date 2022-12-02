using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TNO.CSS.Models;

namespace TNO.CSS.Extensions
{
    /// <summary>
    /// HttpResponseMessageExtensions static class, provides extension methods for HttpResponseMessage objects.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Provides a generic way to deserialize the response to return the specified 'T' type, or to throw an exception if the request failed.
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception type="HttpClientRequestException">The request failed.</exception>
        /// <returns></returns>
        public static async Task<T> HandleResponseAsync<T>(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    try
                    {
                        using var responseStream = await response.Content.ReadAsStreamAsync();
                        var result = await responseStream.DeserializeAsync<T>();
                        if (result != null) return result;
                        else if (typeof(T).IsNullable())
                        {
                            return default!;
                        }
                        throw new InvalidOperationException("Response was empty and the specified type is not nullable.");
                    }
                    catch (Exception ex)
                    {
                        throw new HttpClientRequestException(response, ex);
                    }
                }

                if (typeof(T).IsNullable())
                {
                    return default!;
                }
                throw new InvalidOperationException("Response was empty and the specified type is not nullable.");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Keycloak returns 404s when an item doesn't exist instead of the correct 204...
                    // We will have to assume that all 404 are now item does not exist and should be returning a 204 instead.
                    // We will then return 'null'.
                    if (typeof(T).IsNullable())
                    {
                        return default!;
                    }
                    throw new InvalidOperationException("Response was empty and the specified type is not nullable.");
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var error = await responseStream.DeserializeAsync<ErrorResponseModel>();
                throw new HttpClientRequestException(response, error?.Message ?? "An unexpected error");
            }
        }

        /// <summary>
        /// Provides a generic way to return the specified result, or to throw an exception if the request failed.
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception type="HttpClientRequestException">The request failed.</exception>
        /// <returns></returns>
        public static async Task HandleResponseAsync(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var error = await responseStream.DeserializeAsync<ErrorResponseModel>();
                throw new HttpClientRequestException(response, error?.Message ?? "An unexpected error");
            }
        }

        /// <summary>
        /// Provides a generic way to return the specified result, or to throw an exception if the request failed.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception type="HttpClientRequestException">The request failed.</exception>
        /// <returns></returns>
        public static async Task<T> HandleResponseAsync<T>(this HttpResponseMessage response, T result)
        {
            if (response.IsSuccessStatusCode)
            {
                return result;
            }
            else
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var error = await responseStream.DeserializeAsync<ErrorResponseModel>();
                throw new HttpClientRequestException(response, error?.Message ?? "An unexpected error");
            }
        }
    }
}

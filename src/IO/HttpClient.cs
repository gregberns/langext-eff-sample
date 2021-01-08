using System.Net.Http;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LangExtEffSample
{
    public interface HttpClientIO
    {
        EitherAsync<Error, HttpResponse> HttpRequest(HttpRequestMessage message);
        Task<Either<Error, HttpResponse>> HttpRequestAsync(HttpRequestMessage message);
    }

    public class LiveHttpClientIO : HttpClientIO
    {
        HttpClient _httpClient;
        public LiveHttpClientIO()
        {
            _httpClient = new HttpClient();
        }

        public EitherAsync<Error, HttpResponse> HttpRequest(HttpRequestMessage message) =>
            HttpRequestAsync(message).ToAsync();

        public async Task<Either<Error, HttpResponse>> HttpRequestAsync(HttpRequestMessage message)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.SendAsync(message);
            }
            catch (Exception e)
            {
                return Left<Error, HttpResponse>(Error.New($"HttpRequest failed. Message: {e.Message}", e));
            }
            return await ProcessHttpResponse(response);
        }

        private static async Task<Either<Error, HttpResponse>> ProcessHttpResponse(HttpResponseMessage response)
        {
            Stream stream = null;
            try
            {
                stream = await response.Content.ReadAsStreamAsync();
                var body = await new StreamReader(stream).ReadToEndAsync();
                return Right<Error, HttpResponse>(new HttpResponse()
                {
                    StatusCode = (int)response.StatusCode,
                    Headers = response.Headers.ToDictionary(
                            kv => kv.Key,
                            kv => kv.Value.Freeze()
                        ).ToMap(),
                    Body = body
                });
            }
            catch (Exception e)
            {
                return Left<Error, HttpResponse>(Error.New($"ProcessHttpResponse ReadAsStreamAsync failed. Message: {e.Message}", e));
            }
        }

        /// <summary>
        /// Serialize object to JSON, to be passed into an HttpClient
        /// </summary>
        public static HttpContent CreateJsonRequest<T>(T obj) =>
            new StringContent(JsonConvert.SerializeObject(obj), System.Text.Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Object containing data returned from Http Response
    /// </summary>
    public struct HttpResponse
    {
        /// <summary>
        /// Status Code of the Http Response
        /// </summary>
        public int StatusCode;
        /// <summary>
        /// Headers from Http Response
        /// </summary>
        public Map<string, Lst<string>> Headers;
        /// <summary>
        /// Body from Http Response
        /// </summary>
        public string Body;
    }

}

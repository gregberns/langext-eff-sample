using System.Net.Http;
using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Interfaces;
using LanguageExt.Attributes;
using static LanguageExt.Prelude;

namespace LanguageExt.Interfaces
{
    public interface HttpClientIO
    {
        // ToDo
        ValueTask<HttpResponseMessage> GetAsync(string requestUri);
    }
    [Typeclass("*")]
    public interface HasHttpClient<RT> : HasCancel<RT>
        where RT : struct, HasCancel<RT>
    {
        Eff<RT, HttpClientIO> HttpClientEff { get; }
        Aff<RT, HttpClientIO> HttpClientAff { get; }
    }

    public interface HttpResponseMessageIO
    {
        HttpContent Content();
        // ToDo
    }
    [Typeclass("*")]
    public interface HasHttpResponseMessage<RT> : HasCancel<RT>
        where RT : struct, HasCancel<RT>
    {
        Eff<RT, HttpResponseMessageIO> HttpResponseMessageEff { get; }
        Aff<RT, HttpResponseMessageIO> HttpResponseMessageAff { get; }
    }
    public interface HttpContentIO
    {
        ValueTask<string> ReadAsStringAsync();
    }
    [Typeclass("*")]
    public interface HasHttpContent<RT> : HasCancel<RT>
        where RT : struct, HasCancel<RT>
    {
        Eff<RT, HttpContentIO> HttpContentEff { get; }
        Aff<RT, HttpContentIO> HttpContentAff { get; }
    }
}

namespace LanguageExt.LiveIO
{
    public struct HttpClientIO : Interfaces.HttpClientIO
    {
        private readonly HttpClient _client;
        public static Interfaces.HttpClientIO Default =>
            new HttpClientIO(new HttpClient());

        public HttpClientIO(HttpClient client)
        {
            _client = client;
        }

        // HttpClient.HttpClient()
        // Initializes a new instance of the System.Net.Http.HttpClient  class using a System.Net.Http.HttpClientHandler  that is disposed when this instance is disposed.

        // HttpClient.HttpClient(HttpMessageHandler handler)
        // handler: The HTTP handler stack to use for sending requests.
        // Initializes a new instance of the System.Net.Http.HttpClient  class with the specified handler. The handler is disposed when this instance is disposed.

        // HttpMessageHandler handler, bool disposeHandler)
        // handler: The System.Net.Http.HttpMessageHandler responsible for processing the HTTP response messages.
        // Initializes a new instance of the System.Net.Http.HttpClient  class with the provided handler, and specifies whether that handler should be disposed when this instance is disposed.

        // h.BaseAddress
        // System.Uri HttpClient.BaseAddress { get; set; }
        // Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests.
        public System.Uri BaseAddress() =>
            _client.BaseAddress;

        public Unit SetBaseAddress(System.Uri uri)
        {
            _client.BaseAddress = uri;
            return unit;
        }

        // void HttpClient.CancelPendingRequests()
        // Cancel all pending requests on this instance.
        public Unit CancelPendingRequests()
        {
            _client.CancelPendingRequests();
            return unit;
        }

        // System.Net.Http.Headers.HttpRequestHeaders HttpClient.DefaultRequestHeaders { get; }
        // Gets the headers which should be sent with each request.
        public System.Net.Http.Headers.HttpRequestHeaders DefaultRequestHeaders() =>
            _client.DefaultRequestHeaders;

        // h.DefaultRequestVersion
        // System.Version HttpClient.DefaultRequestVersion { get; set; }
        // Gets or sets the default HTTP version used on subsequent requests made by this HttpClient instance.
        public System.Version DefaultRequestVersion() =>
            _client.DefaultRequestVersion;

        public Unit SetDefaultRequestVersion(System.Version version)
        {
            _client.DefaultRequestVersion = version;
            return unit;
        }

        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.DeleteAsync(string requestUri) (+ 3 overloads)
        // Send a DELETE request to the specified Uri as an asynchronous operation.
        public ValueTask<HttpResponseMessage> DeleteAsync(string requestUri) =>
            _client.DeleteAsync(requestUri).ToValue();
        public ValueTask<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken) =>
            _client.DeleteAsync(requestUri, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> DeleteAsync(Uri requestUri) =>
            _client.DeleteAsync(requestUri).ToValue();
        public ValueTask<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken) =>
            _client.DeleteAsync(requestUri, cancellationToken).ToValue();

        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.GetAsync(string requestUri) (+ 7 overloads)
        // Send a GET request to the specified Uri as an asynchronous operation.
        public ValueTask<HttpResponseMessage> GetAsync(string requestUri) =>
            _client.GetAsync(requestUri).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption) =>
            _client.GetAsync(requestUri, completionOption).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
            _client.GetAsync(requestUri, completionOption, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken) =>
            _client.GetAsync(requestUri, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(Uri requestUri) =>
            _client.GetAsync(requestUri).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption) =>
            _client.GetAsync(requestUri, completionOption).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
            _client.GetAsync(requestUri, completionOption, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken) =>
            _client.GetAsync(requestUri, cancellationToken).ToValue();

        // (awaitable) System.Threading.Tasks.Task<byte[]> HttpClient.GetByteArrayAsync(string requestUri) (+ 1 overload)
        // Sends a GET request to the specified Uri and return the response body as a byte array in an asynchronous operation.
        public ValueTask<byte[]> GetByteArrayAsync(string requestUri) =>
            _client.GetByteArrayAsync(requestUri).ToValue();
        public ValueTask<byte[]> GetByteArrayAsync(Uri requestUri) =>
            _client.GetByteArrayAsync(requestUri).ToValue();

        // (awaitable) System.Threading.Tasks.Task<System.IO.Stream> HttpClient.GetStreamAsync(string requestUri) (+ 1 overload)
        // Send a GET request to the specified Uri and return the response body as a stream in an asynchronous operation.
        public ValueTask<System.IO.Stream> GetStreamAsync(string requestUri) =>
            _client.GetStreamAsync(requestUri).ToValue();
        public ValueTask<System.IO.Stream> GetStreamAsync(Uri requestUri) =>
            _client.GetStreamAsync(requestUri).ToValue();

        // (awaitable) System.Threading.Tasks.Task<string> HttpClient.GetStringAsync(string requestUri) (+ 1 overload)
        // Send a GET request to the specified Uri and return the response body as a string in an asynchronous operation.
        public ValueTask<string> GetStringAsync(string requestUri) =>
            _client.GetStringAsync(requestUri).ToValue();
        public ValueTask<string> GetStringAsync(Uri requestUri) =>
            _client.GetStringAsync(requestUri).ToValue();

        // long HttpClient.MaxResponseContentBufferSize { get; set; }
        // Gets or sets the maximum number of bytes to buffer when reading the response content.
        public long MaxResponseContentBufferSize() =>
            _client.MaxResponseContentBufferSize;

        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.PatchAsync(string requestUri, HttpContent content) (+ 3 overloads)
        // Sends a PATCH request to a Uri designated as a string as an asynchronous operation.
        public ValueTask<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content) =>
            _client.PatchAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PatchAsync(requestUri, content, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content) =>
            _client.PatchAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PatchAsync(requestUri, content, cancellationToken).ToValue();

        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.PostAsync(string requestUri, HttpContent content) (+ 3 overloads)
        // Send a POST request to the specified Uri as an asynchronous operation.
        public ValueTask<HttpResponseMessage> PostAsync(string requestUri, HttpContent content) =>
            _client.PostAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PostAsync(requestUri, content, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content) =>
            _client.PostAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PostAsync(requestUri, content, cancellationToken).ToValue();

        // h.PutAsync
        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.PutAsync(string requestUri, HttpContent content) (+ 3 overloads)
        // Send a PUT request to the specified Uri as an asynchronous operation.
        public ValueTask<HttpResponseMessage> PutAsync(string requestUri, HttpContent content) =>
            _client.PutAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PutAsync(requestUri, content, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content) =>
            _client.PutAsync(requestUri, content).ToValue();
        public ValueTask<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken) =>
            _client.PutAsync(requestUri, content, cancellationToken).ToValue();

        // h.SendAsync
        // (awaitable) System.Threading.Tasks.Task<HttpResponseMessage> HttpClient.SendAsync(HttpRequestMessage request) (+ 3 overloads)
        // Send an HTTP request as an asynchronous operation.
        public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
            _client.SendAsync(request).ToValue();
        public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption) =>
            _client.SendAsync(request, completionOption).ToValue();
        public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
            _client.SendAsync(request, completionOption, cancellationToken).ToValue();
        public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            _client.SendAsync(request, cancellationToken).ToValue();

        // System.TimeSpan HttpClient.Timeout { get; set; }
        // Gets or sets the timespan to wait before the request times out.
        public System.TimeSpan Timeout() =>
            _client.Timeout;
        public Unit SetTimeout(System.TimeSpan timeout)
        {
            _client.Timeout = timeout;
            return unit;
        }
    }

    public static class HttpClient_IO
    {
        public static Aff<RT, HttpResponseMessage> getAsync<RT>(string requestUri)
            where RT : struct, HasHttpClient<RT> =>
                default(RT).HttpClientAff.MapAsync(e => e.GetAsync(requestUri));

        // public static Aff<RT, HttpResponseMessage> getAsync2<RT>(string requestUri)
        //     where RT : struct, HasHttpClient<RT>, HasHttpResponseMessage<RT> =>
        //         // from c in default(RT).HttpClientAff.MapAsync(e => e.GetAsync(requestUri))
        //         from r in localAff<RT, RT, A>(env => env.HttpClientAff (id).IfFail(env), ma)
        //         select r;
    }

    public struct HttpResponseMessageIO : Interfaces.HttpResponseMessageIO
    {
        HttpResponseMessage _message;
        public HttpResponseMessageIO(HttpResponseMessage message)
        {
            _message = message;
        }

        // HttpContent HttpResponseMessage.Content { get; set; }
        // Gets or sets the content of a HTTP response message.
        public HttpContent Content() =>
            _message.Content;

        // HttpResponseMessage HttpResponseMessage.EnsureSuccessStatusCode()
        // Throws an exception if the HttpResponseMessage.IsSuccessStatusCode property for the HTTP response is false.
        // Note: PROBABLY DONT WANT TO DO THIS
        public HttpResponseMessage EnsureSuccessStatusCode() =>
            _message.EnsureSuccessStatusCode();

        // System.Net.HttpStatusCode HttpResponseMessage.StatusCode { get; set; }
        // Gets or sets the status code of the HTTP response.
        public System.Net.HttpStatusCode StatusCode() =>
            _message.StatusCode;
        public Unit SetStatusCode(System.Net.HttpStatusCode statusCode)
        {
            _message.StatusCode = statusCode;
            return unit;
        }


        async Task test()
        {
            var c = new HttpClient();
            var m = await c.GetAsync("");

            // m.Headers
            // System.Net.Http.Headers.HttpResponseHeaders HttpResponseMessage.Headers { get; }
            // Gets the collection of HTTP response headers.

            // m.IsSuccessStatusCode
            // bool HttpResponseMessage.IsSuccessStatusCode { get; }
            // Gets a value that indicates if the HTTP response was successful.

            // m.ReasonPhrase
            // string HttpResponseMessage.ReasonPhrase { get; set; }
            // Gets or sets the reason phrase which typically is sent by servers together with the status code.

            // m.RequestMessage
            // HttpRequestMessage HttpResponseMessage.RequestMessage { get; set; }
            // Gets or sets the request message which led to this response message.


            // m.TrailingHeaders
            // System.Net.Http.Headers.HttpResponseHeaders HttpResponseMessage.TrailingHeaders { get; }
            // Gets the collection of trailing headers included in an HTTP response.

            // m.Version
            // Version HttpResponseMessage.Version { get; set; }
            // Gets or sets the HTTP message version.
        }
    }

    public static class HttpResponseMessage_IO
    {
        public static Eff<RT, HttpContent> content<RT>()
            where RT : struct, HasHttpResponseMessage<RT> =>
                default(RT).HttpResponseMessageEff.Map(e => e.Content());
    }
    public struct HttpContentIO
    {
        HttpContent _content;
        public HttpContentIO(HttpContent content)
        {
            _content = content;
        }

        // (awaitable) Task<Stream> HttpContent.ReadAsStreamAsync()
        // Serialize the HTTP content and return a stream that represents the content as an asynchronous operation.
        public ValueTask<Stream> ReadAsStreamAsync() =>
            _content.ReadAsStreamAsync().ToValue();

        // (awaitable) Task<string> HttpContent.ReadAsStringAsync()
        // Serialize the HTTP content to a string as an asynchronous operation.
        public ValueTask<string> ReadAsStringAsync() =>
            _content.ReadAsStringAsync().ToValue();

        void test()
        {
            // _content.CopyToAsync
            // (awaitable) Task HttpContent.CopyToAsync(Stream stream) (+ 1 overload)
            // Serialize the HTTP content into a stream of bytes and copies it to the stream object provided as the stream parameter.

            // _content.Headers
            // System.Net.Http.Headers.HttpContentHeaders HttpContent.Headers { get; }
            // Gets the HTTP content headers as defined in RFC 2616.

            // _content.LoadIntoBufferAsync
            // (awaitable) Task HttpContent.LoadIntoBufferAsync() (+ 1 overload)
            // Serialize the HTTP content to a memory buffer as an asynchronous operation.

            // _content.ReadAsByteArrayAsync
            // (awaitable) Task<byte[]> HttpContent.ReadAsByteArrayAsync()
            // Serialize the HTTP content to a byte array as an asynchronous operation.
        }
    }
    public static class HttpContent_IO
    {
        public static Aff<RT, string> readAsStringAsync<RT>()
            where RT : struct, HasHttpContent<RT> =>
                default(RT).HttpContentAff.MapAsync(e => e.ReadAsStringAsync());
    }

    public class test
    {
        void Test<RT>()
            where RT : struct, HasHttpClient<RT>, HasHttpResponseMessage<RT>, HasHttpContent<RT>
        {
            // Try to mimic this code:
            // var client = new HttpClient();
            // var res = await client.GetAsync("url");
            // var body = await res.Content.ReadAsStringAsync();
            var a =
                from httpResponseMessage in HttpClient_IO.getAsync<RT>("/url")
                    // This is awkward... Content is a property on the response,
                    //   almost need to put content in a 'context' and 'read' from the context?
                from httpContent in HttpResponseMessage_IO.content<RT>()
                from str in HttpContent_IO.readAsStringAsync<RT>()
                select str;
        }
    }
}
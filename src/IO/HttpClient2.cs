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
        ValueTask<LiveIO.HttpResponseMessageIO> GetAsync(string requestUri);
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
        public ValueTask<HttpResponseMessageIO> GetAsync(string requestUri) =>
            _client.GetAsync(requestUri).Map(r => new HttpResponseMessageIO(r)).ToValue();
    }

    public static class HttpClient_IO
    {
        public static Aff<RT, HttpResponseMessageIO> getAsync<RT>(string requestUri)
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
        public HttpContent Content() =>
            _message.Content;
    }

    public static class HttpResponseMessage_IO
    {
        public static Eff<RT, HttpContent> withContent<RT>(this HttpResponseMessageIO msg)
            where RT : struct, HasHttpResponseMessage<RT> =>
                // from r in localEff<RT, RT, HttpContent>(env => env, ma)
                from r in Eff<RT, HttpContent>(env => msg.Content())
                select r;
    }
    public struct HttpContentIO
    {
        HttpContent _content;
        public HttpContentIO(HttpContent content)
        {
            _content = content;
        }
        public ValueTask<string> ReadAsStringAsync() =>
            _content.ReadAsStringAsync().ToValue();
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
                    // from httpContent in HttpResponseMessage_IO.content<RT>()
                from httpContent in httpResponseMessage.withContent<RT>()
                from str in HttpContent_IO.readAsStringAsync<RT>()
                select str;
        }
    }
}
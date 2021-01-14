using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;
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
        LiveIO.HttpContentIO Content();
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
        public HttpContentIO Content() =>
            new HttpContentIO(_message.Content);
        public System.Net.HttpStatusCode StatusCode() =>
            _message.StatusCode;
        public HashMap<string, IEnumerable<string>> Headers() =>
            _message.Headers.ToHashMap();//.ToDictionary<string, string>(kv => kv.Key, kv => kv.Value);
        // System.Net.Http.Headers.HttpResponseHeaders HttpResponseMessage.Headers { get; }
    }

    public static class HttpResponseMessage_IO
    {
        public static Eff<RT, HttpContentIO> content<RT>(this HttpResponseMessageIO msg)
            where RT : struct, HasHttpResponseMessage<RT> =>
                from r in Eff<RT, HttpContentIO>(env => msg.Content())
                select r;
        // Pure
        public static int getResponseStatusCode(HttpResponseMessageIO msg) =>
            (int)msg.StatusCode();
        public static Option<IEnumerable<string>> getResponseHeader(string header, HttpResponseMessageIO msg) =>
            msg.Headers().Find(header);

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
        public static Aff<RT, string> readAsStringAsync<RT>(this Eff<RT, HttpContentIO> effC)
            where RT : struct, HasHttpContent<RT> =>
                from content in effC.ToAsync()
                from str in Aff<RT, string>(env => content.ReadAsStringAsync())
                select str;
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
            // V1
            // var a =
            //     from httpResponseMessage in HttpClient_IO.getAsync<RT>("/url")
            //         // This is awkward... Content is a property on the response,
            //         //   almost need to put content in a 'context' and 'read' from the context?
            //         // from httpContent in HttpResponseMessage_IO.content<RT>()
            //     from httpContent in httpResponseMessage.withContent<RT>()
            //     from str in httpContent.readAsStringAsync<RT>()
            //     select str;
            // V2
            var a =
                from response in HttpClient_IO.getAsync<RT>("/url")
                from body in response.content<RT>().readAsStringAsync<RT>()
                select body;
        }


        void Test2<RT>()
           where RT : struct, HasHttpClient<RT>, HasHttpResponseMessage<RT>, HasHttpContent<RT>
        {
            // Attempt to mimic the haskell httpclient
            // https://github.com/snoyberg/http-client/blob/master/TUTORIAL.md#request-building

            // response <- httpJSON request
            // putStrLn $ "The status code was: " ++
            //            show (getResponseStatusCode response)
            // print $ getResponseHeader "Content-Type" response
            // S8.putStrLn $ Yaml.encode (getResponseBody response :: Value)

            var a =
                from response in HttpClient_IO.getAsync<RT>("/url")

                    // show (getResponseStatusCode response)
                let statusCode = HttpResponseMessage_IO.getResponseStatusCode(response)

                //getResponseHeader "Content-Type" response
                let contentType = HttpResponseMessage_IO.getResponseHeader("Content-Type", response)

                // S8.putStrLn $ Yaml.encode (getResponseBody response :: Value)
                from body in response.content<RT>().readAsStringAsync<RT>()
                select body;
        }

    }
}
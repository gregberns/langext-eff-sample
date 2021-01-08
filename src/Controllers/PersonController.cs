using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
// using static Logging.Logger;
using Microsoft.Extensions.Logging;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;
using NickDarvey.LanguageExt.AspNetCore;
// using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace LangExtEffSample
{
    public class Utils
    {
        public static Task<IActionResult> ToResult<T>(EitherAsync<Error, T> result)
        {
            return result.ToActionResult(
                Right: c =>
                {
                    // Console.WriteLine($"RES200: {c}");
                    return new ObjectResult(c) { StatusCode = 200 };
                },
                Left: e =>
                {
                    Console.WriteLine($"RES{e.Code}: {e}");
                    return new ObjectResult(e) { StatusCode = e.Code };
                }
            );
        }
        public static Option<string> GetCookie(string key, Microsoft.AspNetCore.Http.IRequestCookieCollection cookies)
        {
            string value = null;
            return cookies.TryGetValue(key, out value)
                ? Some(value)
                : None;
        }
    }


    // [Authorize]
    [ApiController]
    public class PersonController : Controller
    {
        // private readonly Configuration _configuration;
        private string _cookieKey;
        private Func<Microsoft.AspNetCore.Http.IRequestCookieCollection, Eff<Option<string>>> _GetCookie;
        private Func<Microsoft.AspNetCore.Http.IHeaderDictionary, Either<Error, string>> _GetAuthHeader;
        public PersonController()
        {
            _cookieKey = "a_cookie";
            _GetCookie = (cookies) =>
            {
                var cookie = Utils.GetCookie(_cookieKey, cookies);
                Console.WriteLine($"Cookie: {cookie}, Exists: {cookies.ContainsKey(_cookieKey)}");
                return EffMaybe<Option<string>>(() => FinSucc(cookie));
            };
            var authHeader = "Authentication";
            var bearer = "Bearer ";
            _GetAuthHeader = (headers) =>
                headers.Find(kv => kv.Key == authHeader)
                    .ToEither(Error.New($"'{authHeader}' not found."))
                    .Bind<string>(v =>
                        v.Value.ToString().ToLower().StartsWith(bearer.ToLower())
                            ? Right<Error, string>(v.Value)
                            : Left<Error, string>(Error.New("'{bearer}' not in token"))
                        );
        }

        [HttpGet]
        [Route("/person/")]
        public Task<IActionResult> GetPerson()
        {
            // var c = _GetCookie(Request.Cookies);

            var getPerson =
                        from name in Users<UserRuntime>.userName
                        from conn in Users<UserRuntime>.userConnectingString
                        select (name, conn);

            // c.Bind(oc => oc.ToEff(Error.New("Current user not set")))
            // .Bind(cookie =>
            //     Users<UserRuntime>.withUser(cookie, getPerson)
            // );
            var a =
                from cookie in _GetCookie(Request.Cookies).Bind(oc => oc.ToEff(Error.New("Current user not set")))
                from u in Users<UserRuntime>.withUser(cookie, getPerson)
                select u
                ;

            Eff<IActionResult> b =
                from cookie in GetCookie(Request.Cookies).Bind(oc => oc.ToEff(Error.New("Current user not set")))

                select (IActionResult)new ObjectResult(cookie) { StatusCode = 200 };

            // Aff<HttpRuntime, IActionResult> c =
            //     ProcessRequest<HttpRuntime, IActionResult>(b.ToAsync(), Request);




            return Utils.ToResult(new EitherAsync<Error, string> { });
        }

        Eff<Option<string>> GetCookie(Microsoft.AspNetCore.Http.IRequestCookieCollection cookies)
        {
            var cookie = Utils.GetCookie(_cookieKey, cookies);
            Console.WriteLine($"Cookie: {cookie}, Exists: {cookies.ContainsKey(_cookieKey)}");
            return EffMaybe<Option<string>>(() => FinSucc(cookie));
        }

        // Aff<RT, A> ProcessRequest<RT, A>(Aff<RT, A> ma, Microsoft.AspNetCore.Http.HttpRequest request)
        //     where RT : struct, HasCancel<RT>, HasHttp<RT>
        // {
        //     return
        //         // from a in withRequest(request, ma)
        //         from r in localAff<RT, RT, A>(env => env.SetCurrentRequest(request).IfFail(env), ma)
        //             // from c in localAff<RT, RT, A>(env => env.CaptureUserCookie(request.Cookies).IfFail(env), ma)
        //         from cookie in _GetCookie(Request.Cookies).Bind(oc => oc.ToEff(Error.New("Current user not set")))
        //             // from response in Users<UserRuntime>.withUser(cookie, ma)
        //             // select Aff<RT, OkResult>(env => new ValueTask<OkResult>(Task.FromResult(Ok())))
        //             // select Utils.ToResult(new EitherAsync<Error, string> { }).Result;
        //         select a;
        //     ;
        // }

        // public static Aff<RT, A> withRequest<RT, A>(Aff<RT, A> ma)
        //     where RT : struct, HasCancel<RT>, HasHttp<RT> =>
        //         // from _ in default(RT).AssertUserExists(id)
        //         from r in localAff<RT, RT, A>(env => env.SetCurrentRequest(req).IfFail(env), ma)
        //         select r;
    }

    public interface HasHttp<RT>
    {
        Fin<RT> SetCurrentRequest(Microsoft.AspNetCore.Http.HttpRequest id);
        // Fin<RT> CaptureUserCookie(Microsoft.AspNetCore.Http.IRequestCookieCollection cookies);
    }
    public struct HttpRuntime : HasCancel<HttpRuntime>, HasHttp<HttpRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        // readonly Env env;
        readonly Microsoft.AspNetCore.Http.HttpRequest env;

        public CancellationToken CancellationToken { get; }

        public static HttpRuntime New(Microsoft.AspNetCore.Http.HttpRequest env) =>
            new HttpRuntime(env, new CancellationTokenSource());

        HttpRuntime(Microsoft.AspNetCore.Http.HttpRequest env, CancellationTokenSource cancellationTokenSource) =>
            (this.env, this.cancellationTokenSource, CancellationToken) =
                (env, cancellationTokenSource, cancellationTokenSource.Token);

        public HttpRuntime LocalCancel =>
            new HttpRuntime(env, new CancellationTokenSource());

        public Eff<HttpRuntime, CancellationTokenSource> CancellationTokenSource =>
            Eff<HttpRuntime, CancellationTokenSource>(env => env.cancellationTokenSource);

        // Fin<RT> CaptureUserCookie(Microsoft.AspNetCore.Http.IRequestCookieCollection cookies);
        // Fin<RT> SetCurrentRequest(Microsoft.AspNetCore.Http.HttpRequest id);
        public Fin<HttpRuntime> SetCurrentRequest(Microsoft.AspNetCore.Http.HttpRequest req) =>
            FinSucc(new HttpRuntime(req, cancellationTokenSource));
        // env.Users.ContainsKey(id)
        //     ? FinSucc(new HttpRuntime(env.With(User: env.Users[id]), cancellationTokenSource))
        //     : FinFail<HttpRuntime>(Error.New("Invalid user ID"));

        public Eff<HttpRuntime, Microsoft.AspNetCore.Http.HttpRequest> CurrentRequest =>
            EffMaybe<HttpRuntime, Microsoft.AspNetCore.Http.HttpRequest>(
                env => env.env);
        //    .Match(
        //        Some: FinSucc,
        //        None: FinFail<Microsoft.AspNetCore.Http.HttpRequest>(Error.New("No Current Request"))));

        public Eff<HttpRuntime, Option<string>> GetCookie(string cookieKey) =>
            EffMaybe<HttpRuntime, Option<string>>(env =>
                 FinSucc(Utils.GetCookie(cookieKey, env.env.Cookies)));


        // public Eff<HttpRuntime, Unit> AssertUserExists(string id) =>
        //     EffMaybe<HttpRuntime, Unit>(
        //         rt => rt.env.Users.ContainsKey(id)
        //                   ? FinSucc(unit)
        //                   : FinFail<Unit>(Error.New("User doesn't exist")));
    }
}
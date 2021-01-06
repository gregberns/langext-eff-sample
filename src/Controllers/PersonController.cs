using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
// using static Logging.Logger;
using Microsoft.Extensions.Logging;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
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
    }

    // [Authorize]
    [ApiController]
    public class CartController : Controller
    {
        // private readonly Configuration _configuration;
        private string _cookieKey;
        private Func<Microsoft.AspNetCore.Http.IRequestCookieCollection, Option<Guid>> _GetCookie;
        private Func<Microsoft.AspNetCore.Http.IHeaderDictionary, Either<Error, string>> _GetAuthHeader;
        public CartController()
        // (ICartDataLayer dataLayer)
        {
            // CheckoutHandler.DataStore = new AccountStore();
            // CheckoutHandler.DataStore = dataLayer;

            // _cookieKey = "issa_cookie";
            // _GetCookie = (cookies) =>
            // {
            //     // Cleanup
            //     string value = null;
            //     var exists = cookies.TryGetValue(_cookieKey, out value);

            //     var cookie = Utils.GetCookie(_cookieKey, cookies).Bind(parseGuid);
            //     Console.WriteLine($"Cookie: {cookie}, Exists: {exists}, Guid: {parseGuid(value)}");
            //     return cookie;
            // };
            // var authHeader = "Authentication";
            // var bearer = "Bearer ";
            // _GetAuthHeader = (headers) =>
            //     headers.Find(kv => kv.Key == authHeader)
            //         .ToEither(Error.New($"'{authHeader}' not found."))
            //         .Bind<string>(v =>
            //             v.Value.ToString().ToLower().StartsWith(bearer.ToLower())
            //                 ? Right<Error, string>(v.Value)
            //                 : Left<Error, string>(Error.New("'{bearer}' not in token"))
            //             );
        }

        // [HttpGet]
        // [Route("/cart/")]
        // public Task<IActionResult> GetCart() =>
        //     Utils.ToResult(CartHandler.GetCart(_GetCookie(Request.Cookies)));

        // [HttpPost]
        // [Route("/cart/")]
        // public Task<IActionResult> AddItems(System.Collections.Generic.List<Person> lineItems) =>
        //     Utils.ToResult(CartHandler.AddItems(_GetCookie(Request.Cookies), lineItems.Freeze()));

        // [HttpDelete]
        // [Route("/cart/")]
        // public Task<IActionResult> RemoveItem(Person lineItem) =>
        //     Utils.ToResult(CartHandler.RemoveItem(_GetCookie(Request.Cookies), lineItem));

    }
}
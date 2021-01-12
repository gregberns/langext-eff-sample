using System;
using System.Net.Http;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

namespace LangExtEffSample
{
    public interface HasGithubCredentials<RT>
        where RT : struct
    {
        GithubCredentials GithubCredentials { get; }
    }
    public static class Github
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static Aff<RT, Lst<GithubOrg>> GetUserOrgs<RT>(string username)
            where RT : struct, HasCancel<RT>, HasHttpClient<RT>, HasJson<RT>, HasGithubCredentials<RT>
        {
            // Auth isn't required unless you want to make a lot of calls
            // var authHeader = $"Basic {Base64Encode($"{credentials.Username}:{credentials.Password}")}";

            // var request = new HttpRequestMessage
            // {
            //     RequestUri = new Uri($"https://api.github.com/users/{username}/orgs"),
            //     Method = HttpMethod.Get,
            //     Headers = {
            //         { "Authorization", $"{authHeader}" },
            //         { "User-Agent",  "curl/7.33.0" },
            //         { "Accept", "application/vnd.github.v3+json" }
            //     },
            // };

            return
                from credentials in Eff<RT, GithubCredentials>(env => env.GithubCredentials)
                let authHeader = $"Basic {Base64Encode($"{credentials.Username}:{credentials.Password}")}"
                let request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://api.github.com/users/{username}/orgs"),
                    Method = HttpMethod.Get,
                    Headers = {
                        { "Authorization", $"{authHeader}" },
                        { "User-Agent",  "curl/7.33.0" },
                        { "Accept", "application/vnd.github.v3+json" }
                    },
                }
                from res in HttpClientAff<RT>.sendRequest(request)
                from _ in
                    (res.StatusCode == 200
                        ? SuccessAff<HttpResponse>(res)
                        : FailAff<HttpResponse>(Error.New($"Github Auth failed. StatusCode: {res.StatusCode}. Body: {res.Body}")))
                from b in JsonEff<RT>.deserialize<List<GithubOrg>>(res.Body)
                select b.Freeze();
        }
    }
    public class GithubCredentials : Record<GithubCredentials>
    {
        public readonly string Username;
        public readonly string Password;
        public GithubCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
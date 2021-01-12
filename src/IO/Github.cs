using System;
using System.Net.Http;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

namespace LangExtEffSample
{
    public interface GithubIO
    {
        EitherAsync<Error, Lst<GithubOrg>> GetUserOrgs(string username);
        Aff<RT, Lst<GithubOrg>> GetUserOrgs<RT>(string username)
            // where RT : struct, HasCancel<RT>;
            where RT : struct, HasCancel<RT>, HasHttpClient<RT>, HasJson<RT>;
    }

    public interface HasGithub<RT>
        where RT : struct, HasCancel<RT>
    {
        Aff<RT, GithubIO> AffGithub();
        string GithubUser { get; }
        string GithubToken { get; }
    }

    public class LiveGithubIO : GithubIO
    {
        readonly HttpClientIO _httpClient;
        readonly JsonIO _json;
        public string _githubUser;
        public string _githubToken;
        public LiveGithubIO(HttpClientIO httpClient, JsonIO json, string githubUser, string githubToken)
        {
            (_httpClient, _json, _githubUser, _githubToken)
                = (httpClient, json, githubUser, githubToken);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public EitherAsync<Error, Lst<GithubOrg>> GetUserOrgs(string username)
        {
            // This isn't required unless you want to make a lot of calls
            var authHeader = $"Basic {Base64Encode($"{_githubUser}:{_githubToken}")}";

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://api.github.com/users/{username}/orgs"),
                Method = HttpMethod.Get,
                Headers = {
                    { "Authorization", $"{authHeader}" },
                    { "User-Agent",  "curl/7.33.0" },
                    { "Accept", "application/vnd.github.v3+json" }
                },
            };

            return _httpClient.HttpRequest(request)
                .Bind<HttpResponse>(res =>
                    res.StatusCode == 200 ? RightAsync<Error, HttpResponse>(res)
                    : LeftAsync<Error, HttpResponse>(Error.New($"Github Auth failed. StatusCode: {res.StatusCode}. Body: {res.Body}")))
                .Bind<List<GithubOrg>>(res => _json.Deserialize<List<GithubOrg>>(res.Body))
                // .Bind<List<GithubOrg>>(res => JsonEff<RT>.deserialize<List<GithubOrg>>(res.Body))
                .Map(ghres => ghres.Freeze());
        }

        public Aff<RT, Lst<GithubOrg>> GetUserOrgs<RT>(string username)
            where RT : struct, HasCancel<RT>, HasHttpClient<RT>, HasJson<RT>
        {
            // This isn't required unless you want to make a lot of calls
            var authHeader = $"Basic {Base64Encode($"{_githubUser}:{_githubToken}")}";

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://api.github.com/users/{username}/orgs"),
                Method = HttpMethod.Get,
                Headers = {
                    { "Authorization", $"{authHeader}" },
                    { "User-Agent",  "curl/7.33.0" },
                    { "Accept", "application/vnd.github.v3+json" }
                },
            };

            return
                from res in HttpClientAff<RT>.sendRequest(request)
                from _ in
                    (res.StatusCode == 200
                        ? SuccessAff<HttpResponse>(res)
                        : FailAff<HttpResponse>(Error.New($"Github Auth failed. StatusCode: {res.StatusCode}. Body: {res.Body}")))
                from b in JsonEff<RT>.deserialize<List<GithubOrg>>(res.Body)
                select b.Freeze();
        }
    }
    public class GithubOrg : Record<GithubOrg>
    {
        public string login { get; set; }
        public string id { get; set; }
        public string node_id { get; set; }
        public string url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string hooks_url { get; set; }
        public string issues_url { get; set; }
        public string members_url { get; set; }
        public string public_members_url { get; set; }
        public string avatar_url { get; set; }
        public string description { get; set; }
    }
}

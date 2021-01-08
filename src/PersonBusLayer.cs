using System;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Interfaces;
using static LanguageExt.Prelude;

namespace LangExtEffSample
{
    public struct SomeRuntime : HasCancel<SomeRuntime>, HasGithub<SomeRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken { get; }
        public static SomeRuntime New(string githubUser, string githubToken) =>
            new SomeRuntime(githubUser, githubToken, new CancellationTokenSource());
        SomeRuntime(string githubUser, string githubToken, CancellationTokenSource cancellationTokenSource) =>
            (_githubUser, _githubToken, this.cancellationTokenSource, CancellationToken) =
               (githubUser, githubToken, cancellationTokenSource, cancellationTokenSource.Token);
        public SomeRuntime LocalCancel =>
            new SomeRuntime(_githubUser, _githubToken, new CancellationTokenSource());
        public Eff<SomeRuntime, CancellationTokenSource> CancellationTokenSource =>
            Eff<SomeRuntime, CancellationTokenSource>(env => env.cancellationTokenSource);


        public Eff<SomeRuntime, HttpClientIO> EffHttpClient =>
            Eff<SomeRuntime, HttpClientIO>(env => new LiveHttpClientIO());
        public Aff<SomeRuntime, HttpClientIO> AffHttpClient =>
            EffHttpClient.ToAsync();

        public Eff<SomeRuntime, JsonIO> EffJson =>
            Eff<SomeRuntime, JsonIO>(env => new LiveJsonIO());
        public Aff<SomeRuntime, JsonIO> AffJson =>
            EffJson.ToAsync();

        public string _githubUser;
        public string _githubToken;
        public string GithubUser => _githubUser;
        public string GithubToken => _githubToken;

        public Aff<SomeRuntime, GithubIO> AffGithub()
        {
            var user = GithubUser;
            var token = GithubToken;
            return Combine<SomeRuntime, HttpClientIO, JsonIO, GithubIO>(
                EffHttpClient, EffJson, (http, json) => (GithubIO)new LiveGithubIO(http, json, user, token));
        }

        // I can't do the following in AffGithub, so have implemented this (Pretty sure this is already a function, but don't remember its name)
        //      from http in EffHttpClient
        //      from json in EffJson
        //      select (GithubIO)new LiveGithubIO(http, json);
        public static Aff<RT, C> Combine<RT, A, B, C>(Aff<RT, A> a, Aff<RT, B> b, Func<A, B, C> f)
            where RT : struct, HasCancel<RT> =>
            from aa in a
            from bb in b
            select f(aa, bb);
    }

    public class PersonBusLayer
    {
        public static Aff<RT, Lst<GithubOrg>> GetUserOrgs<RT>(string username)
            where RT : struct, HasCancel<RT>, HasGithub<RT> =>
                // from authToken in default(RT).AffGithub.Bind(gh =>
                //         AffMaybe<RT, string>(
                //             env =>
                //                 gh.Auth()
                //                     .Match(
                //                         Right: r => FinSucc<string>(r),
                //                         Left: e => FinFail<string>(e))
                //                     .ToValue()))
                from orgs in default(RT).AffGithub().Bind(gh =>
                    AffMaybe<RT, Lst<GithubOrg>>(
                        env =>
                            gh.GetUserOrgs(username)
                                .Match(
                                    Right: r => FinSucc<Lst<GithubOrg>>(r),
                                    Left: e => FinFail<Lst<GithubOrg>>(e))
                                .ToValue()))
                select orgs;

    }
}

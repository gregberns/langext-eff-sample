using System;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Interfaces;
using static LanguageExt.Prelude;

namespace LangExtEffSample
{
    public class PersonBusLayerV2
    {
        public static Aff<RT, Lst<GithubOrg>> GetUserOrgs<RT>(string username)
            where RT : struct, HasCancel<RT>, HasHttpClient<RT>, HasJson<RT>, HasGithubCredentials<RT>
        {
            return
                from orgs in Github.GetUserOrgs<RT>(username)
                select orgs;
        }
    }
    public struct SomeRuntimeV2 :
        HasCancel<SomeRuntimeV2>,
        HasHttpClient<SomeRuntimeV2>,
        HasJson<SomeRuntimeV2>,
        HasGithubCredentials<SomeRuntimeV2>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken { get; }
        public static SomeRuntimeV2 New(GithubCredentials githubCredentials) =>
            new SomeRuntimeV2(githubCredentials, new CancellationTokenSource());
        SomeRuntimeV2(GithubCredentials githubCredentials, CancellationTokenSource cancellationTokenSource) =>
            (_githubCredentials, this.cancellationTokenSource, CancellationToken) =
               (githubCredentials, cancellationTokenSource, cancellationTokenSource.Token);
        public SomeRuntimeV2 LocalCancel =>
            new SomeRuntimeV2(_githubCredentials, new CancellationTokenSource());
        public Eff<SomeRuntimeV2, CancellationTokenSource> CancellationTokenSource =>
            Eff<SomeRuntimeV2, CancellationTokenSource>(env => env.cancellationTokenSource);

        public Eff<SomeRuntimeV2, HttpClientIO> EffHttpClient =>
            Eff<SomeRuntimeV2, HttpClientIO>(env => new LiveHttpClientIO());
        public Aff<SomeRuntimeV2, HttpClientIO> AffHttpClient =>
            EffHttpClient.ToAsync();

        public Eff<SomeRuntimeV2, JsonIO> EffJson =>
            Eff<SomeRuntimeV2, JsonIO>(env => new LiveJsonIO());
        // public Aff<SomeRuntimeV2, JsonIO> AffJson =>
        //     EffJson.ToAsync();

        GithubCredentials _githubCredentials;
        public GithubCredentials GithubCredentials => _githubCredentials;
    }
}

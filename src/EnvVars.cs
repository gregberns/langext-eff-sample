using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

namespace LangExtEffSample
{
    public static class AffExt
    {
        public static Aff<Env, D> Apply<Env, A, B, C, D>(this (Aff<Env, A>, Aff<Env, B>, Aff<Env, C>) ms, Func<A, B, C, D> apply)
            where Env : struct, HasCancel<Env> =>
                AffMaybe<Env, D>(async env =>
                    {
                        var t1 = ms.Item1.RunIO(env).AsTask();
                        var t2 = ms.Item2.RunIO(env).AsTask();
                        var t3 = ms.Item3.RunIO(env).AsTask();

                        var tasks = new Task[] { t1, t2, t3 };
                        await Task.WhenAll(tasks);
                        return (t1.Result.ToValid(), t2.Result.ToValid(), t3.Result.ToValid())
                                    .Apply(apply)
                                    .Match(Succ: FinSucc, Fail: ErrorAggregate<D>);
                    });

        static Validation<Error, A> ToValid<A>(this Fin<A> ma) =>
            ma.Match(Succ: Success<Error, A>, Fail: Fail<Error, A>);

        static Fin<A> ErrorAggregate<A>(Seq<Error> errs) =>
            FinFail<A>(Error.New(new AggregateException(errs.Map(e => (Exception)e))));
    }

    public static class EnvVars
    {
        public static Eff<RT, System.Collections.IDictionary> getEnvironmentVariables<RT>()
            where RT : struct, HasEnvironment<RT> =>
                from envvars in default(RT).EnvironmentEff.Map(e => e.GetEnvironmentVariables())
                select envvars;
        public static Eff<RT, Unit> setEnvironmentVariable<RT>(string key, string value)
            where RT : struct, HasEnvironment<RT> =>
                from _ in default(RT).EnvironmentEff.Map(e => e.SetEnvironmentVariable(key, value))
                select unit;

        public static Eff<RT, Lst<Option<(K, V)>>> Lookup<RT, K, V>(System.Collections.IDictionary dict, Lst<K> ms) =>
            Eff<RT, Lst<Option<(K, V)>>>(env =>
                ms.Map(m =>
                    dict.Contains(m)
                        // This cast could fail
                        ? Some<(K, V)>(((K)m, (V)dict[m]))
                        : Option<(K, V)>.None)
            );
        public static Eff<RT, V> Lookup<RT, K, V>(System.Collections.IDictionary dict, K m) =>
            // Eff<RT, V>(env =>
            dict.Contains(m)
                // This cast could fail
                // ? Some<(K, V)>(((K)m, (V)dict[m]))
                ? SuccessEff((V)dict[m])
                // : Option<(K, V)>.None
                : FailEff<V>(Error.New($"Not found: {m}"))
            // );
            ;





    }

    // Objective:
    // Provide the same(similar) functionality as this:
    // https://github.com/gregberns/LanguageExtCommon/blob/main/LanguageExtCommon/Environment.cs#L11
    public struct LiveEnvVarsIO
    {
        /// <summary>
        /// Get Environment Variable
        /// </summary>
        // public Validation<string, string> GetEnv(string env)
        public string GetEnv(string env)
        {
            var val = System.Environment.GetEnvironmentVariable(env);
            return String.IsNullOrWhiteSpace(val)
                // ? Fail<string, string>($"Invalid environmental variable: {env}")
                // ? throw new Exception($"Invalid environmental variable: '{env}'")
                ? throw new Exception($"Invalid environmental variable: '{env}'")
                // : Success<string, string>(val);
                : val;
        }

        // /// <summary>
        // /// Get Environment Variable of type integer
        // /// </summary>
        // public Validation<string, int> GetEnvInt(string env) =>
        //     GetEnv(env).Bind<int>(
        //         i => parseInt(i)
        //             .ToEither($"Invalid environmental integer: {env} => {i}")
        //             .ToValidation()
        //     );

        // /// <summary>
        // /// Get Environment Variable of type boolean
        // /// </summary>
        // public Validation<string, bool> GetEnvBool(string env) =>
        //     GetEnv(env).Bind<bool>(
        //         b => parseBool(b)
        //             .ToEither($"Invalid environmental boolean: {env} => {b}")
        //             .ToValidation()
        //     );

        // /// <summary>
        // /// Get Environment Variable of type Uri (System.UriKind.RelativeOrAbsolute)
        // /// </summary>
        // public Validation<string, Uri> GetEnvUri(string env) =>
        //     GetEnv(env)
        //         .Bind<Uri>(v =>
        //             ParseUri(UriKind.RelativeOrAbsolute, v)
        //                 .Match(
        //                     Some: res => Success<string, Uri>(res),
        //                     None: () => Fail<string, Uri>($"Invalid environmental variable. Not a valid Uri. EnvVar: '{env}', Value: '{v}'")
        //                 ));

        // /// <summary>
        // /// Get Environment Variable of type Absolute Uri (System.UriKind.Absolute)
        // /// </summary>
        // public Validation<string, Uri> GetEnvUriAbs(string env) =>
        //     GetEnv(env)
        //         .Bind<Uri>(v =>
        //             ParseUri(UriKind.RelativeOrAbsolute, v)
        //                 .Match(
        //                     Some: res => Success<string, Uri>(res),
        //                     None: () => Fail<string, Uri>($"Invalid environmental variable. Not a valid Absolute Uri. EnvVar: '{env}', Value: '{v}'")
        //                 ));

        // /// <summary>
        // /// Get Environment Variable of type Relative Uri (System.UriKind.Relative
        // /// </summary>
        // public Validation<string, Uri> GetEnvUriRel(string env) =>
        //     GetEnv(env)
        //         .Bind<Uri>(v =>
        //             ParseUri(UriKind.RelativeOrAbsolute, v)
        //                 .Match(
        //                     Some: res => Success<string, Uri>(res),
        //                     None: () => Fail<string, Uri>($"Invalid environmental variable. Not a valid Relative Uri. EnvVar: '{env}', Value: '{v}'")
        //                 ));

        // static Option<Uri> ParseUri(UriKind kind, string uri)
        // {
        //     Uri result = null;
        //     return Uri.TryCreate(uri, kind, out result)
        //         ? Some<Uri>(result)
        //         : Option<Uri>.None;
        // }

        // public static Validation<string, Configuration> GetConfig() =>
        //     (
        //         GetEnvUriAbs("ICOLOR_API_URL"),
        //         GetEnv("ICOLOR_API_AUTH_USERNAME"),
        //         GetEnv("ICOLOR_API_AUTH_PASSWORD"),
        //         GetEnv("ICOLOR_API_AUTH_SUBSCRIBERID"),
        //         GetEnv("ICOLOR_API_AUTH_CONSUMERID"),
        //         GetEnvInt("ICOLOR_API_PAGE_SIZE"),
        //         GetEnv("AZURE_STORAGE_CONNECTION_STRING"),
        //         GetEnv("AZURE_CONTAINER_NAME")
        //     ).Apply(
        //         (a, b, c, d, e, f, g, h) => new Configuration
        //         {
        //             IColorUrl = a,
        //             IColorUsername = b,
        //             IColorPassword = c,
        //             IColorSubscriberId = d,
        //             IColorConsumerId = e,
        //             IColorPageSize = f,
        //             AzureStorageConnectionString = g,
        //             AzureContainerName = h,
        //             StartDate = DateTime.Today.AddDays(-1),
        //             EndDate = DateTime.Today,
        //         }
        //     );
    }
}
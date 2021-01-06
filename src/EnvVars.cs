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

using Dapper;
using Microsoft.Data.SqlClient;

namespace LangExtEffSample
{
    // specifies the behaviour of IO sub-system
    public interface EnvVarsIO
    {
        string GetEnv(string env);
        // Validation<string, int> GetEnvInt(string env);
        // Validation<string, bool> GetEnvBool(string env);
        // Validation<string, Uri> GetEnvUri(string env);
        // Validation<string, Uri> GetEnvUriAbs(string env);
        // Validation<string, Uri> GetEnvUriRel(string env);
    }
    public interface HasEnvVars<RT> where RT : struct
    {
        Eff<RT, EnvVarsIO> EffEnvVars { get; }
    }
    public static class EnvVarsEff<RT>
        where RT : struct, HasEnvVars<RT>
    {
        public static Eff<RT, string> getEnv(string env) =>
            default(RT).EffEnvVars.Map(p => p.GetEnv(env));
    }

    // Objective:
    // Provide the same(similar) functionality as this:
    // https://github.com/gregberns/LanguageExtCommon/blob/main/LanguageExtCommon/Environment.cs#L11
    public struct LiveEnvVarsIO : EnvVarsIO
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
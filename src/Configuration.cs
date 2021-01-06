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
    public class Configuration
    {
        public readonly string ConnectionString;
        public Configuration(
            string connectionString
        ) =>
            (ConnectionString) =
                (connectionString);


    }
    public static class ConfigurationStore
    {
        static readonly Atom<Option<Configuration>> configMap = Atom(Option<Configuration>.None);

        public static Eff<Unit> SetConfig(Configuration config) =>
            Eff(() => ignore(configMap.Swap(_ => config)));

        static Eff<A> NotInitialised<A>() =>
            FailEff<A>(Error.New("Configuration not initialised"));

        public static Eff<string> ConnectionString =>
            configMap.Value
                     .Map(c => c.ConnectionString)
                     .Match(SuccessEff, NotInitialised<string>);
    }
}

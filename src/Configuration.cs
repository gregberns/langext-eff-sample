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

        public static Eff<RT, Unit> SetConfig<RT>(Configuration config) =>
            Eff(() => ignore(configMap.Swap(_ => config)));

        public static Configuration configOrThrow() =>
            configMap.Value.IfNone(() => throw new InvalidOperationException("ConfigurationStore not initialized"));
    }
}

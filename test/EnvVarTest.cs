using System;
using Xunit;
using System.Threading;
using LangExtEffSample;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

namespace LangExtEffSample.Test
{
    public class TestConfiguration
    {
        public string Abc;
        public string Def;
    }

    public struct EnvRuntime : HasCancel<EnvRuntime>, HasEnvironment<EnvRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken { get; }
        public static EnvRuntime New() =>
            new EnvRuntime(new CancellationTokenSource());
        EnvRuntime(CancellationTokenSource cancellationTokenSource) =>
            (this.cancellationTokenSource, CancellationToken) =
                (cancellationTokenSource, cancellationTokenSource.Token);
        public EnvRuntime LocalCancel =>
            new EnvRuntime(new CancellationTokenSource());
        public Eff<EnvRuntime, CancellationTokenSource> CancellationTokenSource =>
            Eff<EnvRuntime, CancellationTokenSource>(env => env.cancellationTokenSource);

        public Eff<EnvRuntime, EnvironmentIO> EnvironmentEff =>
            Eff<EnvRuntime, LanguageExt.Interfaces.EnvironmentIO>(env => new LanguageExt.LiveIO.EnvironmentIO());
        public Aff<EnvRuntime, EnvironmentIO> EnvironmentAff =>
            EnvironmentEff.ToAsync();
    }

    public class EnvVarTest
    {
        [Fact]
        public void tryit()
        {
            var doit =
                from __1 in EnvVars.setEnvironmentVariable<EnvRuntime>("ABC", "123")
                from __2 in EnvVars.setEnvironmentVariable<EnvRuntime>("DEF", "234")
                from envs in EnvVars.getEnvironmentVariables<EnvRuntime>()
                from a in EnvVars.Lookup<EnvRuntime, string, string>(envs, "ABC")
                from d in EnvVars.Lookup<EnvRuntime, string, string>(envs, "DEF")
                select new
                {
                    Abc = a,
                    Def = d
                };

            var res = doit.RunIO(EnvRuntime.New()).ThrowIfFail();

            Assert.Equal("123", res.Abc);
            Assert.Equal("234", res.Def);
        }

        // [Fact]
        // public void MultipleEnvVar()
        // {
        //     System.Environment.SetEnvironmentVariable("abc", "123");
        //     System.Environment.SetEnvironmentVariable("def", "456");

        //     var conf =
        //         GetConfig<ConfigRuntime>()
        //             .RunIO(new ConfigRuntime())
        //             .ThrowIfFail();

        //     Assert.Equal("123", conf.Abc);
        //     Assert.Equal("456", conf.Def);

        //     Eff<RT, TestConfiguration> GetConfig<RT>()
        //         where RT : struct, HasEnvVars<RT> =>
        //             from abc in EnvVarsEff<RT>.getEnv("abc")
        //             from def in EnvVarsEff<RT>.getEnv("def")
        //             select new TestConfiguration
        //             {
        //                 Abc = abc,
        //                 Def = def,
        //             };
        // }

        // [Fact]
        // public void ApplyMultipleEnvVar()
        // {
        //     System.Environment.SetEnvironmentVariable("abc", "123");
        //     System.Environment.SetEnvironmentVariable("def", "456");

        //     var conf =
        //         GetConfig<ConfigRuntime>()
        //             .RunIO(new ConfigRuntime())
        //             .ThrowIfFail();

        //     Assert.Equal("123", conf.Abc);
        //     Assert.Equal("456", conf.Def);

        //     Eff<RT, TestConfiguration> GetConfig<RT>()
        //         where RT : struct, HasEnvVars<RT> =>
        //             // How do we do this as an Applicative like Validation
        //             from abc in EnvVarsEff<RT>.getEnv("abc")
        //             from def in EnvVarsEff<RT>.getEnv("def")
        //             select new TestConfiguration
        //             {
        //                 Abc = abc,
        //                 Def = def,
        //             };
        //     // EnvVarsEff<RT>.getEnv("abc")
        //     // .Apply()

        //     // (
        //     //     EnvVarsEff<RT>.getEnv("abc"),
        //     //     EnvVarsEff<RT>.getEnv("def")
        //     // )
        //     // .Apply(
        //     //     (a, b) =>
        //     //         new TestConfiguration
        //     //         {
        //     //             Abc = abc,
        //     //             Def = def,
        //     //         }
        //     // );
        // }

        // [Fact]
        // public void StringEnvVar()
        // {
        //     System.Environment.SetEnvironmentVariable("abc", "123");

        //     var abc =
        //         EnvVarsEff<ConfigRuntime>.getEnv("abc")
        //             .RunIO(new ConfigRuntime())
        //             .ThrowIfFail();

        //     Assert.Equal("123", abc);
        // }

        // [Fact]
        // public void FailEnvVar()
        // {
        //     var config =
        //         EnvVarsEff<ConfigRuntime>.getEnv("asdf")
        //             .RunIO(new ConfigRuntime());

        //     Assert.True(config.IsFail);
        //     Assert.Equal("Invalid environmental variable: 'asdf'", config.IfFail(e => e.Message));
        // }
    }
}

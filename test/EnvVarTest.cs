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

    public class EnvVarTest
    {
        [Fact]
        public void MultipleEnvVar()
        {
            System.Environment.SetEnvironmentVariable("abc", "123");
            System.Environment.SetEnvironmentVariable("def", "456");

            var conf =
                GetConfig<ConfigRuntime>()
                    .RunIO(new ConfigRuntime())
                    .ThrowIfFail();

            Assert.Equal("123", conf.Abc);
            Assert.Equal("456", conf.Def);

            Eff<RT, TestConfiguration> GetConfig<RT>()
                where RT : struct, HasEnvVars<RT> =>
                    from abc in EnvVarsEff<RT>.getEnv("abc")
                    from def in EnvVarsEff<RT>.getEnv("def")
                    select new TestConfiguration
                    {
                        Abc = abc,
                        Def = def,
                    };
        }

        [Fact]
        public void ApplyMultipleEnvVar()
        {
            System.Environment.SetEnvironmentVariable("abc", "123");
            System.Environment.SetEnvironmentVariable("def", "456");

            var conf =
                GetConfig<ConfigRuntime>()
                    .RunIO(new ConfigRuntime())
                    .ThrowIfFail();

            Assert.Equal("123", conf.Abc);
            Assert.Equal("456", conf.Def);

            Eff<RT, TestConfiguration> GetConfig<RT>()
                where RT : struct, HasEnvVars<RT> =>
                    // How do we do this as an Applicative like Validation
                    from abc in EnvVarsEff<RT>.getEnv("abc")
                    from def in EnvVarsEff<RT>.getEnv("def")
                    select new TestConfiguration
                    {
                        Abc = abc,
                        Def = def,
                    };
            // EnvVarsEff<RT>.getEnv("abc")
            // .Apply()

            // (
            //     EnvVarsEff<RT>.getEnv("abc"),
            //     EnvVarsEff<RT>.getEnv("def")
            // )
            // .Apply(
            //     (a, b) =>
            //         new TestConfiguration
            //         {
            //             Abc = abc,
            //             Def = def,
            //         }
            // );
        }

        [Fact]
        public void StringEnvVar()
        {
            System.Environment.SetEnvironmentVariable("abc", "123");

            var abc =
                EnvVarsEff<ConfigRuntime>.getEnv("abc")
                    .RunIO(new ConfigRuntime())
                    .ThrowIfFail();

            Assert.Equal("123", abc);
        }

        [Fact]
        public void FailEnvVar()
        {
            var config =
                EnvVarsEff<ConfigRuntime>.getEnv("asdf")
                    .RunIO(new ConfigRuntime());

            Assert.True(config.IsFail);
            Assert.Equal("Invalid environmental variable: 'asdf'", config.IfFail(e => e.Message));
        }
    }
}

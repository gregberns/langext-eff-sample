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
            Assert.Equal("Invalid environmental variable: asdf", config.IfFail(e => e.Message));
        }
    }
}

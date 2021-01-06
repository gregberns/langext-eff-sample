using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LangExtEffSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Get the envvars to run - could use a runtime
            //  * Startup Runtime??
            // var configRt = new ConfigRuntime();
            var config =
                GetConfig<ConfigRuntime>()
                    .RunIO(new ConfigRuntime())
                    .ThrowIfFail();


            // Web layer

            // BusLog layer

            // Data Layer
            //    * Start here



            var serverPort = 8080;

            new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{serverPort}")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        public static Eff<RT, Configuration> GetConfig<RT>()
            where RT : struct, HasEnvVars<RT> =>
                from abc in EnvVarsEff<RT>.getEnv("abc")
                select new Configuration(abc);

    }
}

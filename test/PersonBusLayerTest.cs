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
    public class PersonBusLayerTest
    {
        public static string ConnectionString = "Data Source=localhost;Initial Catalog=people_db;User ID=sa;Password=Pass@word";
        public PersonBusLayerTest()
        {
            Helper.ResetDb(ConnectionString).Wait();
        }

        [Fact]
        public async void GetOrgs()
        {
            var githubUser = "gregberns";
            var githubToken = "<NOT NEEDED>";

            var getOrgs =
                from orgs in PersonBusLayer.GetUserOrgs<SomeRuntime>("gregberns")
                select orgs;

            var env = SomeRuntime.New(githubUser, githubToken);
            var os = await getOrgs.RunIO(env);

            var res = os.ThrowIfFail();
            Assert.Equal(1, res.Count());
            Assert.Equal("ISSAOnline", res.Head().login);
        }
    }
}

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
    public class DataLayerE2ETest
    {
        public static string ConnectionString = "Data Source=localhost;Initial Catalog=people_db;User ID=sa;Password=Pass@word";
        public DataLayerE2ETest()
        {
            Helper.ResetDb(ConnectionString).Wait();
        }

        [Fact]
        public async void CreatePerson()
        {
            var createPerson = new Person()
            {
                Id = 1,
                Name = "John Doe",
                Age = 62,
            };

            var connectionString = "Data Source=localhost;Initial Catalog=people_db;User ID=sa;Password=Pass@word";
            var env = new DataLayerRuntime(new CancellationTokenSource(), connectionString);

            var CreateNewPerson = DataLayer.CreatePerson<DataLayerRuntime>(createPerson);

            var b = await CreateNewPerson.RunIO(env);

            Assert.Equal(createPerson, b.ThrowIfFail());
            // Assert.True(b.IsSucc);

        }
    }
}

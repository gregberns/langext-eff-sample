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

            var CreateNewPerson =
                // from _ in DataLayer.SetConfig<DataLayerRuntime>(new Configuration(ConnectionString))
                from person in DataLayer.CreatePerson<DataLayerRuntime>(createPerson)
                select person;

            var env = DataLayerRuntime.New(ConnectionString);
            var b = await CreateNewPerson.RunIO(env);

            Assert.Equal(createPerson, b.ThrowIfFail());
            // Assert.True(b.IsSucc);

        }
    }
}

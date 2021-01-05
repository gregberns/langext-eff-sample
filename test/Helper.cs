using System;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using LangExtEffSample;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

using Dapper;
using Microsoft.Data.SqlClient;

namespace LangExtEffSample.Test
{
    public class Helper
    {
        public static async Task ResetDb(string connectionString)
        {
            var sql = @"
                TRUNCATE TABLE persons
                DBCC CHECKIDENT ('persons', RESEED, 1)
            ";

            var _ = await Execute(connectionString, sql);
        }

        public static async Task<int> Execute(string connectionString, string sql)
        {
            using (var c = new SqlConnection(connectionString))
            {
                return await c.ExecuteAsync(sql);
            }
        }

    }
}
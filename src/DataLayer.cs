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
    // specifies the behaviour of IO sub-system
    public interface SqlDbIO
    {
        // ExecuteAsync
        // Execute a command asynchronously using Task. Returns: The number of rows affected.
        // Task<int> IDbConnection.ExecuteAsync(string sql, [object param = null], [IDbTransaction transaction = null], [int? commandTimeout = null], [CommandType? commandType = null])
        // int ExecuteAsync(string sql);

        // QuerySingleAsync
        // Execute a single-row query asynchronously using Task.
        // (awaitable, extension) Task<T> IDbConnection.QuerySingleAsync<T>(string sql, [object param = null], [IDbTransaction transaction = null], [int? commandTimeout = null], [CommandType? commandType = null]) (+ 1 overload)
        ValueTask<T> QuerySingle<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        // QueryAsync
        // Execute a query asynchronously using Task. Returns: A sequence of data of T;
        // (awaitable, extension) Task<IEnumerable<T>> IDbConnection.QueryAsync<T>(string sql, [object param = null], [IDbTransaction transaction = null], [int? commandTimeout = null], [CommandType? commandType = null])
        // Task<IEnumerable<T>> IDbConnection.QueryAsync<T>(string sql, [object param = null], [IDbTransaction transaction = null], [int? commandTimeout = null], [CommandType? commandType = null])
        ValueTask<IEnumerable<T>> Query<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        string Pwd();
    }
    // Trait
    public interface HasSqlDb<RT> where RT : struct, HasCancel<RT>
    {
        Aff<RT, SqlDbIO> AffSqlDb { get; }
    }

    public struct LiveSqlDbIO : SqlDbIO
    {
        public readonly string ConnectionString;
        LiveSqlDbIO(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static SqlDbIO New(string connectionString) =>
            new LiveSqlDbIO(connectionString);
        // public static readonly SqlDbIO Default = new LiveSqlDbIO();

        public string Pwd() =>
            System.IO.Directory.GetCurrentDirectory();
        // throw new Exception("AHHHHHHHHH!!!");

        public async ValueTask<T> QuerySingle<T>(string query, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new Exception("AHHHHHH");
            Console.WriteLine($"============={ConnectionString}");
            using (var c = new SqlConnection(ConnectionString))
            {
                return await c.QuerySingleAsync<T>(query, param, transaction, commandTimeout, commandType);
            }
        }
        public async ValueTask<IEnumerable<T>> Query<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var c = new SqlConnection(ConnectionString))
            {
                return await c.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
        }
    }

    public static class SqlDbAff<RT>
        where RT : struct, HasCancel<RT>, HasSqlDb<RT>
    {
        public static Aff<RT, string> pwd() =>
            default(RT).AffSqlDb.Map(p => p.Pwd());

        public static Aff<RT, T> querySingle<T>(string query, object param) =>
            default(RT).AffSqlDb.MapAsync(p => p.QuerySingle<T>(query, param));
    }

    public struct DataLayerRuntime : HasCancel<DataLayerRuntime>, HasSqlDb<DataLayerRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        readonly string connectionString;

        public CancellationToken CancellationToken { get; }

        public static DataLayerRuntime New(string connectionString) =>
            new DataLayerRuntime(connectionString, new CancellationTokenSource());

        DataLayerRuntime(string connectionString, CancellationTokenSource cancellationTokenSource) =>
            (this.connectionString, this.cancellationTokenSource, CancellationToken) =
               (connectionString, cancellationTokenSource, cancellationTokenSource.Token);

        public DataLayerRuntime LocalCancel =>
            new DataLayerRuntime(connectionString, new CancellationTokenSource());

        public Eff<DataLayerRuntime, CancellationTokenSource> CancellationTokenSource =>
            Eff<DataLayerRuntime, CancellationTokenSource>(env => env.cancellationTokenSource);

        // Either of the two options below work:
        //   Either you can pass the config values in through the constructor
        //   Or you can have the Runtime get the values from the configuration
        public Aff<DataLayerRuntime, SqlDbIO> AffSqlDb =>
            EffSqlDb.ToAsync();
        public Eff<DataLayerRuntime, SqlDbIO> EffSqlDb =>
            Eff<DataLayerRuntime, SqlDbIO>(env => LiveSqlDbIO.New(env.connectionString));
        // public Aff<DataLayerRuntime, SqlDbIO> AffSqlDb =>
        //     from conn in ConfigurationStore.ConnectionString
        //     select LiveSqlDbIO.New(conn);
        // public Eff<DataLayerRuntime, SqlDbIO> EffSqlDb =>
        //     from conn in ConfigurationStore.ConnectionString
        //     select LiveSqlDbIO.New(conn);
    }

    public class DataLayer
    {
        public static Aff<RT, Unit> SetConfig<RT>(Configuration configuration)
            where RT : struct, HasCancel<RT>, HasSqlDb<RT> =>
                from _ in ConfigurationStore.SetConfig(configuration)
                select unit;

        // Illustrates doing multiple queries and tieing them together
        public static Aff<RT, Person> CreatePerson<RT>(Person person)
            where RT : struct, HasCancel<RT>, HasSqlDb<RT> =>
                from personId in InsertPerson<RT>(person)
                from newPerson in ReadPerson<RT>(personId)
                select newPerson;

        public static Aff<RT, int> InsertPerson<RT>(Person person)
            where RT : struct, HasCancel<RT>, HasSqlDb<RT>
        {
            var sqlInsert = @"
                INSERT INTO persons
                (
                    name,
                    age
                )
                OUTPUT INSERTED.id
                VALUES
                (
                    @Name,
                    @Age
                )
            ";
            return from personId in SqlDbAff<RT>.querySingle<int>(sqlInsert, person)
                   select personId;
        }

        public static Aff<RT, Person> ReadPerson<RT>(int personId)
            where RT : struct, HasCancel<RT>, HasSqlDb<RT>
        {
            var sqlQuery = @"
                select
                    id as Id,
                    name as Name,
                    age as Age
                FROM
                    persons
                WHERE
                    id = @Id
            ";
            return from newPerson in SqlDbAff<RT>.querySingle<Person>(sqlQuery, new { Id = personId })
                   select newPerson;
        }
    }
}
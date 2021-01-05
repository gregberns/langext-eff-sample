using System;
using System.Linq;
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
        ValueTask<T> QuerySingle<T>(string sql, object param);

        // QueryAsync
        // Execute a query asynchronously using Task. Returns: A sequence of data of T;
        // (awaitable, extension) Task<T> IDbConnection.QuerySingleAsync<T>(string sql, [object param = null], [IDbTransaction transaction = null], [int? commandTimeout = null], [CommandType? commandType = null]) (+ 1 overload)

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

        public async ValueTask<T> QuerySingle<T>(string query, object param)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new Exception("AHHHHHH");
            Console.WriteLine($"============={ConnectionString}");
            using (var c = new SqlConnection(ConnectionString))
            {
                return await c.QuerySingleAsync<T>(query, param);
            }
        }
    }

    public static class SqlDbAff
    {
        public static Aff<RT, string> pwd<RT>()
            where RT : struct, HasCancel<RT>, HasSqlDb<RT> =>
                default(RT).AffSqlDb.Map(p => p.Pwd());

        public static Aff<RT, T> querySingle<RT, T>(string query, object param)
            where RT : struct, HasCancel<RT>, HasSqlDb<RT> =>
                default(RT).AffSqlDb.MapAsync(p => p.QuerySingle<T>(query, param));
    }

    public struct DataLayerRuntime : HasCancel<DataLayerRuntime>, HasSqlDb<DataLayerRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken { get; }
        public DataLayerRuntime(CancellationTokenSource cancellationTokenSource, string connectionString)
        {
            (this.cancellationTokenSource, CancellationToken)
                = (cancellationTokenSource, cancellationTokenSource.Token);
            ConnectionString.Swap(old => connectionString);
        }

        public Eff<DataLayerRuntime, CancellationTokenSource> CancellationTokenSource =>
            SuccessEff(cancellationTokenSource);
        public DataLayerRuntime LocalCancel =>
            new DataLayerRuntime(new CancellationTokenSource(), ConnectionString.Value);

        public static Atom<string> ConnectionString = Atom("");
        public Aff<DataLayerRuntime, SqlDbIO> AffSqlDb =>
            SuccessAff(LiveSqlDbIO.New(ConnectionString));
    }

    public class DataLayer
    {
        // public static Aff<RT, T> QuerySingle<RT, T>(string query, object param)
        //     where RT : struct, HasCancel<RT>, HasSqlDb<RT> =>
        //         from t in SqlDbAff.querySingle<RT, T>(query, param)
        //         select t;

        // Illustrates doing multiple queries and tieing them together
        public static Aff<RT, Person> CreatePerson<RT>(Person person)
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
            return from personId in SqlDbAff.querySingle<RT, int>(sqlInsert, person)
                   from newPerson in SqlDbAff.querySingle<RT, Person>(sqlQuery, new { Id = personId })
                   select newPerson;
        }
    }
}
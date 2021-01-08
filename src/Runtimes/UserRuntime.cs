using System;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.Interfaces;

namespace LangExtEffSample
{
    // [Record]
    public class UserEnv : Record<Env>
    {
        public readonly string Name;
        public readonly string ConnectionString;
    }

    public class Env : Record<Env>
    {
        public readonly HashMap<string, UserEnv> Users;
        public readonly Option<UserEnv> User;
        Env(HashMap<string, UserEnv> users, Option<UserEnv> user) =>
            (Users, User) =
                (users, user);
        public Env With(HashMap<string, UserEnv>? Users = null, Option<UserEnv>? User = null) =>
            new Env(
                Users ?? this.Users,
                User ?? this.User
            );
    }

    public interface HasUsers<RT>
    {
        Fin<RT> SetCurrentUser(string id);
        Eff<RT, UserEnv> CurrentUser { get; }
        Eff<RT, Unit> AssertUserExists(string id);
    }

    public struct UserRuntime : HasCancel<UserRuntime>, HasUsers<UserRuntime>
    {
        readonly CancellationTokenSource cancellationTokenSource;
        readonly Env env;

        public CancellationToken CancellationToken { get; }

        public static UserRuntime New(Env env) =>
            new UserRuntime(env, new CancellationTokenSource());

        UserRuntime(Env env, CancellationTokenSource cancellationTokenSource) =>
            (this.env, this.cancellationTokenSource, CancellationToken) =
                (env, cancellationTokenSource, cancellationTokenSource.Token);

        public UserRuntime LocalCancel =>
            new UserRuntime(env, new CancellationTokenSource());

        public Eff<UserRuntime, CancellationTokenSource> CancellationTokenSource =>
            Eff<UserRuntime, CancellationTokenSource>(env => env.cancellationTokenSource);

        public Fin<UserRuntime> SetCurrentUser(string id) =>
            env.Users.ContainsKey(id)
                ? FinSucc(new UserRuntime(env.With(User: env.Users[id]), cancellationTokenSource))
                : FinFail<UserRuntime>(Error.New("Invalid user ID"));

        public Eff<UserRuntime, UserEnv> CurrentUser =>
            EffMaybe<UserRuntime, UserEnv>(
                env =>
                   env.env.User.Match(
                       Some: FinSucc,
                       None: FinFail<UserEnv>(Error.New("Current user not set"))));

        public Eff<UserRuntime, Unit> AssertUserExists(string id) =>
            EffMaybe<UserRuntime, Unit>(
                rt => rt.env.Users.ContainsKey(id)
                          ? FinSucc(unit)
                          : FinFail<Unit>(Error.New("User doesn't exist")));
    }

    public static class Users<RT> where RT : struct, HasCancel<RT>, HasUsers<RT>
    {
        public static Aff<RT, A> withUser<A>(string id, Aff<RT, A> ma) =>
            from _ in default(RT).AssertUserExists(id)
            from r in localAff<RT, RT, A>(env => env.SetCurrentUser(id).IfFail(env), ma)
            select r;

        public static Eff<RT, A> withUser<A>(string id, Eff<RT, A> ma) =>
            from _ in default(RT).AssertUserExists(id)
            from r in localEff<RT, RT, A>(env => env.SetCurrentUser(id).IfFail(env), ma)
            select r;

        public static Eff<RT, UserEnv> user =>
            from env in runtime<RT>()
            from usr in env.CurrentUser
            select usr;

        public static Eff<RT, string> userName =>
            from usr in user
            select usr.Name;

        public static Eff<RT, string> userConnectingString =>
            from usr in user
            select usr.ConnectionString;
    }
}
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

namespace LangExtEffSample
{
    // This doesn't really do anything... so need to attach this to a "Program" Runtime??
    // public struct ConfigRuntime : HasEnvVars<ConfigRuntime>
    // {
    //     public Eff<ConfigRuntime, EnvVarsIO> EffEnvVars =>
    //         Eff<ConfigRuntime, EnvVarsIO>(env => new LiveEnvVarsIO());
    // }
}

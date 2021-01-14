using System;
using System.IO;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Interfaces;
using static LanguageExt.Prelude;
using LanguageExt.Attributes;
using Newtonsoft.Json;

namespace LanguageExt.Interfaces
{
    public interface EnvironmentIO
    {
        string CommandLine();
        string CurrentDirectory();
        Unit SetCurrentDirectory(string directory);
        int CurrentManagedThreadId();
        Unit Exit(int exitCode);
        int ExitCode();
        Unit SetExitCode(int exitCode);
        string ExpandEnvironmentVariables(string name);
        Unit FailFast(string? message);
        Unit FailFast(Option<string> message, Option<Exception> exception);
        Arr<string> GetCommandLineArgs();
        Option<string> GetEnvironmentVariable(string variable);
        Option<string> GetEnvironmentVariable(string variable, EnvironmentVariableTarget target);
        System.Collections.IDictionary GetEnvironmentVariables();
        System.Collections.IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target);
        string GetFolderPath(Environment.SpecialFolder folder);
        string GetFolderPath(Environment.SpecialFolder folder, Environment.SpecialFolderOption option);
        Arr<string> GetLogicalDrives();
        bool HasShutdownStarted();
        bool Is64BitOperatingSystem();
        bool Is64BitProcess();
        string MachineName();
        string NewLine();
        OperatingSystem OSVersion();
        int ProcessorCount();
        Unit SetEnvironmentVariable(string variable, string? value);
        Unit SetEnvironmentVariable(string variable, string? value, EnvironmentVariableTarget target);
        string StackTrace();
        string SystemDirectory();
        int SystemPageSize();
        int TickCount();
        long TickCount64();
        string UserDomainName();
        bool UserInteractive();
        string UserName();
        Version Version();
        long WorkingSet();
    }

    /// <summary>
    /// Type-class giving a struct the trait of supporting Environment IO
    /// </summary>
    /// <typeparam name="RT">Runtime</typeparam>
    [Typeclass("*")]
    public interface HasEnvironment<RT> : HasCancel<RT>
        where RT : struct, HasCancel<RT>
    {
        /// <summary>
        /// Access the environment asynchronous effect environment
        /// </summary>
        /// <returns>Environment asynchronous effect environment</returns>
        Aff<RT, EnvironmentIO> EnvironmentAff { get; }

        /// <summary>
        /// Access the environment synchronous effect environment
        /// </summary>
        /// <returns>Environment synchronous effect environment</returns>
        Eff<RT, EnvironmentIO> EnvironmentEff { get; }
    }
}

namespace LanguageExt.LiveIO
{
    public struct EnvironmentIO : Interfaces.EnvironmentIO
    {
        public static Interfaces.EnvironmentIO Default =>
            new EnvironmentIO();

        // Gets the command line for this process.
        public string CommandLine() =>
            Environment.CommandLine;

        // Gets the fully qualified path of the current working directory.
        public string CurrentDirectory() =>
            Environment.CurrentDirectory;

        // Sets the fully qualified path of the current working directory.
        // directory: fully qualified path of the current working directory.
        public Unit SetCurrentDirectory(string directory)
        {
            Environment.CurrentDirectory = directory;
            return unit;
        }

        // Gets a unique identifier for the current managed thread.
        public int CurrentManagedThreadId() =>
            Environment.CurrentManagedThreadId;

        // Terminates this process and returns an exit code to the operating system.
        public Unit Exit(int exitCode)
        {
            Environment.Exit(exitCode);
            return unit;
        }

        // Gets the exit code of the process.
        public int ExitCode() =>
            Environment.ExitCode;

        // Sets the exit code of the process.
        // exitCode: exit code of the process
        public Unit SetExitCode(int exitCode)
        {
            Environment.ExitCode = exitCode;
            return unit;
        }

        // Replaces the name of each environment variable embedded in the specified string with the string equivalent of the value of the variable, then returns the resulting string.
        // name: A string containing the names of zero or more environment variables. Each environment variable is quoted with the percent sign character (%).
        public string ExpandEnvironmentVariables(string name) =>
            Environment.ExpandEnvironmentVariables(name);

        // Immediately terminates a process after writing a message to the Windows Application event log, and then includes the message in error reporting to Microsoft.
        // message: A message that explains why the process was terminated, or null if no explanation is provided.
        public Unit FailFast(string? message)
        {
            Environment.FailFast(message);
            return unit;
        }
        // Immediately terminates a process after writing a message to the Windows Application event log, and then includes the message and exception information in error reporting to Microsoft.
        // message: A message that explains why the process was terminated, or null if no explanation is provided.
        // exception: An exception that represents the error that caused the termination. This is typically the exception in a catch block.
        public Unit FailFast(Option<string> message, Option<Exception> exception)
        {
            Environment.FailFast(message.IfNone(() => null), exception.IfNone(() => null));
            return unit;
        }

        // Returns a string array containing the command-line arguments for the current process.
        public Arr<string> GetCommandLineArgs() =>
            Environment.GetCommandLineArgs();

        // Retrieves the value of an environment variable from the current process.
        // variable: The name of an environment variable.
        public Option<string> GetEnvironmentVariable(string variable) =>
            Environment.GetEnvironmentVariable(variable);

        // Retrieves the value of an environment variable from the current process or from the Windows operating system registry key for the current user or local machine.
        // variable: The name of an environment variable.
        public Option<string> GetEnvironmentVariable(string variable, EnvironmentVariableTarget target) =>
            Environment.GetEnvironmentVariable(variable, target);

        // Retrieves all environment variable names and their values from the current process.
        public System.Collections.IDictionary GetEnvironmentVariables() =>
            Environment.GetEnvironmentVariables();

        // Retrieves all environment variable names and their values from the current process, or from the Windows operating system registry key for the current user or local machine.
        // target: One of the System.EnvironmentVariableTarget values. Only System.EnvironmentVariableTarget.Process is supported on .NET Core running on Unix-based systems.
        public System.Collections.IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target) =>
            Environment.GetEnvironmentVariables(target);

        // Gets the path to the system special folder that is identified by the specified enumeration.
        // folder: One of enumeration values that identifies a system special folder.
        public string GetFolderPath(Environment.SpecialFolder folder) =>
            Environment.GetFolderPath(folder);

        // Gets the path to the system special folder that is identified by the specified enumeration, and uses a specified option for accessing special folders.
        // folder: One of the enumeration values that identifies a system special folder.
        // option: One of the enumeration values that specifies options to use for accessing a special folder.
        public string GetFolderPath(Environment.SpecialFolder folder, Environment.SpecialFolderOption option) =>
            Environment.GetFolderPath(folder, option);

        // Returns an array of string containing the names of the logical drives on the current computer.
        // string[] Environment.GetLogicalDrives()
        public Arr<string> GetLogicalDrives() =>
            Environment.GetLogicalDrives();

        // Gets a value that indicates whether the current application domain is being unloaded or the common language runtime (CLR) is shutting down.
        public bool HasShutdownStarted() =>
            Environment.HasShutdownStarted;

        // Determines whether the current operating system is a 64-bit operating system.
        public bool Is64BitOperatingSystem() =>
            Environment.Is64BitOperatingSystem;

        // Determines whether the current process is a 64-bit process.
        public bool Is64BitProcess() =>
            Environment.Is64BitProcess;

        // Gets the NetBIOS name of this local computer.
        public string MachineName() =>
            Environment.MachineName;

        // Gets the newline string defined for this environment.
        public string NewLine() =>
            Environment.NewLine;

        // Gets an OperatingSystem object that contains the current platform identifier and version number.
        public OperatingSystem OSVersion() =>
            Environment.OSVersion;

        // Gets the number of processors on the current machine.
        public int ProcessorCount() =>
            Environment.ProcessorCount;

        // Creates, modifies, or deletes an environment variable stored in the current process.
        // variable: The name of an environment variable.
        // value: A value to assign to variable .
        public Unit SetEnvironmentVariable(string variable, string? value)
        {
            Environment.SetEnvironmentVariable(variable, value);
            return unit;
        }

        // Creates, modifies, or deletes an environment variable stored in the current process or in the Windows operating system registry key reserved for the current user or local machine.
        // variable: The name of an environment variable.
        // value: A value to assign to variable.
        // target: One of the enumeration values that specifies the location of the environment variable.
        public Unit SetEnvironmentVariable(string variable, string? value, EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable(variable, value, target);
            return unit;
        }

        // Gets current stack trace information.
        public string StackTrace() =>
            Environment.StackTrace;

        // Gets the fully qualified path of the system directory.
        public string SystemDirectory() =>
            Environment.SystemDirectory;

        // Gets the number of bytes in the operating system's memory page.
        public int SystemPageSize() =>
            Environment.SystemPageSize;

        // Gets the number of milliseconds elapsed since the system started.
        public int TickCount() =>
            Environment.TickCount;

        // Gets the number of milliseconds elapsed since the system started.
        public long TickCount64() =>
            Environment.TickCount64;

        // Gets the network domain name associated with the current user.
        public string UserDomainName() =>
            Environment.UserDomainName;

        // Gets a value indicating whether the current process is running in user interactive mode.
        public bool UserInteractive() =>
            Environment.UserInteractive;

        // Gets the user name of the person who is currently logged on to the operating system.
        public string UserName() =>
            Environment.UserName;

        // Gets a Version object that describes the major, minor, build, and revision numbers of the common language runtime.
        public Version Version() =>
            Environment.Version;

        // Gets the amount of physical memory mapped to the process context.
        public long WorkingSet() =>
            Environment.WorkingSet;
    }
}
#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using BackingUpConsole.Utilities.Exceptions;

namespace BackingUpConsole.Utilities
{
    internal static class Miscellaneous
    {
        public static string[] CommandLineToArgs(string commandLine) => Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => UnixCommandLineToArgs(commandLine),
            PlatformID.Win32NT => WindowsCommandLineToArgs(commandLine),
            _ => throw new OSNotSupportedException(Environment.OSVersion)
        };
        private static string[] UnixCommandLineToArgs(string commandLine)
        {
            string dllPath = PathHandler.Combine(Assembly.GetExecutingAssembly().Location.Split('/')[0..^1].CustomToString("/"), "ArgumentGetter.dll");
            string dotnetPath = "/usr/share/dotnet/dotnet";
            Process executor = new();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = dotnetPath,
                Arguments = $"{dllPath} {commandLine}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            executor.StartInfo = startInfo;
            executor.Start();
            List<string> args = new List<string>();
            while (!executor.StandardOutput.EndOfStream)
            {
                string line = executor.StandardOutput.ReadLine();
                args.Add(line);
                // do something with line
            }
            // executor.WaitForExit();
            // var output = executor.StandardOutput;
            // Console.WriteLine($"Read: <<<{output.ReadToEnd()}>>>");
            return args.ToArray();
        }
#pragma warning disable
        //Source: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
    [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private static string[] WindowsCommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
#pragma warning enable

        public static void ExitProgram(int exitCode, string source)
        {
            Console.WriteLine($"Program wil be terminated. Reason: {source}");
            Environment.Exit(exitCode);
        }

        public static bool CheckLength(this string[] arr, int min, int max) => (arr.Length <= max || max == -1) && (arr.Length >= min || min == -1);
        
        public static string CustomToString<T>(this ICollection<T> collection, string delimiter = ", ")  // TODO: Find a better name for this function
        {
            string result = string.Empty;
            using IEnumerator<T> enumerator = collection.GetEnumerator();
            while(enumerator.MoveNext())
            {
                result += $"{enumerator.Current}{delimiter}";
            }
            result = result.Substring(0, Math.Max(result.Length - delimiter.Length, 0));
            return result;
        }
    }
}

//#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BackingUpConsole.Utilities
{
    internal static class Miscellaneous
    {
        //#pragma warning disable
#nullable disable
        //Source: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
    [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
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
        //#pragma warning enable
#nullable enable

        public static void ExitProgram(int exitCode, string source)
        {
            Console.WriteLine($"Program wil be terminated. Reason: {source}");
            Environment.Exit(exitCode);
        }

        public static bool CheckLength(this string[] arr, int min, int max) => (arr.Length <= max || max == -1) && (arr.Length >= min || min == -1);

        public static string CustomToString<T>(this ICollection<T> collection, string delimiter = ", ")  // TODO: Find a better name for this function
        {
            string result = string.Empty;
            var enumerator = collection.GetEnumerator();
            while(enumerator.MoveNext())
            {
                result += $"{enumerator.Current}{delimiter}";
            }
            result = result.Substring(0, Math.Max(result.Length - delimiter.Length, 0));
            return result;
        }
    }
}

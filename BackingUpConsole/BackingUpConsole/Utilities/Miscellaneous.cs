using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BackingUpConsole.Utilities.Exceptions;

namespace BackingUpConsole.Utilities
{
    internal static class Miscellaneous
    {
        public static string DllFolderPath => Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => string.Join('/', Assembly.GetExecutingAssembly().Location.Split('/')[0..^1]),
            PlatformID.Win32NT => string.Join('\\', Assembly.GetExecutingAssembly().Location.Split('\\')[0..^1]),
            _ => throw new OSNotSupportedException(Environment.OSVersion)
        };
        public static string[] CommandLineToArgs(string commandLine) => Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => UnixCommandLineToArgs(commandLine),
            PlatformID.Win32NT => WindowsCommandLineToArgs(commandLine),
            _ => throw new OSNotSupportedException(Environment.OSVersion)
        };
        private static string[] UnixCommandLineToArgs(string commandLine)
        {
            string dllPath = PathHandler.Combine(DllFolderPath, "ArgumentGetter.dll");
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
#nullable disable
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
            using IEnumerator<T> enumerator = collection.GetEnumerator();
            while(enumerator.MoveNext())
            {
                result += $"{enumerator.Current}{delimiter}";
            }
            result = result.Substring(0, Math.Max(result.Length - delimiter.Length, 0));
            return result;
        }

        public static char[] FilenameForbiddenChars { get; } = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

        // Source: https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/
        public static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();

            var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
            var results = new Task<Task<T>>[buckets.Length];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                results[i] = buckets[i].Task;
            }

            int nextTaskIndex = -1;
            Action<Task<T>> continuation = completed =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            };

            foreach (var inputTask in inputTasks)
                inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return results;
        }

        public static FileStream CreateFileAndDirectoryIfNotExist(FileInfo file)
        {
            if (file.Directory.Exists == false)
                file.Directory.Create();

            return File.Create(file.FullName);
        }
        
        public static async Task<bool> FilesAreIdentical(FileInfo fileA, FileInfo fileB)
        {
            if ((fileA.Exists && fileB.Exists) == false)
                return false;

            if (fileA.Length != fileB.Length)
                return false;

            await using FileStream sourceStreamA = new FileStream(fileA.FullName, FileMode.Open);
            await using FileStream sourceStreamB = new FileStream(fileB.FullName, FileMode.Open);
            int currentValue;
            while ((currentValue = sourceStreamA.ReadByte()) == sourceStreamB.ReadByte())
                if (currentValue == -1)
                    return true;
            
            return false;
        }
    }
}

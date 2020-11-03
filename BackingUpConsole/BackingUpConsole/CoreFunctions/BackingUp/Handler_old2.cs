#if FALSE

using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.BackingUp
{
    internal static class Handler
    {
        private static int MaxBlockSize => 1;
        private static string Path => @"N:\Clemens\Dateien\Schule";
        private static async Task<MessageHandler> BackUpFile(FileInfo file, CancellationToken cancellationToken)
        {
            bool? success = false;
            try
            {
                using StreamReader sr = new StreamReader(file.FullName);
                char[] buffer = new char[MaxBlockSize];
                while (sr.EndOfStream == false)
                {
                    int saved = await sr.ReadAsync(buffer, 0, MaxBlockSize);
                    for (int i = 0; i < buffer.Length && i < saved; i++)
                    {
                        if (buffer[i] == '~')
                        {
                            success = true;
                            //goto BreakOuterAsync;
                        }
                    }
                }
            }
            catch
            {
                success = null;
            }
        BreakOuterAsync:
            return MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}");
        }
        private static async IAsyncEnumerable<MessageHandler> BackUpDirectory(DirectoryInfo directory, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            List<IAsyncEnumerable<MessageHandler>> tasks = new List<IAsyncEnumerable<MessageHandler>>();
            tasks.AddRange(directory.EnumerateDirectories().Select(dir => BackUpDirectory(dir, cancellationToken)));
            ////IAsyncEnumerable<MessageHandler> asyncEnumerable = new IAsyncEnumerable<MessageHandler>();
            ////tasks.Add(directory.EnumerateFiles().Select(file => BackUpFile(file, cancellationToken)));
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                yield return await BackUpFile(file, cancellationToken);
            }
        }

        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await foreach (MessageHandler message in BackUpDirectory(new DirectoryInfo(Path), new CancellationTokenSource().Token))
            {
                messagePrinter.Print(message);
            }
            sw.Stop();
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms.");
            return MessageProvider.Success();
        }
    }
}
#endif

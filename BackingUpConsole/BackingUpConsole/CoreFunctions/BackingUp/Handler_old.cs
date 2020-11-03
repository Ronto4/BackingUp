#if FALSE
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.BackingUp
{
    internal static class Handler
    {
        private const int MaxBlockSize = 1;

        //private static async Task BackUpFile(FileInfo file, BlockingCollection<MessageHandler> messageCollection)
        //{
        //    bool? success = false;
        //    try
        //    {
        //        using StreamReader sr = new StreamReader(file.FullName);
        //        char[] buffer = new char[MaxBlockSize];
        //        while (sr.EndOfStream == false)
        //        {
        //            int saved = await sr.ReadAsync(buffer, 0, MaxBlockSize);
        //            for (int i = 0; i < buffer.Length && i < saved; i++)
        //            {
        //                if (buffer[i] == '~')
        //                {
        //                    success = true;
        //                    //goto BreakOuterAsync;
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        success = null;
        //    }
        //BreakOuterAsync:
        //    messageCollection.Add(MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}"));
        //}
        //private static async Task BackUpDirectory(DirectoryInfo directory, string[] blacklistedExtensions, BlockingCollection<MessageHandler> messageCollection)
        //{
        //    List<Task> tasks = new List<Task>();
        //    tasks.AddRange(directory.EnumerateDirectories().Select(dir => Task.Run(() => BackUpDirectory(dir, blacklistedExtensions, messageCollection))));
        //    tasks.AddRange(directory.EnumerateFiles().Select(file => Task.Run(() => BackUpFile(file, messageCollection))));
        //    await Task.WhenAll(tasks);
        //}
        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter)
        {
            string path = @"C:\Windows\System32";
            //Stopwatch sw;
            //using (BlockingCollection<MessageHandler> messageCollection = new BlockingCollection<MessageHandler>())
            //{
            //    sw = new Stopwatch();
            //    sw.Start();
            //    CancellationTokenSource cts = new CancellationTokenSource();
            //    Task taskSave = Task.Run(async () =>
            //    {
            //        await BackUpDirectory(new DirectoryInfo(path), new string[] { }, messageCollection);
            //        messageCollection.Add(MessageProvider.Message("Done!"));
            //        messageCollection.CompleteAdding();
            //        cts.Cancel();
            //    });
            //    Task taskGet = Task.Run(() =>
            //    {
            //        while (messageCollection.IsCompleted == false)
            //        {
            //            MessageHandler message = messageCollection.Take(cts.Token);
            //            messagePrinter.Print(message);
            //        }
            //    });
            //    await Task.WhenAll(taskSave, taskGet);
            //    sw.Stop();
            //}
            //Console.WriteLine("----  ----  ----  ----");
            ClassicBackup(path);
            //messagePrinter.Print(MessageProvider.Message($"Async Time: {sw.ElapsedMilliseconds} ms"));
            return MessageProvider.Success();
        }
        private static void Dir(DirectoryInfo directory)
        {
            try
            {
                foreach (var dir in directory.EnumerateDirectories())
                {
                    Dir(dir);
                }
                foreach (var file in directory.EnumerateFiles())
                {
                    bool? success = File(file);
                    Console.WriteLine($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}");
                }
            }
            catch { }
        }
        private static void ClassicBackup(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Dir(directory);
            sw.Stop();
            Console.WriteLine($"Sync Time: {sw.ElapsedMilliseconds} ms");
        }
        private static bool? File(FileInfo file)
        {
            bool? success = false;
            try
            {
                using StreamReader sr = new StreamReader(file.FullName);
                char[] buffer = new char[MaxBlockSize];
                while (sr.EndOfStream == false)
                {
                    int saved = sr.Read(buffer, 0, MaxBlockSize);
                    for (int i = 0; i < buffer.Length && i < saved; i++)
                    {
                        if (buffer[i] == '~')
                        {
                            success = true;
                            //goto BreakOuterSync;
                        }
                    }
                    success = false;
                }
            }
            catch
            {
                success = null;
            }
        BreakOuterSync:
            return success;
            //messageCollection.Add(MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}"));
        }
    }
}
#endif

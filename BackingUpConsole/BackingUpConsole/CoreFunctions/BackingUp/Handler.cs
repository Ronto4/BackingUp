#if TRUE
using BackingUpConsole.Utilities;
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
        //private static int MaxBlockSize => 1048576;
        private const int DefaultMaxBlockSize = 65536;
        private static int MaxBlockSize { get; set; }
#if FALSE
        private static string Path => @"P:\Trend\EEP16\Resourcen\Rollmaterial";
        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter)
        {
            Stopwatch sw = new Stopwatch();
            //sw.Start();
            //await MediumParallel.PerformBackup(messagePrinter);
            //sw.Stop();
            //long medium = sw.ElapsedMilliseconds;
            //messagePrinter.Print(MessageProvider.Message("---- Next ----", color: ConsoleColor.Red));
            //sw = new Stopwatch();
            sw.Start();
            await SequentialBackup.PerformBackup(new DirectoryInfo(Path), new DirectoryInfo(backUp.FileContainer.DataDir), messagePrinter);
            sw.Stop();
            long classic = sw.ElapsedMilliseconds;
            messagePrinter.Print(MessageProvider.Message("---- Next ----", color: ConsoleColor.Red));
            messagePrinter.Print(MessageProvider.Message("---- Press <Enter> to continue ----", color: ConsoleColor.Red));
            Console.ReadLine();
            sw = new Stopwatch();
            sw.Start();
            await Parallel.PerformBackup(new DirectoryInfo(Path), new DirectoryInfo(backUp.FileContainer.DataDir), messagePrinter);
            sw.Stop();
            long parallel = sw.ElapsedMilliseconds;
            //Console.WriteLine($"Time: {parallel} ms.");
            Console.WriteLine($"Time: Sequential: {classic} ms, Parallel: {parallel} ms.");
            //Console.WriteLine($"Time: Classic: {classic} ms, Medium: {medium} ms, Parallel: {parallel} ms.");
            return MessageProvider.Success();
        }
#endif
        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter, bool useSequential = false, int maxBlockSize = DefaultMaxBlockSize)
        {
            MaxBlockSize = maxBlockSize;
            Stopwatch sw = new Stopwatch();
            if (useSequential)
            {
                sw.Start();
                await SequentialBackup.PerformBackup(new DirectoryInfo(backUp.Settings!.Settings.Paths[0]), new DirectoryInfo(backUp.FileContainer.DataDir), messagePrinter);
                sw.Stop();
            }
            else
            {
                sw.Start();
                await Parallel.PerformBackup(new DirectoryInfo(backUp.Settings!.Settings.Paths[0]), new DirectoryInfo(backUp.FileContainer.DataDir), messagePrinter);
                sw.Stop();
            }
            return MessageProvider.Message($"The process took {sw.ElapsedMilliseconds} ms in {(useSequential ? "sequential" : "parallel")} mode with a MaxBlockSize of {MaxBlockSize}.");
        }
        //private static async Task<MessageHandler> BackUpFile(FileInfo file, FileInfo target, bool doArchive, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        bool differenceFound = (await Miscellaneous.FilesAreIdentical(file, target, MaxBlockSize)) == false;
        //        if (differenceFound)
        //        {
        //            using StreamReader sourceStream = new StreamReader(file.FullName);
        //            using StreamWriter targetStream = new StreamWriter(Miscellaneous.CreateFileAndDirectoryIfNotExist(target));
        //            char[] sourceBuffer = new char[MaxBlockSize];
        //            while (sourceStream.EndOfStream == false)
        //            {
        //                int saved = await sourceStream.ReadAsync(sourceBuffer, 0, MaxBlockSize);
        //                Task write = targetStream.WriteAsync(sourceBuffer, 0, saved);
        //                if (doArchive)
        //                {
        //                    // Process archive, still missing.
        //                }
        //                await write;
        //            }
        //            return MessageProvider.Message($"Successfully copied file at '{file.FullName}' to '{target.FullName}'.", color: ConsoleColor.Green);
        //        }
        //        else
        //        {
        //            return MessageProvider.Message($"File at '{file.FullName}' already exists at '{target.FullName}'.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return MessageProvider.Message($"An error occurred while backing up file at '{file.FullName}':{Environment.NewLine}{ex.Message}", MessageCollections.Levels.Error);
        //    }
        //}
        private static async Task<MessageHandler> BackUpFile(FileInfo file, FileInfo target, bool doArchive, CancellationToken cancellationToken)
        {
            try
            {
                bool differenceFound = (await Miscellaneous.FilesAreIdentical(file, target, MaxBlockSize)) == false;
                if (differenceFound)
                {
                    byte[] msBuffer = new byte[MaxBlockSize];
                    using FileStream sourceStream = new FileStream(file.FullName, FileMode.Open);
                    //using StreamWriter targetStream = new StreamWriter(Miscellaneous.CreateFileAndDirectoryIfNotExist(target));
                    using FileStream targetStream = Miscellaneous.CreateFileAndDirectoryIfNotExist(target);
                    //using MemoryStream sourceMSStream = new MemoryStream(msBuffer);
                    //char[] sourceBuffer = new char[MaxBlockSize];
                    int read = 1;
                    while (read > 0)
                    {
                        read = await sourceStream.ReadAsync(msBuffer, 0, MaxBlockSize);
                        Task write = targetStream.WriteAsync(msBuffer, 0, read);
                        //int saved = await sourceStream.ReadAsync(sourceBuffer, 0, MaxBlockSize);
                        //Task write = targetStream.WriteAsync(sourceBuffer, 0, saved);
                        if (doArchive)
                        {
                            // Process archive, still missing.
                        }
                        await write;
                    }
                    return MessageProvider.Message($"Successfully copied file at '{file.FullName}' to '{target.FullName}'.", color: ConsoleColor.Green);
                }
                else
                {
                    return MessageProvider.Message($"File at '{file.FullName}' already exists at '{target.FullName}'.");
                }
            }
            catch (Exception ex)
            {
                return MessageProvider.Message($"An error occurred while backing up file at '{file.FullName}':{Environment.NewLine}{ex.Message}", MessageCollections.Levels.Error);
            }
        }

        private static class Parallel
        {
            private static async Task<List<FileInfo>> EnumerateAllFilesRecursively(DirectoryInfo root, CancellationToken cancellationToken)
            {
                List<FileInfo> files = new List<FileInfo>();
                try
                {
                    foreach (DirectoryInfo directory in root.EnumerateDirectories())
                    {
                        files.AddRange(await EnumerateAllFilesRecursively(directory, cancellationToken));
                    }
                }
                catch { }
                try
                {
                    files.AddRange(root.EnumerateFiles());
                }
                catch { }
                return files;
            }
            private static async IAsyncEnumerable<MessageHandler> BackupDirectory(DirectoryInfo directory, DirectoryInfo backupDir, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                yield return MessageProvider.Message("Beginning to enumerate all files...");
                List<FileInfo> files = await EnumerateAllFilesRecursively(directory, cancellationToken);
                yield return MessageProvider.Message($"Enumerating done! Found {files.Count} files.");
                List<Task<MessageHandler>> tasks = new List<Task<MessageHandler>>();
                tasks.AddRange(files.Select(file => Task.Run(() => BackUpFile(file, new FileInfo(PathHandler.Combine(backupDir.FullName, file.FullName.Replace(':', '-'))), false, cancellationToken))));
                foreach (var bucket in Miscellaneous.Interleaved(tasks))
                {
                    var t = await bucket;
                    yield return await t;
                }
                //while (tasks.Any()) // -> probably slower than solution above
                //{
                //    Task<MessageHandler> message = await Task.WhenAny(tasks);
                //    tasks.Remove(message);
                //    //yield return MessageProvider.Message("Got a message!");
                //    yield return await message;
                //}
            }
            internal static async Task PerformBackup(DirectoryInfo backupRoot, DirectoryInfo backupDir, MessagePrinter messagePrinter)
            {
                await foreach (MessageHandler message in BackupDirectory(backupRoot, backupDir, new CancellationTokenSource().Token))
                {
                    messagePrinter.Print(message);
                }
            }
        }
#if FALSE // MediumParallel
        private static class MediumParallel
        {
            private static async Task BackUpFile(FileInfo file, CancellationToken cancellationToken, MessagePrinter messagePrinter)
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
                messagePrinter.Print(MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}"));
                //return MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}");
            }
            private static async Task BackUpDirectory(DirectoryInfo directory, CancellationToken cancellationToken, MessagePrinter messagePrinter)
            {
                try
                {
                    List<Task> tasks = new List<Task>();
                    tasks.AddRange(directory.EnumerateDirectories().Select(dir => Task.Run(() => BackUpDirectory(dir, cancellationToken, messagePrinter))));
                    tasks.AddRange(directory.EnumerateFiles().Select(file => Task.Run(() => BackUpFile(file, cancellationToken, messagePrinter))));
                    await Task.WhenAll(tasks);
                }
                catch { }
            }
            internal static async Task PerformBackup(MessagePrinter messagePrinter)
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                await BackUpDirectory(new DirectoryInfo(Path), new CancellationTokenSource().Token, messagePrinter);
                //sw.Stop();
                //Console.WriteLine("----  ----  ----  ----");
                //ClassicBackup(Path);
                //Console.WriteLine($"Async Time: {sw.ElapsedMilliseconds} ms.");
                //return MessageProvider.Success();
                //return sw.ElapsedMilliseconds;
            }
        }
#endif
        private static class SequentialBackup
        {
            private static async Task Dir(DirectoryInfo directory, DirectoryInfo backupDir, bool doArchive, MessagePrinter messagePrinter, CancellationToken cancellationToken)
            {
                try
                {
                    foreach (var dir in directory.EnumerateDirectories())
                    {
                        await Dir(dir, backupDir, doArchive, messagePrinter, cancellationToken);
                    }
                }
                catch { }
                try { 
                    foreach (var file in directory.EnumerateFiles())
                    {
                        MessageHandler message = await BackUpFile(file, new FileInfo(PathHandler.Combine(backupDir.FullName, file.FullName.Replace(':', '-'))), false, cancellationToken);
                        messagePrinter.Print(message);
                    }
                }
                catch { }
            }
            internal static async Task PerformBackup(DirectoryInfo backupRoot, DirectoryInfo backupDir, MessagePrinter messagePrinter)
            {
                await Dir(backupRoot, backupDir, false, messagePrinter, new CancellationTokenSource().Token);
            }
            //private static bool? File(FileInfo file)
            //{
            //    bool? success = false;
            //    try
            //    {
            //        using StreamReader sr = new StreamReader(file.FullName);
            //        char[] buffer = new char[MaxBlockSize];
            //        while (sr.EndOfStream == false)
            //        {
            //            int saved = sr.Read(buffer, 0, MaxBlockSize);
            //            for (int i = 0; i < buffer.Length && i < saved; i++)
            //            {
            //                if (buffer[i] == '~')
            //                {
            //                    success = true;
            //                    //goto BreakOuterSync;
            //                }
            //            }
            //            success = false;
            //        }
            //    }
            //    catch
            //    {
            //        success = null;
            //    }
            //BreakOuterSync:
            //    return success;
            //    //messageCollection.Add(MessageProvider.Message($"File at directory '{file.Directory.FullName}': '{file.FullName}': {success.ToString() ?? "Error"}"));
            //}
        }
    }
}
#endif

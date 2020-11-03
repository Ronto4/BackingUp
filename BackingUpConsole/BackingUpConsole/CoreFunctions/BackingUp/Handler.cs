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
        private static int MaxBlockSize => 1;
        private static string Path => @"N:\Clemens\Dateien\Verkehr";
        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter)
        {
            Stopwatch sw = new Stopwatch();
            //sw.Start();
            //await MediumParallel.PerformBackup(messagePrinter);
            //sw.Stop();
            //long medium = sw.ElapsedMilliseconds;
            //messagePrinter.Print(MessageProvider.Message("---- Next ----", color: ConsoleColor.Red));
            //sw = new Stopwatch();
            //sw.Start();
            //Classic.ClassicBackup(Path);
            //sw.Stop();
            //long classic = sw.ElapsedMilliseconds;
            //messagePrinter.Print(MessageProvider.Message("---- Next ----", color: ConsoleColor.Red));
            //sw = new Stopwatch();
            sw.Start();
            await Parallel.PerformBackup(new DirectoryInfo(Path), new DirectoryInfo(backUp.FileContainer.DataDir), messagePrinter);
            sw.Stop();
            long parallel = sw.ElapsedMilliseconds;
            Console.WriteLine($"Time: {parallel} ms.");
            //Console.WriteLine($"Time: Classic: {classic} ms, Medium: {medium} ms, Parallel: {parallel} ms.");
            return MessageProvider.Success();
        }
        private static class Parallel
        {
            private static async Task<MessageHandler> BackUpFile(FileInfo file, FileInfo target, bool doArchive, CancellationToken cancellationToken)
            {
                try
                {
                    bool differenceFound = false;
                    FileInfo copyFile = new FileInfo($"{target.FullName}.butmp-{DateTime.UtcNow.Ticks}");
                    {
                        using StreamReader sourceStream = new StreamReader(file.FullName);
                        using StreamReader? targetStream = target.Exists ? new StreamReader(target.FullName) : null;
                        using StreamWriter copyStream = new StreamWriter(File.Create(copyFile.FullName));
                        differenceFound = targetStream is null || file.Length != target.Length;
                        char[] buffer = new char[MaxBlockSize];
                        char[] secondaryBuffer = new char[MaxBlockSize];
                        while (sourceStream.EndOfStream == false)
                        {
                            int saved = await sourceStream.ReadAsync(buffer, 0, MaxBlockSize);
                            Task write = copyStream.WriteAsync(buffer, 0, saved);
                            if (differenceFound == false && targetStream is StreamReader)
                            {
                                await targetStream.ReadAsync(secondaryBuffer, 0, saved);
                                for (int i = 0; i < saved; i++)
                                {
                                    if (buffer[i] != secondaryBuffer[i])
                                    {
                                        differenceFound = true;
                                        break;
                                    }
                                }
                            }
                            await write;
                        }
                    }
                    if (differenceFound)
                    {
                        copyFile.MoveTo(target.FullName, true);
                        return MessageProvider.Message($"Successfully copied file at '{file.FullName}' to '{target.FullName}'.", color: ConsoleColor.Green);
                    }
                    else
                    {
                        copyFile.Delete();
                        return MessageProvider.Message($"File at '{file.FullName}' already exists at '{target.FullName}'.");
                    }
                }
                catch(Exception ex)
                {
                    return MessageProvider.Message($"An error occurred while backing up file at '{file.FullName}':{Environment.NewLine}{ex.Message}", MessageCollections.Levels.Error);
                }
            }
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
                //yield return MessageProvider.Message("Starting...");
                foreach (var bucket in Miscellaneous.Interleaved(tasks))
                {
                    var t = await bucket;
                    yield return await t;
                }
                //while (tasks.Any())
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
        private static class Classic
        {
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
            internal static void ClassicBackup(string path)
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                Dir(directory);
                //sw.Stop();
                //return sw.ElapsedMilliseconds;
                //Console.WriteLine($"Sync Time: {sw.ElapsedMilliseconds} ms");
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
}
#endif

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BackingUpConsole.Utilities.Exceptions;

namespace BackingUpConsole.CoreFunctions.BackingUp
{
    internal static class Handler
    {
        private const int DefaultMaxBlockSize = 65536;
        private static int MaxBlockSize { get; set; }
        public static async Task<MessageHandler> PerformBackup(this BackUpFile backUp, MessagePrinter messagePrinter, bool useSequential, bool verbose, int maxBlockSize = DefaultMaxBlockSize)
        {
            MaxBlockSize = maxBlockSize;
            Stopwatch sw = new Stopwatch();
            if (useSequential)
            {
                sw.Start();
                await SequentialBackup.PerformBackup(backUp.Settings!.Settings.Paths.Select(path => new DirectoryInfo(path)), new DirectoryInfo(backUp.FileContainer.DataDir), verbose, messagePrinter);
                sw.Stop();
            }
            else
            {
                sw.Start();
                await Parallel.PerformBackup(backUp.Settings!.Settings.Paths.Select(path => new DirectoryInfo(path)), new DirectoryInfo(backUp.FileContainer.DataDir), verbose, messagePrinter);
                sw.Stop();
            }
            return MessageProvider.Message($"The process took {sw.ElapsedMilliseconds} ms in {(useSequential ? "sequential" : "parallel")} mode with a MaxBlockSize of {MaxBlockSize}.");
        }
        
        private static async Task<MessageHandler> BackUpFile(FileInfo file, FileInfo target, bool doArchive, CancellationToken cancellationToken)
        {
            try
            {
                bool differenceFound = (await Miscellaneous.FilesAreIdentical(file, target)) == false;
                if (differenceFound)
                {
                    byte[] buffer = new byte[MaxBlockSize];
                    await using FileStream sourceStream = new FileStream(file.FullName, FileMode.Open);
                    await using FileStream targetStream = Miscellaneous.CreateFileAndDirectoryIfNotExist(target);
                    int read = 1;
                    while (read > 0)
                    {
                        read = await sourceStream.ReadAsync(buffer, 0, MaxBlockSize, cancellationToken);
                        Task write = targetStream.WriteAsync(buffer, 0, read, cancellationToken);
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
                return MessageProvider.Message($"An error occurred while backing up file at '{file.FullName}':{Environment.NewLine}\t{ex.Message}", MessageCollections.Levels.Error);
            }
        }

        private static class Parallel
        {
            private static async Task<List<FileInfo>> EnumerateAllFilesRecursively(DirectoryInfo root, CancellationToken cancellationToken) => await EnumerateAllFilesRecursively(new DirectoryInfo[] { root }, cancellationToken);
            private static async Task<List<FileInfo>> EnumerateAllFilesRecursively(IEnumerable<DirectoryInfo> roots, CancellationToken cancellationToken)
            {
                List<FileInfo> files = new List<FileInfo>();
                foreach (DirectoryInfo root in roots)
                {
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
                }
                return files;
            }
            private static async IAsyncEnumerable<MessageHandler> BackupDirectory(IEnumerable<DirectoryInfo> directories, DirectoryInfo backupDir, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                yield return MessageProvider.Message("Beginning to enumerate all files...");
                List<FileInfo> files = await EnumerateAllFilesRecursively(directories, cancellationToken);
                yield return MessageProvider.Message($"Enumerating done! Found {files.Count} files.");
                List<Task<MessageHandler>> tasks = new List<Task<MessageHandler>>();

                string GetTargetPath(FileInfo file) => PathHandler.Combine(backupDir.FullName,
                    Environment.OSVersion.Platform switch
                    {
                        PlatformID.Unix => file.FullName[1..],
                        PlatformID.Win32NT => file.FullName.Replace(':', '-'),
                        _ => throw new OSNotSupportedException(Environment.OSVersion)
                    });
                tasks.AddRange(files.Select(file => Task.Run(() => BackUpFile(file, new FileInfo(GetTargetPath(file)), false, cancellationToken), cancellationToken)));
                foreach (var bucket in Miscellaneous.Interleaved(tasks))
                {
                    var t = await bucket;
                    yield return await t;
                }
            }
            internal static async Task PerformBackup(IEnumerable<DirectoryInfo> backupRoots, DirectoryInfo backupDir, bool verbose, MessagePrinter messagePrinter)
            {
                int messagesRead = 0;
                await foreach (MessageHandler message in BackupDirectory(backupRoots, backupDir, new CancellationTokenSource().Token))
                {
                    messagesRead++;
                    // Show the second message, containing the count of the files.
                    if (verbose == false && message.Level > MessageCollections.Levels.Warning && messagesRead != 2)
                        continue;
                    
                    messagePrinter.Print(message);
                    if(messagesRead == 2 && verbose == false)
                        messagePrinter.Print(MessageProvider.Message("Copying files... This may take a while."));
                }
                if(verbose == false)
                    messagePrinter.Print(MessageProvider.Message("Done.", color: ConsoleColor.Green));
            }
        }
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
            internal static async Task PerformBackup(IEnumerable<DirectoryInfo> backupRoots, DirectoryInfo backupDir, bool verbose, MessagePrinter messagePrinter)
            {
                foreach (DirectoryInfo backupRoot in backupRoots)
                    await Dir(backupRoot, backupDir, false, messagePrinter, new CancellationTokenSource().Token);
            }
        }
    }
}

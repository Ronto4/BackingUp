﻿using COMPATIBILITY = BackUp_0_3;
using BackingUpConsole.CoreFunctions;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace BackingUpConsole.Utilities.Commands
{
    public static class CommandCollections
    {
        public static Command RunFile => new Command("run");
        public static Command Exit => new Command("exit");
        public static Command Cd => new Command("cd");
        public static Command Dir => new Command("dir");
        public static Command Tilde => new Command("~");
        public static Command Reportlevel => new Command("reportlevel");
        public static Command Backup => new Command("backup");

        public static Command GetCommand(string cmd) => new Command(cmd);

        internal static Dictionary<string, CommandProperties> Properties = new Dictionary<string, CommandProperties>()
        {
            {"exit", new CommandProperties(0,0,Parse_Exit, Run_Exit) },
            {"cd", new CommandProperties(1,1,Parse_Cd, Run_Cd) },
            {"run", new CommandProperties(1,1,Parse_RunFile_Async, Run_RunFile_Async) },
            {"dir", new CommandProperties(0, 1, Parse_Dir, Run_Dir) },
            {"~", new CommandProperties(-1, -1, Parse_Tilde, Run_Tilde) },
            {"reportlevel", new CommandProperties(1,1,Parse_ReportLevel, Run_ReportLevel) },
            {"backup", new CommandProperties(1, 4, Parse_BackUp, Run_BackUp_Async) }
        };

        internal static Dictionary<string, string[]> ArgumentOrder = new Dictionary<string, string[]>()
        {
            {"exit", new string[] { } },
            {"cd", new string[] { "Path" } },
            {"run", new string[] { "Path" } },
            {"dir", new string[] { "Path" } },
            {"~", new string[] { } },
            {"reportlevel", new string[] { "Level" } },
            {"backup", new string[] {"Mode", "Path", "Name", "Usage" } }
        };

        private static (MessageHandler message, string? path) Parse_Exit(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE)), null);
        }
        private static (MessageHandler message, string? path) Run_Exit(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            Miscellaneous.ExitProgram(0, "User input or script");
            return (MessageProvider.InvalidMethodExecution(flags, args, "'Miscellaneous.ExitProgram(0, \"User input or script\");' was called, but the program did not stop.", silent: !flags.IsSet(Flags.VERBOSE)), null);
        }

        private static (MessageHandler message, string? path) Parse_Cd(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string targetPath = args[0];
            string newPath = PathHandler.Combine(paths.CurrentWorkingDirectory, targetPath);
            return Directory.Exists(newPath) ? (MessageProvider.ParseDirectoryChanged(silent: !flags.IsSet(Flags.VERBOSE)), newPath) : (MessageProvider.DirectoryNotFound(newPath, silent: !flags.IsSet(Flags.VERBOSE)), (string?)null);
        }
        private static (MessageHandler message, string? path) Run_Cd(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string targetPath = args[0];
            string newPath = PathHandler.Combine(paths.CurrentWorkingDirectory, targetPath);
            return (MessageProvider.DirectoryChanged(newPath, silent: !flags.IsSet(Flags.VERBOSE)), newPath);
        }

        private async static Task<(MessageHandler message, string? path)> Parse_RunFile_Async(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = PathHandler.Combine(paths.CurrentWorkingDirectory, args[0]);

            if (!File.Exists(path))
                return (MessageProvider.FileNotFound(path, silent: !flags.IsSet(Flags.VERBOSE)), null);

            if (!flags.IsSet(Flags.RUN) && !flags.IsSet(Flags.CHAIN_COMPILE))
                return (MessageProvider.ParseSuccess(path, silent: !flags.IsSet(Flags.VERBOSE)), null);

            Paths parsingPaths = (Paths)paths.Clone();
            int line = 0;
            parsingPaths.CurrentWorkingDirectory = Path.GetDirectoryName(path) ?? parsingPaths.CurrentWorkingDirectory;
            using StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                line++;
                string? cmds = sr.ReadLine();
                if (cmds == null)
                    return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected.", silent: !flags.IsSet(Flags.VERBOSE)), null);

                MessageHandler message;
                (message, parsingPaths) = await MainHandler.Compute(cmds, messagePrinter, parsingPaths, (UInt16)(flags & ~Flags.RUN), true);
                if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                    return (MessageProvider.ParseError(message, $"{path} at line {line}", silent: !flags.IsSet(Flags.VERBOSE)), parsingPaths.CurrentWorkingDirectory);
            }
            MessageHandler success = MessageProvider.ParseSuccess(path, silent: !flags.IsSet(Flags.VERBOSE));
            if (flags.IsSet(Flags.VERBOSE))
                messagePrinter.Print(success);
            return (success, null);
        }
        private async static Task<(MessageHandler message, string? path)> Run_RunFile_Async(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = PathHandler.Combine(paths.CurrentWorkingDirectory, args[0]);
            Paths usingPaths = (Paths)paths.Clone();
            usingPaths.CurrentWorkingDirectory = Path.GetDirectoryName(path) ?? usingPaths.CurrentWorkingDirectory;

            int line = 0;

            //if (flags.IsSet(Flags.COMPILE))
            //    messagePrinter.Print(MessageProvider.ParseSuccess());

            MessagePrinter localPrinter = (MessagePrinter)messagePrinter.Clone();


            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    line++;
                    string? cmds = sr.ReadLine();
                    if (cmds == null)
                        return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected.", silent: !flags.IsSet(Flags.VERBOSE)), null);

                    MessageHandler message;
                    (message, usingPaths) = await MainHandler.Compute(cmds, localPrinter, usingPaths, (UInt16)(flags & ~Flags.COMPILE));

                    if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                        return (MessageProvider.RuntimeError(message, $"{path} at line {line}", silent: !flags.IsSet(Flags.VERBOSE)), usingPaths.CurrentWorkingDirectory);

                    if (flags.IsSet(Flags.VERBOSE))
                        messagePrinter.Print(message);
                }
            }

            return (MessageProvider.ExecutionSuccess(path, silent: !flags.IsSet(Flags.VERBOSE)), null);
        }

        private static (MessageHandler message, string? path) Parse_Dir(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = args.Length > 0 ? PathHandler.Combine(paths.CurrentWorkingDirectory, args[0]) : paths.CurrentWorkingDirectory;

            return Directory.Exists(path) ? (MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE)), null) : (MessageProvider.DirectoryNotFound(path, silent: !flags.IsSet(Flags.VERBOSE)), (string?)null);
        }
        private static (MessageHandler message, string? path) Run_Dir(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = args.Length > 0 ? PathHandler.Combine(paths.CurrentWorkingDirectory, args[0]) : paths.CurrentWorkingDirectory;
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            DirectoryInfo[] subdirs = root.GetDirectories();
            string text = String.Empty;
            text += $"Content of directory {root.FullName}:{Environment.NewLine}";
            text += $"    Last modified    |  Size (in B)  | <DIR> | Name{Environment.NewLine}";
            var content = ((FileSystemInfo[])files).Concat(subdirs);
            content = from c in content orderby c.Name ascending select c;
            foreach(var entry in content)
            {
                text += $" {entry.LastWriteTime.ToString()} | {(entry is FileInfo f ? string.Format("{0,13:#,###,###,###}", f.Length) : "             ")} | {(entry is DirectoryInfo ? "<DIR>" : "     ")} | {entry.Name}{Environment.NewLine}";
            }
            return (MessageProvider.Message(text, silent: !flags.IsSet(Flags.VERBOSE)), null);
        }

        private static (MessageHandler message, string? path) Parse_Tilde(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (MessageProvider.CompatibilityMode(silent: !flags.IsSet(Flags.VERBOSE)), null);
        }
        private static (MessageHandler message, string? path) Run_Tilde(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            COMPATIBILITY.Program.Main(args);
            return (MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE)), null);
        }

        private static (MessageHandler message, string? path) Parse_ReportLevel(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            bool success = Enum.TryParse(typeof(MessageCollections.Levels), args[0], true, out object? l);
            if (!success)
                return (MessageProvider.UnknownReportLevel(args[0], silent: !flags.IsSet(Flags.VERBOSE)), null);

            MessageCollections.Levels level = (MessageCollections.Levels)l!;
            MessageHandler message = messagePrinter.ChangeLevel(level, true);
            if (!message.IsSuccess(true, messagePrinter))
                return (message, null);

            return (MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE)), null);
        }
        private static (MessageHandler message, string? path) Run_ReportLevel(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            MessageCollections.Levels level = (MessageCollections.Levels)Enum.Parse(typeof(MessageCollections.Levels), args[0], true);
            MessageHandler message = messagePrinter.ChangeLevel(level);
            if (!message.IsSuccess(false, messagePrinter))
                return (message, null);

            return (MessageProvider.ReportLevelChanged(level, silent: !flags.IsSet(Flags.VERBOSE)), null);
        }

        private static (MessageHandler message, string? path) Parse_BackUp(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (CommandHandler.Parse(args, flags, paths, messagePrinter), null);
        }

        private async static Task<(MessageHandler message, string? path)> Run_BackUp_Async(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (await CoreFunctions.CommandHandler.Run(args, flags, paths, messagePrinter), null);
        }
    }
}

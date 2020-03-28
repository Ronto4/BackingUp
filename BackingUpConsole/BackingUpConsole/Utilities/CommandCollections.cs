using COMPATIBILITY = BackUp_0_3;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BackingUpConsole.Utilities.Commands
{
    public static class CommandCollections
    {
        public static Command RunFile => new Command("run");
        public static Command Exit => new Command("exit");
        public static Command Cd => new Command("cd");
        public static Command Dir => new Command("dir");
        public static Command Tilde => new Command("~");

        public static Command GetCommand(string cmd) => new Command(cmd);

        internal static Dictionary<string, CommandProperties> Properties = new Dictionary<string, CommandProperties>()
        {
            {"exit", new CommandProperties(0,0,Parse_Exit, Run_Exit) },
            {"cd", new CommandProperties(1,1,Parse_Cd, Run_Cd) },
            {"run", new CommandProperties(1,1,Parse_RunFile, Run_RunFile) },
            {"dir", new CommandProperties(0, 1, Parse_Dir, Run_Dir) },
            {"~", new CommandProperties(-1, -1, Parse_Tilde, Run_Tilde) }
        };

        private static (MessageHandler message, string? path) Parse_Exit(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (MessageProvider.ParseSuccess(), null);
        }
        private static (MessageHandler message, string? path) Run_Exit(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            Miscellaneous.ExitProgram(0, "User input or script");
            return (MessageProvider.InvalidMethodExecution(flags, args, "'Miscellaneous.ExitProgram(0, \"User input or script\");' was called, but the program did not stop."), null);
        }

        private static (MessageHandler message, string? path) Parse_Cd(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string targetPath = args[0];
            string newPath = PathHandler.Combine(paths.currentWorkingDirectory, targetPath);
            return Directory.Exists(newPath) ? (MessageProvider.ParseDirectoryChanged(), newPath) : (MessageProvider.DirectoryNotFound(newPath), (string?)null);
        }
        private static (MessageHandler message, string? path) Run_Cd(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string targetPath = args[0];
            string newPath = PathHandler.Combine(paths.currentWorkingDirectory, targetPath);
            return (MessageProvider.DirectoryChanged(newPath), newPath);
        }

        private static (MessageHandler message, string? path) Parse_RunFile(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = PathHandler.Combine(paths.currentWorkingDirectory, args[0]);

            if (!File.Exists(path))
                return (MessageProvider.FileNotFound(path), null);

            if (!flags.IsSet(Flags.RUN) && !flags.IsSet(Flags.CHAIN_COMPILE))
                return (MessageProvider.ParseSuccess(), null);

            Paths parsingPaths = paths;
            int line = 0;
            parsingPaths.currentWorkingDirectory = Path.GetDirectoryName(path) ?? parsingPaths.currentWorkingDirectory;
            using StreamReader sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                line++;
                string? cmds = sr.ReadLine();
                if (cmds == null)
                    return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected."), null);

                MessageHandler message;
                (message, parsingPaths) = MainHandler.Compute(cmds, messagePrinter, parsingPaths, (UInt16)(flags & ~Flags.RUN), true);
                if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                    return (MessageProvider.ParseError(message, $"{path} at line {line}"), parsingPaths.currentWorkingDirectory);
            }
            return (MessageProvider.ParseSuccess(), null);
        }
        private static (MessageHandler message, string? path) Run_RunFile(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = PathHandler.Combine(paths.currentWorkingDirectory, args[0]);
            Paths usingPaths = paths;
            usingPaths.currentWorkingDirectory = Path.GetDirectoryName(path) ?? usingPaths.currentWorkingDirectory;

            int line = 0;

            if (flags.IsSet(Flags.COMPILE))
                messagePrinter.Print(MessageProvider.ParseSuccess());


            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    line++;
                    string? cmds = sr.ReadLine();
                    if (cmds == null)
                        return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected."), null);

                    MessageHandler message;
                    (message, usingPaths) = MainHandler.Compute(cmds, messagePrinter, usingPaths, (UInt16)(flags & ~Flags.COMPILE));

                    if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                        return (MessageProvider.RuntimeError(message, $"{path} at line {line}"), usingPaths.currentWorkingDirectory);
                }
            }

            return (MessageProvider.ExecutionSuccess(), null);
        }

        private static (MessageHandler message, string? path) Parse_Dir(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = args.Length > 0 ? PathHandler.Combine(paths.currentWorkingDirectory, args[0]) : paths.currentWorkingDirectory;

            return Directory.Exists(path) ? (MessageProvider.ParseSuccess(), null) : (MessageProvider.DirectoryNotFound(path), (string?)null);
        }
        private static (MessageHandler message, string? path) Run_Dir(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string path = args.Length > 0 ? PathHandler.Combine(paths.currentWorkingDirectory, args[0]) : paths.currentWorkingDirectory;
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
            return (MessageProvider.Message(text), null);
        }

        private static (MessageHandler message, string? path) Parse_Tilde(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return (MessageProvider.CompatibilityMode(), null);
        }
        private static (MessageHandler message, string? path) Run_Tilde(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            COMPATIBILITY.Program.Main(args);
            return (MessageProvider.Success(), null);
        }
    }
}

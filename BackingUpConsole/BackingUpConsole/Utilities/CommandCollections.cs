using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackingUpConsole.Utilities.Commands
{
    public static class CommandCollections
    {
        public static Command RunFile => new Command("run");
        public static Command Exit => new Command("exit");
        public static Command Cd => new Command("cd");

        public static Command GetCommand(string cmd) => new Command(cmd);

        internal static Dictionary<string, CommandProperties> Properties = new Dictionary<string, CommandProperties>()
        {
            {"exit", new CommandProperties(0,0,Parse_Exit, Run_Exit) },
            {"cd", new CommandProperties(1,1,Parse_Cd, Run_Cd) },
            {"run", new CommandProperties(1,1,Parse_RunFile, Run_RunFile) }
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
            return Directory.Exists(newPath) ? (MessageProvider.ParseSuccess(), null) : (MessageProvider.DirectoryNotFound(newPath), (string?)null);
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
    }
}

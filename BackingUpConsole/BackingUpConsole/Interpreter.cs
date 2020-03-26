#nullable enable

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.IO;

namespace BackingUpConsole
{
    class Interpreter
    {
        public static (MessageHandler, string?) Interprete(Command command, string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
        {
            if (command.cmd.StartsWith(';'))
                return (MessageProvider.Success(), null);

            //Console.Write(command.cmd);
            //for (int i = 0; i < args.Length; i++)
            //{
            //    Console.Write($"\"{args[i]}\"");
            //}
            //Console.WriteLine($": {Convert.ToString(flags, 2)}");
            messagePrinter.Print(MessageProvider.ExecutionDebug(command, flags, args, paths));

            if (command == CommandCollections.RunFile)
                return RunFile(args, messagePrinter, flags, paths);

            else if (command == CommandCollections.Exit)
                return Exit(args, messagePrinter, flags, paths);

            else if (command == CommandCollections.Cd)
                return Cd(args, messagePrinter, flags, paths);

            else
                return (MessageProvider.UnknownCommand(command.cmd), null);
        }

        private static bool CheckArgsLength(string[] args, int min, int max) => (args.Length <= max || max == -1) && (args.Length >= min || min == -1);

        private static (MessageHandler, string?) Cd(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
        {
            string targetPath;
            string currentPath = paths.currentWorkingDirectory;
            string newPath;
            //currentPath += currentPath.EndsWith('\\') ? String.Empty : "\\";
            if ((flags & Flags.COMPILE) != 0)
            {
                if (!CheckArgsLength(args, 1, 1))
                    return (MessageProvider.IncorrectArgumentCount(), null);

                targetPath = args[0];
                //targetPath = targetPath.StartsWith('\\') ? targetPath.Substring(1) : targetPath;
                newPath = PathHandler.Combine(currentPath, targetPath);
                if (!Directory.Exists(newPath))
                    return (MessageProvider.DirectoryNotFound(newPath), null);
            }
            else
            {
                targetPath = args[0];
                newPath = PathHandler.Combine(currentPath, targetPath);
                //targetPath = targetPath.StartsWith('\\') ? targetPath.Substring(1) : targetPath;
            }
            newPath = PathHandler.Flatten(newPath);
            if ((flags & Flags.RUN) == 0)
                return (MessageProvider.ParseDirectoryChanged(), newPath);

            //string newPath = currentPath + targetPath;
            return (MessageProvider.DirectoryChanged(newPath), newPath);
        }

        private static (MessageHandler, string?) Exit(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
        {
            if((flags & Flags.COMPILE) != 0){
                if (!CheckArgsLength(args, 0, 0))
                    return (MessageProvider.IncorrectArgumentCount(), null);
            }

            if ((flags & Flags.RUN) == 0)
                return (MessageProvider.Success(), null);

            Miscellaneous.ExitProgram(0, "User input or script");
            return (MessageProvider.InvalidMethodExecution(flags, args, "'Miscellaneous.ExitProgram(0, \"User input or script\");' was called, but the program did not stop."), null);
        }

        private static (MessageHandler, string?) RunFile(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
        {
            string path;
            int line;
            bool compiled;
            Paths localPaths;
            if (compiled = (flags & Flags.COMPILE) != 0)
            {
                if (!CheckArgsLength(args, 1, 1))
                    return (MessageProvider.IncorrectArgumentCount(), null);

                path = PathHandler.Flatten(PathHandler.Combine(paths.currentWorkingDirectory, args[0]));

                if (!File.Exists(path))
                    return (MessageProvider.FileNotFound(path), null);

                if (((flags & Flags.RUN) == 0) && ((flags & Flags.CHAIN_COMPILE) == 0))
                    return (MessageProvider.ParseSuccess(), null);

                line = 0;

                //if ((flags & Flags.NO_COMPILE) == 0)
                //{
                localPaths.currentWorkingDirectory = Path.GetDirectoryName(path);
                Paths parsingPaths = localPaths;
                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        line++;
                        string? cmds = sr.ReadLine();
                        if (cmds == null)
                            return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected."), null);

                        string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                        Command cmd = CommandCollections.GetCommand(parts[0]);
                        string[] cmdargs = new string[parts.Length - 1];
                        for (int i = 0; i < cmdargs.Length; i++)
                        {
                            cmdargs[i] = parts[i + 1];
                        }
                        //Command cmd = CommandCollections.GetCommand(sr.ReadLine());
                        (MessageHandler, string?) result = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.RUN), parsingPaths);
                        if (result.Item1 == MessageProvider.ParseDirectoryChanged())
                            parsingPaths.currentWorkingDirectory = result.Item2;
                        //if (result.Item1 != MessageProvider.Success() && result.Item1 != MessageProvider.ParseSuccess())
                        //if(!result.Item1.IsSuccess(true))
                        if(result.Item1.Level != MessageCollections.Levels.Debug && result.Item1.Level != MessageCollections.Levels.Information)
                            return (MessageProvider.ParseError(result.Item1, $"{path} at line {line}"), result.Item2);
                    }
                }

                //if ((flags & Flags.ONLY_COMPILE) != 0)
                //    return MessageProvider.ParseSuccess();

                //messagePrinter.Print(MessageProvider.ParseSuccess());
                //}
                //Console.WriteLine("Executing...");
                //return MessageProvider.ParseSuccess();
            }
            else
            {
                path = PathHandler.Combine(paths.currentWorkingDirectory, args[0]);
                localPaths.currentWorkingDirectory = Path.GetDirectoryName(path);
            }
            line = 0;

            if ((flags & Flags.RUN) == 0)
                return (compiled
                        ? MessageProvider.ParseSuccess()
                        : MessageProvider.Success()
                       , null);
            if (compiled)
                messagePrinter.Print(MessageProvider.ParseSuccess());

            Paths usingPaths = localPaths;

            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    line++;
                    string? cmds = sr.ReadLine();
                    if (cmds == null)
                        return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected."), null);

                    string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                    Command cmd = CommandCollections.GetCommand(parts[0]);
                    string[] cmdargs = new string[parts.Length - 1];
                    for (int i = 0; i < cmdargs.Length; i++)
                    {
                        cmdargs[i] = parts[i + 1];
                    }
                    (MessageHandler, string?) result = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.COMPILE), usingPaths);
                    if (result.Item1 == MessageProvider.DirectoryChanged(String.Empty))
                        usingPaths.currentWorkingDirectory = result.Item2;
                    //if (result.Item1 != MessageProvider.Success())
                    //if(!result.Item1.IsSuccess(false))
                    if (result.Item1.Level != MessageCollections.Levels.Debug && result.Item1.Level != MessageCollections.Levels.Information)
                        return (MessageProvider.RuntimeError(result.Item1, $"{path} at line {line}"), result.Item2);
                }
            }

            return (MessageProvider.ExecutionSuccess(), null);
        }
    }
}

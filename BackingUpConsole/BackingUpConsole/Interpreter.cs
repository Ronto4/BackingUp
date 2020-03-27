﻿using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.IO;

namespace BackingUpConsole
{
    class Interpreter
    {
        public static (MessageHandler message, string? path) Interprete(Command command, string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
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

        private static (MessageHandler message, string? path) Cd(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
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

        private static (MessageHandler message, string? path) Exit(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
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

        private static (MessageHandler message, string? path) RunFile(string[] args, MessagePrinter messagePrinter, UInt16 flags, in Paths paths)
        {
            string path;
            int line;
            bool compiled;
            Paths localPaths = paths;
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
                localPaths.currentWorkingDirectory = Path.GetDirectoryName(path) ?? localPaths.currentWorkingDirectory;
                Paths parsingPaths = localPaths;
                using StreamReader sr = new StreamReader(path);
                while (!sr.EndOfStream)
                {
                    line++;
                    string? cmds = sr.ReadLine();
                    if (cmds == null)
                        return (MessageProvider.InvalidMethodExecution(flags, args, "ReadLine returned null, when EOF was not detected."), null);

                    MessageHandler message;
                    (message, parsingPaths) = MainHandler.Compute(cmds, messagePrinter, parsingPaths, (UInt16)(flags & ~Flags.RUN), true);

                    //string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                    //Command cmd = CommandCollections.GetCommand(parts[0]);
                    ////string[] cmdargs = new string[parts.Length - 1];
                    ////for (int i = 0; i < cmdargs.Length; i++)
                    ////{
                    ////    cmdargs[i] = parts[i + 1];
                    ////}
                    //string[] cmdargs = parts[1..];
                    ////Command cmd = CommandCollections.GetCommand(sr.ReadLine());
                    //(MessageHandler message, string? newPath) = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.RUN), parsingPaths);
                    //if (message == MessageProvider.ParseDirectoryChanged())
                    //    parsingPaths.currentWorkingDirectory = newPath ?? parsingPaths.currentWorkingDirectory;
                    ////if (result.Item1 != MessageProvider.Success() && result.Item1 != MessageProvider.ParseSuccess())
                    //if(!result.Item1.IsSuccess(true))
                    if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                        return (MessageProvider.ParseError(message, $"{path} at line {line}"), parsingPaths.currentWorkingDirectory);
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
                localPaths.currentWorkingDirectory = Path.GetDirectoryName(path) ?? localPaths.currentWorkingDirectory;
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

                    MessageHandler message;
                    (message, usingPaths) = MainHandler.Compute(cmds, messagePrinter, usingPaths, (UInt16)(flags & ~Flags.COMPILE));

                    //string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                    //Command cmd = CommandCollections.GetCommand(parts[0]);
                    ////string[] cmdargs = new string[parts.Length - 1];
                    ////for (int i = 0; i < cmdargs.Length; i++)
                    ////{
                    ////    cmdargs[i] = parts[i + 1];
                    ////}
                    //string[] cmdargs = parts[1..];
                    //(MessageHandler message, string? newPath) = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.COMPILE), usingPaths);
                    //if (message == MessageProvider.DirectoryChanged(String.Empty))
                    //    usingPaths.currentWorkingDirectory = newPath ?? usingPaths.currentWorkingDirectory;
                    //if (result.Item1 != MessageProvider.Success())
                    //if(!result.Item1.IsSuccess(false))
                    if (message.Level != MessageCollections.Levels.Debug && message.Level != MessageCollections.Levels.Information)
                        return (MessageProvider.RuntimeError(message, $"{path} at line {line}"), usingPaths.currentWorkingDirectory);
                }
            }

            return (MessageProvider.ExecutionSuccess(), null);
        }
    }
}

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.IO;

namespace BackingUpConsole
{
    class Interpreter
    {
        public static MessageHandler Interprete(Command command, string[] args, MessagePrinter messagePrinter, UInt16 flags)
        {
            if (command.cmd.StartsWith(';'))
                return MessageProvider.Success();

            if (command == CommandCollections.RunFile)
                return RunFile(args, messagePrinter, flags);

            else if (command == CommandCollections.Exit)
                return Exit(args, messagePrinter, flags);

            else
                return MessageProvider.UnknownCommand(command.cmd);
        }

        private static bool CheckArgsLength(string[] args, int min, int max) => (args.Length <= max || max == -1) && (args.Length >= min || min == -1);

        private static MessageHandler Exit(string[] args, MessagePrinter messagePrinter, UInt16 flags)
        {
            if (!CheckArgsLength(args, 0, 0))
                return MessageProvider.IncorrectArgumentCount();

            return MessageProvider.QuitProgram();
        }

        private static MessageHandler RunFile(string[] args, MessagePrinter messagePrinter, UInt16 flags)
        {
            if (!CheckArgsLength(args, 1, 1))
                return MessageProvider.IncorrectArgumentCount();

            string path = args[0];

            if (!File.Exists(path))
                return MessageProvider.FileNotFound(path);

            if (((flags & Flags.ONLY_COMPILE) != 0) && ((flags & Flags.CHAIN_COMPILE) == 0))
                return MessageProvider.Success();

            int line = 0;

            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    line++;
                    string cmds = sr.ReadLine();
                    string[] parts = cmds.Split(" ");
                    Command cmd = CommandCollections.GetCommand(parts[0]);
                    string[] cmdargs = new string[parts.Length - 1];
                    for (int i = 0; i < cmdargs.Length; i++)
                    {
                        cmdargs[i] = parts[i + 1];
                    }
                    //Command cmd = CommandCollections.GetCommand(sr.ReadLine());
                    MessageHandler result = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags | Flags.ONLY_COMPILE));
                    if (result != MessageProvider.Success() && result != MessageProvider.ParseSuccess())
                        return MessageProvider.ParseError(result, $"{path} at line {line}");
                }
            }

            return MessageProvider.ParseSuccess();
        }
    }
}

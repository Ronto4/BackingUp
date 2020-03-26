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

            //Console.Write(command.cmd);
            //for (int i = 0; i < args.Length; i++)
            //{
            //    Console.Write($"\"{args[i]}\"");
            //}
            //Console.WriteLine($": {Convert.ToString(flags, 2)}");
            messagePrinter.Print(MessageProvider.ExecutionDebug(command, flags, args));

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
            if((flags & Flags.COMPILE) != 0){
                if (!CheckArgsLength(args, 0, 0))
                    return MessageProvider.IncorrectArgumentCount();
            }

            if ((flags & Flags.RUN) == 0)
                return MessageProvider.Success();

            //return MessageProvider.QuitProgram();
            Miscellaneous.ExitProgram(0, "User input or script");
            return MessageProvider.RuntimeError(MessageProvider.InvalidMethodExecution(nameof(Exit), flags, args, "'Miscellaneous.ExitProgram(0, \"User input or script\");' was called, but the program did not stop."), "Interpreter.Exit");
        }

        private static MessageHandler RunFile(string[] args, MessagePrinter messagePrinter, UInt16 flags)
        {
            string path;
            int line;
            bool compiled;
            if (compiled = (flags & Flags.COMPILE) != 0)
            {
                if (!CheckArgsLength(args, 1, 1))
                    return MessageProvider.IncorrectArgumentCount();

                path = args[0];

                if (!File.Exists(path))
                    return MessageProvider.FileNotFound(path);

                if (((flags & Flags.RUN) == 0) && ((flags & Flags.CHAIN_COMPILE) == 0))
                    return MessageProvider.ParseSuccess();

                line = 0;

                //if ((flags & Flags.NO_COMPILE) == 0)
                //{

                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        line++;
                        string cmds = sr.ReadLine();
                        string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                        //string[] parts = cmds.Split(" ");
                        Command cmd = CommandCollections.GetCommand(parts[0]);
                        string[] cmdargs = new string[parts.Length - 1];
                        for (int i = 0; i < cmdargs.Length; i++)
                        {
                            cmdargs[i] = parts[i + 1];
                        }
                        //Command cmd = CommandCollections.GetCommand(sr.ReadLine());
                        MessageHandler result = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.RUN));
                        if (result != MessageProvider.Success() && result != MessageProvider.ParseSuccess())
                            return MessageProvider.ParseError(result, $"{path} at line {line}");
                    }
                }

                //if ((flags & Flags.ONLY_COMPILE) != 0)
                //    return MessageProvider.ParseSuccess();

                //messagePrinter.Print(MessageProvider.ParseSuccess());
                //}
                //Console.WriteLine("Executing...");
                //return MessageProvider.ParseSuccess();
                //compiled = true;
            }
            else
            {
                path = args[0];
                //compiled = false;
            }
            line = 0;

            if ((flags & Flags.RUN) == 0)
                return compiled
                        ? MessageProvider.ParseSuccess()
                        : MessageProvider.Success();
            if (compiled)
                messagePrinter.Print(MessageProvider.ParseSuccess());

            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    line++;
                    string cmds = sr.ReadLine();
                    string[] parts = Miscellaneous.CommandLineToArgs(cmds);
                    Command cmd = CommandCollections.GetCommand(parts[0]);
                    string[] cmdargs = new string[parts.Length - 1];
                    for (int i = 0; i < cmdargs.Length; i++)
                    {
                        cmdargs[i] = parts[i + 1];
                    }
                    MessageHandler result = Interprete(cmd, cmdargs, messagePrinter, (UInt16)(flags & ~Flags.COMPILE));
                    if (result != MessageProvider.Success())
                        return MessageProvider.RuntimeError(result, $"{path} at line {line}");
                }
            }

            return MessageProvider.Success();
        }
    }
}

#define DEBUG_MSG

#nullable enable

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;

namespace BackingUpConsole
{
    class MainHandler
    {
        static void Main(string[] args)
        {
#if DEBUG_MSG
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Debug, System.ConsoleColor.Gray);
#else
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Information, System.ConsoleColor.Gray);
#endif
            //string currentWorkingDirectory = Environment.CurrentDirectory;
            Paths paths = new Paths();
            paths.currentWorkingDirectory = Environment.CurrentDirectory;
            if (args.Length > 0)
            {
                (MessageHandler, string?) result = CLIInterpreter(args, messagePrinter, paths);
                if (result.Item1 == MessageProvider.DirectoryChanged(String.Empty))
                    paths.currentWorkingDirectory = result.Item2;

                messagePrinter.Print(result.Item1);
                //if (result == MessageProvider.QuitProgram())
                //    return;
            }
            bool exit = false;
            while (!exit)
            {
                Console.Write($"{paths.currentWorkingDirectory}>");
                string input = Console.ReadLine();
                string[] arg = Miscellaneous.CommandLineToArgs(input);
                (MessageHandler, string?) result = CLIInterpreter(arg, messagePrinter, paths);
                if (result.Item1 == MessageProvider.DirectoryChanged(String.Empty))
                    paths.currentWorkingDirectory = result.Item2;

                messagePrinter.Print(result.Item1);
                //if (result == MessageProvider.QuitProgram())
                //    exit = true;
            }
        }

        private static (MessageHandler, string?) CLIInterpreter(string[] args, MessagePrinter messagePrinter, Paths paths)
        {
            Command cmd = CommandCollections.GetCommand(args[0]);
            string[] arg = new string[args.Length - 1];
            for (int i = 0; i < arg.Length; i++)
            {
                arg[i] = args[i + 1];
            }
            return Interpreter.Interprete(cmd, arg, messagePrinter, (UInt16)(0x0 | Flags.CHAIN_COMPILE | Flags.COMPILE | Flags.RUN), paths);
            //return MessageProvider.QuitProgram();
        }



    }
}
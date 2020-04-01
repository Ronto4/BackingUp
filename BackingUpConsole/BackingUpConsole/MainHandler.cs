#define DEBUG_MSG

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Threading.Tasks;

namespace BackingUpConsole
{
    internal static class MainHandler
    {
        static async Task Main(string[] args)
        {
#if DEBUG_MSG
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Debug, System.ConsoleColor.Gray);
#else
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Information, System.ConsoleColor.Gray);
#endif
            //string currentWorkingDirectory = Environment.CurrentDirectory;
            Paths paths = new Paths
            {
                CurrentWorkingDirectory = Environment.CurrentDirectory
            };
            if (args.Length > 0)
            {
                (MessageHandler message, string? path) = await CLIInterpreter(args, messagePrinter, paths);
                if (message == MessageProvider.DirectoryChanged(String.Empty))
                    paths.CurrentWorkingDirectory = path ?? paths.CurrentWorkingDirectory;

                messagePrinter.Print(message);
                //if (result == MessageProvider.QuitProgram())
                //    return;
            }
            bool exit = false;
            while (!exit)
            {
                Console.Write($"{paths.CurrentWorkingDirectory}>");
                string input = Console.ReadLine();
                //string[] arg = Miscellaneous.CommandLineToArgs(input);
                //(MessageHandler message, string? path) = CLIInterpreter(arg, messagePrinter, paths);
                //if (message == MessageProvider.DirectoryChanged(String.Empty))
                //    paths.currentWorkingDirectory = path ?? paths.currentWorkingDirectory;
                MessageHandler message;
                (message, paths) = await Compute(input, messagePrinter, paths, Flags.DEFAULT_FLAGS);

                messagePrinter.Print(message);
                //if (result == MessageProvider.QuitProgram())
                //    exit = true;
            }
        }

        private async static Task<(MessageHandler, string?)> CLIInterpreter(string[] args, MessagePrinter messagePrinter, Paths paths)
        {
            Command cmd = CommandCollections.GetCommand(args[0]);
            //string[] arg = new string[args.Length - 1];
            //for (int i = 0; i < arg.Length; i++)
            //{
            //    arg[i] = args[i + 1];
            //}
            string[] arg = args[1..];
            return await Interpreter.Interprete(cmd, arg, messagePrinter, Flags.DEFAULT_FLAGS, paths);
            //return MessageProvider.QuitProgram();
        }

        public async static Task<(MessageHandler message, Paths paths)> Compute(string input, MessagePrinter messagePrinter, Paths paths, UInt16 flags, bool compile = false)
        {
            string[] arg = Miscellaneous.CommandLineToArgs(input);
            Command cmd = CommandCollections.GetCommand(arg[0]);
            string[] args = arg[1..];
            (MessageHandler message, string? path) = await Interpreter.Interprete(cmd, args, messagePrinter, flags, paths);
            if (compile ? message == MessageProvider.ParseDirectoryChanged() : message == MessageProvider.DirectoryChanged())
                paths.CurrentWorkingDirectory = path ?? paths.CurrentWorkingDirectory;

            return (message, paths);
        }

    }
}
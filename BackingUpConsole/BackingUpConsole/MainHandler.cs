#define DEBUG_MSG

using System;
using BackingUpConsole.Utilities.Messages;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities;

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
            if (args.Length > 0)
            {
                MessageHandler result = CLIInterpreter(args, messagePrinter);
                messagePrinter.Print(result);
                if (result == MessageProvider.QuitProgram())
                    return;
            }
        }

        private static MessageHandler CLIInterpreter(string[] args, MessagePrinter messagePrinter)
        {
            Command cmd = CommandCollections.GetCommand(args[0]);
            string[] arg = new string[args.Length - 1];
            for (int i = 0; i < arg.Length; i++)
            {
                arg[i] = args[i + 1];
            }
            return Interpreter.Interprete(cmd, arg, messagePrinter, (UInt16)(0x0 | Flags.CHAIN_COMPILE));
            //return MessageProvider.QuitProgram();
        }
    }
}
#define DEBUG_MSG

using BackingUpConsole.Utilities.Messages;

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
                MessageHandler result = CLIInterpreter(args);
                if (result == MessageProvider.QuitProgram())
                {
                    messagePrinter.Print(result);
                    return;
                }
            }
        }

        private static MessageHandler CLIInterpreter(string[] args)
        {
            return MessageProvider.QuitProgram();
        }
    }
}
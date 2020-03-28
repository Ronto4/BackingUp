using System;

namespace BackingUpConsole.Utilities.Messages
{
    public class MessagePrinter
    {
        //Attributes
        private MessageCollections.Levels Level;
        private ConsoleColor DefaultColor;

        //Constructors
        public MessagePrinter(MessageCollections.Levels level, ConsoleColor? defaultColor)
        {
            Level = level;
            DefaultColor = defaultColor ?? ConsoleColor.Gray;
        }
        public MessagePrinter(MessageCollections.Levels level) : this(level, null) { }

        //Methods
        public void ChangeLevel(MessageCollections.Levels newLevel) => Level = newLevel;
        public void ChangeDefaultColor(ConsoleColor newDefaultColor) => DefaultColor = newDefaultColor;

        public void Print(MessageHandler message)
        {
            if (Level < message.Level)
                return;

            Console.ForegroundColor = message.Color ?? MessageCollections.Colors[message.Level];
            Console.WriteLine(message.Message);
            Console.ForegroundColor = DefaultColor;
        }

        public bool AskContinue(MessageHandler message)
        {
            do
            {
                Console.ForegroundColor = message.Color ?? MessageCollections.Colors[message.Level];
                Console.Write("Would you nevertheless like to continue? [y|n] ");
                string input = Console.ReadLine();
                Console.ForegroundColor = DefaultColor;
                bool y;
                if ((y = (input == "y")) || (input == "n"))
                    return y;
            } while (true);
        }
    }
}

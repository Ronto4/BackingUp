using System;

namespace BackingUpConsole.Utilities.Messages
{
    public class MessagePrinter : ICloneable
    {
        //Attributes
        private MessageCollections.Levels Level;
        private ConsoleColor DefaultColor;

        //Constructors
        public MessagePrinter(MessageCollections.Levels level, ConsoleColor? defaultColor = null)
        {
            Level = level;
            DefaultColor = defaultColor ?? ConsoleColor.Gray;
        }
        public MessagePrinter(MessagePrinter printer) : this(printer.Level, printer.DefaultColor) { }

        //Methods
        public MessageHandler ChangeLevel(MessageCollections.Levels newLevel, bool parsing = false)
        {
            if (newLevel < MessageCollections.Levels.Warning)
                return MessageProvider.Message($"The chosen minimum level to report messages cannot be set to '{Enum.GetName(typeof(MessageCollections.Levels), newLevel)}'. The minimum required level is 'Warning'.", MessageCollections.Levels.Error);

            if (!parsing) Level = newLevel;
            return MessageProvider.Success();
        }
        public void ChangeDefaultColor(ConsoleColor newDefaultColor) => DefaultColor = newDefaultColor;

        public void Print(MessageHandler message)
        {
            if (Level < message.Level || (Level < MessageCollections.Levels.Debug && message.Silent && message.Level >= MessageCollections.Levels.Information))
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

        public object Clone() => new MessagePrinter(this);
    }
}

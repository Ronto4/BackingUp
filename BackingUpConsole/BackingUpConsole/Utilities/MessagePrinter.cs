using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace BackingUpConsole.Utilities.Messages
{
    public class MessagePrinter
    {
        //Attributes
        private MessageCollections.Levels Level;
        private ConsoleColor DefaultColor;

        //Constructors
        public MessagePrinter(MessageCollections.Levels level, ConsoleColor defaultColor)
        {
            Level = level;
            DefaultColor = defaultColor;
        }

        public void ChangeLevel(MessageCollections.Levels newLevel) => Level = newLevel;
        public void ChangeDefaultColor(ConsoleColor newDefaultColor) => DefaultColor = newDefaultColor;

        public void Print(MessageHandler message)
        {
            if (Level < message.Level)
                return;

            Console.ForegroundColor = 
                message.Color != null 
                    ? (ConsoleColor)message.Color 
                    : DefaultColor;
            Console.WriteLine(message.Message);
            Console.ForegroundColor = DefaultColor;
        }
    }
}

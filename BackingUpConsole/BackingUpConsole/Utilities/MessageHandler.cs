#nullable enable

using System;

namespace BackingUpConsole.Utilities.Messages
{
    public class MessageHandler
    {
        //Attributes
        public MessageCollections.Codes Code { get; }
        public string Message { get; }
        public MessageCollections.Levels Level { get; }
        public ConsoleColor? Color { get; }
        //Constructor
        public MessageHandler(MessageCollections.Codes code, string message, MessageCollections.Levels level, ConsoleColor color)
        {
            Code = code;
            Message = message;
            Level = level;
            Color = color;
        }
        public MessageHandler(MessageCollections.Codes code, string message, MessageCollections.Levels level)
        {
            Code = code;
            Message = message;
            Level = level;
        }
        //overriding operators
        public static bool operator ==(MessageHandler left, MessageHandler right) => (left.Code == right.Code);
        public static bool operator !=(MessageHandler left, MessageHandler right) => !(left == right);
    }
}

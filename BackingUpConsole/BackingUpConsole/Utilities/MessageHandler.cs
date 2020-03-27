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
        public MessageHandler(MessageCollections.Codes code, string message, MessageCollections.Levels level, ConsoleColor? color)
        {
            Code = code;
            Message = message;
            Level = level;
            Color = color;
        }
        public MessageHandler(MessageCollections.Codes code, string message, MessageCollections.Levels level) : this(code, message, level, null) { }

        //overriding operators
        public static bool operator ==(MessageHandler left, MessageHandler right) => (left.Code == right.Code);
        public static bool operator !=(MessageHandler left, MessageHandler right) => !(left == right);

        //Methods
        public bool IsSuccess(bool parsing) => this == MessageProvider.Success() || ((this == MessageProvider.ParseSuccess() || this == MessageProvider.ParseDirectoryChanged()) && parsing);

        public override bool Equals(object? message) => (message is MessageHandler m)
                                                            ? (this == m)
                                                            : throw new ArgumentException($"'{nameof(message)}' is not an '{typeof(MessageHandler)}' object.");

        public override int GetHashCode()
        {
            return (int)Code;
        }

        public override string? ToString() => Message;
    }
}

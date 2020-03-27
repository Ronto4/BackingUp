using BackingUpConsole.Utilities.Messages;
using System;

namespace BackingUpConsole.Utilities.Commands
{
    public readonly struct CommandProperties
    {
        public readonly int minArgs;
        public readonly int maxArgs;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> Parse;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> Run;

        public CommandProperties(int minArgCount, int maxArgCount,
                                 Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> parseFunction,
                                 Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> runFunction)
                                    => (minArgs, maxArgs, Parse, Run) = (minArgCount, maxArgCount, parseFunction, runFunction);
    }
}
using BackingUpConsole.Utilities.Messages;
using System;
using System.Threading.Tasks;

namespace BackingUpConsole.Utilities.Commands
{
    public readonly struct CommandProperties
    {
        public readonly int minArgs;
        public readonly int maxArgs;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)>? Parse;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>>? Parse_Async;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>>? Run_Async;
        public readonly Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)>? Run;
        public readonly bool RunIsAsync;
        public readonly bool ParseIsAsync;

        public CommandProperties(int minArgCount, int maxArgCount,
                                 Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>> parseFunction,
                                 Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>> runFunction)
                                    => (minArgs, maxArgs, Parse, Parse_Async, Run_Async, Run, RunIsAsync, ParseIsAsync) = (minArgCount, maxArgCount, null, parseFunction, runFunction, null, true, true);
        public CommandProperties(int minArgCount, int maxArgCount,
                                 Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> parseFunction,
                                 Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> runFunction)
                                    => (minArgs, maxArgs, Parse, Parse_Async, Run_Async, Run, RunIsAsync, ParseIsAsync) = (minArgCount, maxArgCount, parseFunction, null, null, runFunction, false, false);
        public CommandProperties(int minArgCount, int maxArgCount,
                                         Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>> parseFunction,
                                         Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> runFunction)
                                            => (minArgs, maxArgs, Parse, Parse_Async, Run_Async, Run, RunIsAsync, ParseIsAsync) = (minArgCount, maxArgCount, null, parseFunction, null, runFunction, false, true);
        public CommandProperties(int minArgCount, int maxArgCount,
                                 Func<string[], UInt16, Paths, MessagePrinter, (MessageHandler, string?)> parseFunction,
                                 Func<string[], UInt16, Paths, MessagePrinter, Task<(MessageHandler, string?)>> runFunction)
                                    => (minArgs, maxArgs, Parse, Parse_Async, Run_Async, Run, RunIsAsync, ParseIsAsync) = (minArgCount, maxArgCount, parseFunction, null, runFunction, null, true, false);
    }
}
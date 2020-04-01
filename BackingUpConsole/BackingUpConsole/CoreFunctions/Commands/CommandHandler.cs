using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    public static class CommandHandler
    {
        private static readonly Dictionary<string, Func<string[], UInt16, Paths, MessagePrinter, MessageHandler>> Parse_Funcs = new Dictionary<string, Func<string[], ushort, Paths, MessagePrinter, MessageHandler>>()
        {
            {"list", Commands.List.Parse },
            {"add", Commands.Add.Parse }
        };
        private static readonly Dictionary<string, Func<string[], UInt16, Paths, MessagePrinter, Task<MessageHandler>>> Run_Funcs = new Dictionary<string, Func<string[], ushort, Paths, MessagePrinter, Task<MessageHandler>>>()
        {
            {"list", Commands.List.RunAsync },
            {"add", Commands.Add.RunAsync }
        };

        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            //if (!Array.Exists(modes, (arg) => arg == args[0]))
            //    return MessageProvider.BackingUpUnknownMode(args[0], flags.IsSet(Flags.VERBOSE));
            bool exists = Parse_Funcs.TryGetValue(args[0], out var parse_func);
            if(!exists)
                return MessageProvider.BackingUpUnknownMode(args[0], flags.IsSet(Flags.VERBOSE)); ;

            return parse_func!(args, flags, paths, messagePrinter);

        }

        public async static Task<MessageHandler> Run(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return await Run_Funcs[args[0]](args, flags, paths, messagePrinter);
        }

        
    }
}

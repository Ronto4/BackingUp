﻿using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BackingUpConsole.CoreFunctions
{
    public static class CommandHandler
    {
        private static readonly Dictionary<string, Func<string[], UInt16, Paths, MessagePrinter, MessageHandler>> Parse_Funcs = new Dictionary<string, Func<string[], ushort, Paths, MessagePrinter, MessageHandler>>()
        {
            {"list", Commands.List.Parse }
        };
        private static readonly Dictionary<string, Func<string[], UInt16, Paths, MessagePrinter, MessageHandler>> Run_Funcs = new Dictionary<string, Func<string[], ushort, Paths, MessagePrinter, MessageHandler>>()
        {
            {"list", Commands.List.Run }
        };

        public static MessageHandler Parse(string[] args, UInt16 flags, in Paths paths, MessagePrinter messagePrinter)
        {
            //if (!Array.Exists(modes, (arg) => arg == args[0]))
            //    return MessageProvider.BackingUpUnknownMode(args[0], flags.IsSet(Flags.VERBOSE));
            bool exists = Parse_Funcs.TryGetValue(args[0], out var parse_func);
            if(!exists)
                return MessageProvider.BackingUpUnknownMode(args[0], flags.IsSet(Flags.VERBOSE)); ;

            return parse_func!(args, flags, paths, messagePrinter);

        }

        public static MessageHandler Run(string[] args, UInt16 flags, in Paths paths, MessagePrinter messagePrinter)
        {
            return Run_Funcs[args[0]](args, flags, paths, messagePrinter);
        }

        
    }
}

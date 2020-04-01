using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    class Add
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!File.Exists(args[1]))
                return MessageProvider.FileNotFound(args[1], flags.IsSet(Flags.VERBOSE));

            (MessageHandler message, _) = Utilities.ScanList();
            if (!message.IsSuccess(true, messagePrinter))
                return message;
        }

        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            int z;
        }
    }
}

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
            if (!args.CheckLength(3, 3))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            if (!File.Exists(args[1]))
                return MessageProvider.FileNotFound(args[1], !flags.IsSet(Flags.VERBOSE));

            if (new FileInfo(args[1]).Extension != ".bu")
                return MessageProvider.InvalidExtension(args[1], "bu", silent: flags.IsSet(Flags.VERBOSE));

            (MessageHandler message, _) = Utilities.ScanList();
            if (!message.IsSuccess(true, messagePrinter))
                return message;

            return MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE));
        }

        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            (_, Dictionary<string, string>? entries) = await Utilities.ScanListAsync();
            if (entries is null)
                return MessageProvider.InvalidMethodExecution(flags, args, $"'BackingUpConsole.CoreFunctions.Utilities.ScanListAsync' returned null Dictionary in mode 'run'", silent: !flags.IsSet(Flags.VERBOSE));

            bool success = entries.TryAdd(args[2], args[1]);

            if (!success)
                return MessageProvider.DoubledName(args[2], !flags.IsSet(Flags.VERBOSE));

            MessageHandler message = await Utilities.WriteListFileAsync(entries);
            if (!message.IsSuccess(false, messagePrinter))
                return message;

            return MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE));
        }
    }
}

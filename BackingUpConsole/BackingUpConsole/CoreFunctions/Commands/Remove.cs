using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class Remove
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!args.CheckLength(2, 2))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            (MessageHandler success, Dictionary<string, string>? result) = Utilities.ScanList();
            if (!success.IsSuccess(true, messagePrinter))
                return success;

            if (!result!.ContainsKey(args[1]))
                return MessageProvider.BackupNotFound(args[1], silent: !flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success(!flags.IsSet(Flags.VERBOSE));
        }

        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            (MessageHandler error, Dictionary<string, string>? entries) = await Utilities.ScanListAsync();
            if (entries is null)
                return MessageProvider.InvalidMethodExecution(flags, args, $"'BackingUpConsole.CoreFunctions.Utilities.ScanListAsync' returned null Dictionary in mode 'run' {Environment.NewLine} Error message: '{error.Message}'", silent: !flags.IsSet(Flags.VERBOSE));

            entries.Remove(args[1]);

            MessageHandler message = await Utilities.WriteListFileAsync(entries);
            if (!message.IsSuccess(false, messagePrinter))
                return message;

            return MessageProvider.BackupEntryRemoved(args[1], silent: !flags.IsSet(Flags.VERBOSE));
        }
    }
}

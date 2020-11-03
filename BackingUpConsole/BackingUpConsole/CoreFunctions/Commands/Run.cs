using BackingUpConsole.CoreFunctions.BackingUp;
using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class Run
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (args.CheckLength(1, 1) == false)
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            if (paths.SelectedBackup is null)
                return MessageProvider.NoBackupSelected(!flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success();
        }
        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            return await paths.SelectedBackup!.PerformBackup(messagePrinter);
        }
    }
}

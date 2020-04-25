using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class Select
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!args.CheckLength(1, 2))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            (MessageHandler success, Dictionary<string, string>? result) = Utilities.ScanList();
            if (!success.IsSuccess(true, messagePrinter))
                return success;

            if (args.CheckLength(2, 2) && !result!.ContainsKey(args[1]))
                return MessageProvider.BackupNotFound(args[1], silent: !flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE));
        }
        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (args.CheckLength(1, 1))
                return MessageProvider.Message($"Currently selected Back Up file: '{(paths.SelectedBackup is null ? "<NULL>" : paths.SelectedBackup.Path)}'.", silent: !flags.IsSet(Flags.VERBOSE));

            (_, Dictionary<string, string>? entries) = await Utilities.ScanListAsync();
            if (entries is null)
                return MessageProvider.InvalidMethodExecution(flags, args, $"'BackingUpConsole.CoreFunctions.Utilities.ScanListAsync' returned null Dictionary in mode 'run'", silent: !flags.IsSet(Flags.VERBOSE));

            bool success = entries.TryGetValue(args[1], out string? path);
            if (!success)
                return MessageProvider.BackupNotFound(args[1], silent: !flags.IsSet(Flags.VERBOSE));

            (MessageHandler message, BackUpFile? backup) = await BackUpFile.GetFromFile(path!);
            if (!message.IsSuccess(false, messagePrinter))
                return message;

            paths.SelectedBackup = backup;
            return MessageProvider.BackupChanged(args[1], paths.SelectedBackup is null ? "<NULL>" : paths.SelectedBackup.Path, silent: !flags.IsSet(Flags.VERBOSE));
        }
    }
}

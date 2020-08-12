using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class Settings
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!args.CheckLength(1, 1) && !args.CheckLength(4, 4))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            if (paths.SelectedBackup is null)
                return MessageProvider.NoBackupSelected(!flags.IsSet(Flags.VERBOSE));

            if (args.CheckLength(1, 1))
                return MessageProvider.Success();

            if (!paths.SelectedBackup.Settings.ParameterExists(args[1]))
                return MessageProvider.ParameterDoesNotExist(args[1]);

            if (BackUpSettings.EditTypes.ContainsKey(args[2]) == false)
                return MessageProvider.UnknownSettingsUsage(args[2], !flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success();
        }
        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (args.CheckLength(1, 1))
                return MessageProvider.Message($"Currently selected settings:{Environment.NewLine}{paths.SelectedBackup!.Settings}");

            var run = await paths.SelectedBackup!.Settings.UpdateSettings(args[3], args[1], args[2], messagePrinter);
            if (!run.IsSuccess(messagePrinter))
                return run;

            return MessageProvider.SettingsUpdated(!flags.IsSet(Flags.VERBOSE));
        }
    }
}

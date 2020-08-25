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
            if (!args.CheckLength(1, 4))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            if (paths.SelectedBackup is null)
                return MessageProvider.NoBackupSelected(!flags.IsSet(Flags.VERBOSE));

            if (args.CheckLength(1, 1))
                return MessageProvider.Success();

            if (!paths.SelectedBackup.Settings.ParameterExists(args[1]))
            {
                if (args[1] == "path")
                {
                    if (args.CheckLength(2, 2))
                        return MessageProvider.Success();
                    return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));
                }
                if (args[1] == "select")
                {
                    if (args.CheckLength(3, 3))
                        return MessageProvider.Success();
                    return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));
                }
                if (args[1] == "create")
                {
                    if (args.CheckLength(3, 4))     // TODO: Check if args[3] (from-name) is an existing settings file
                        return MessageProvider.Success();
                    return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));
                }
                if (args[1] == "remove")
                {
                    if (args.CheckLength(2, 3))     // TODO: Check if args[2] (name) is an existing settings file and does not match the currently selected settings
                        return MessageProvider.Success();
                    return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));
                }
                return MessageProvider.ParameterDoesNotExist(args[1]);
            }
            if (args.CheckLength(2, 2))
                return MessageProvider.Success();

            if (args.CheckLength(3, 3))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            if (BackUpSettings.EditTypes.ContainsKey(args[2]) == false)
                return MessageProvider.UnknownSettingsUsage(args[2], !flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success();
        }
        public static async Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (args.CheckLength(1, 1))
                return MessageProvider.Message($"Currently selected settings:{Environment.NewLine}{paths.SelectedBackup!.Settings}");

            if (args.CheckLength(2, 2))
            {
                if (args[1] == "path")
                    return MessageProvider.Message($"Path of currently selected settings: {paths.SelectedBackup!.Settings.Path}");

                return MessageProvider.Message($"Property '{args[1]}' of currently selected settings: {paths.SelectedBackup!.Settings.Settings.PropertyToString(args[1])}");
            }

            var run = await paths.SelectedBackup!.Settings.UpdateSettings(args[3], args[1], args[2], messagePrinter);
            if (!run.IsSuccess(messagePrinter))
                return run;

            return MessageProvider.SettingsUpdated(!flags.IsSet(Flags.VERBOSE));
        }
    }
}

﻿using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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

            if (!paths.SelectedBackup.Settings!.ParameterExists(args[1]))
            {
                if (args[1] == "path")
                {
                    if (args.CheckLength(2, 2))
                        return MessageProvider.Success();
                    return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));
                }
                if (args[1] == "list")
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
                return MessageProvider.Message($"Currently selected settings:{Environment.NewLine}{paths.SelectedBackup!.Settings}", color: ConsoleColor.White, silent: !flags.IsSet(Flags.VERBOSE));

            if (args.CheckLength(2, 2))
            {
                switch (args[1])
                {
                    case "path":
                        return MessageProvider.Message($"Path of currently selected settings: {paths.SelectedBackup!.Settings!.Path}");

                    case "list":
                        {
                            //Stopwatch sw = new Stopwatch();
                            //sw.Start();
                            (ConcurrentQueue<BackUpSettings>? settings, MessageHandler message) = await paths.SelectedBackup!.GetAllSettingsParallel(messagePrinter);
                            //(List<BackUpSettings>? settings, MessageHandler message) = await paths.SelectedBackup!.GetAllSettings(messagePrinter);
                            //sw.Stop();
                            if (message.IsSuccess(messagePrinter) == false)
                                return message;


                            MessageHandler r = MessageProvider.Message($"All available settings files: {settings.Select(Settings => $"\"{Settings.Settings.SettingsName}\": '{Settings.Path.Split('\\')[^1].Split('.')[0..^1].CustomToString("")}'").ToList().CustomToString()}");
                            //Console.WriteLine($"It took {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds} ms). Found {settings!.Count} elements.");
                            //return MessageProvider.Success();
                            return r;
                        }
                    default:
                        return MessageProvider.Message($"Property '{args[1]}' of currently selected settings: {paths.SelectedBackup!.Settings!.Settings.PropertyToString(args[1])}");
                }
            }
            if (args.CheckLength(3, 3))
            {
                switch (args[1])
                {
                    case "create":
                        /*MessageHandler createNew =*/return await paths.SelectedBackup!.CreateNewSettings(args[2], messagePrinter);
                    case "select":
                        MessageHandler result = await paths.SelectedBackup!.SetSettingsFromFile(args[2], messagePrinter);
                        if (result.IsSuccess(messagePrinter) == false)
                            return result;

                        return MessageProvider.BackUpSettingsSelected(args[2]);
                    case "remove":
                        string buseName = $"{args[2]}.buse";
                        string path = PathHandler.Combine(paths.SelectedBackup!.FileContainer.SettingsDir, buseName);
                        if (buseName == paths.SelectedBackup!.FileContainer.SelectedBackupSettings)
                            return MessageProvider.TriedRemovingActiveFile(path);

                        MessageHandler question = MessageProvider.Message($"This action will DELETE the FILE at '{path}'.", MessageCollections.Levels.Warning);
                        if (question.IsSuccess(messagePrinter) == false)
                            return question;

                        File.Delete(path);
                        return MessageProvider.FileRemoved(path);
                    default:
                        break;
                }
            }

            if (args[1] == "create")
                return await paths.SelectedBackup!.CreateNewSettings(args[2], messagePrinter, args[3]);

            var run = await paths.SelectedBackup!.Settings!.UpdateSettings(args[3], args[1], args[2], messagePrinter);
            if (!run.IsSuccess(messagePrinter))
                return run;

            return MessageProvider.SettingsUpdated(!flags.IsSet(Flags.VERBOSE));
        }
    }
}
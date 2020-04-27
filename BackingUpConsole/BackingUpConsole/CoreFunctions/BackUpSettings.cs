﻿using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    //Note: This class is just a placeholder for now, will be implemented later.
    public class BackUpSettings
    {
        //Types
        private struct Settings
        {
            internal string[] BackUpPaths;
        }
        //Enums
        public enum EditType
        {
            remove,
            add
        }
        //Attributes
        private static readonly string FileIdentifier = $"[BackUpSettings]";
        private static readonly int RequiredVersion = 1;
        private string[] Parameters = new string[] { "paths" };
        public string Path { get; }
        private Settings settings = new Settings { BackUpPaths = new string[0] };
        //Constructors
        public BackUpSettings(string path)
        {
            Path = path;
        }
        //Methods
        public bool ParameterExists(string name) => Parameters.Contains(name);
        public async Task<MessageHandler> EditSettings(string entry, EditType editType, MessagePrinter messagePrinter)
        {
            var get = await GetSettings();
            if (!get.IsSuccess(messagePrinter))
                return get;

            string[] newPaths = new string[0];

            switch (editType)
            {
                case EditType.add:
                    newPaths = new string[settings.BackUpPaths.Length + 1];
                    newPaths[0] = entry;
                    for (int i = 0; i < settings.BackUpPaths.Length; i++)
                    {
                        newPaths[i + 1] = settings.BackUpPaths[i];
                    }
                    break;
                case EditType.remove:
                    newPaths = new string[settings.BackUpPaths.Length - 1];
                    for (int i = 0, j = 0; i < settings.BackUpPaths.Length; i++)
                    {
                        //Console.WriteLine($"Iteration: i == {i}; j == {j}; newPaths.Length == {newPaths.Length}; settings.BackUpPaths.Length == {settings.BackUpPaths.Length}");
                        if (settings.BackUpPaths[i] == entry)
                        {
                            //Console.WriteLine($"Continue: '{settings.BackUpPaths[i]}' with entry == '{entry}'.");
                            continue;
                        }

                        newPaths[j] = settings.BackUpPaths[i];
                        j++;
                    }
                    break;
            }
            newPaths = newPaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            settings.BackUpPaths = newPaths;
            var set = await SaveSettings();
            if (!set.IsSuccess(messagePrinter))
                return set;

            return MessageProvider.Success();
        }
        public static async Task<(BackUpSettings?, MessageHandler)> GetFromFile(string path, MessagePrinter messagePrinter)
        {
            var buse = new BackUpSettings(path);
            var get = await buse.GetSettings();
            if (!get.IsSuccess(messagePrinter))
                return (null, get);

            return (buse, MessageProvider.Success());
        }
        public override string ToString()
        {
            string r = String.Empty;
            r += $"Path: {Path}{Environment.NewLine}";
            r += $"BackUpPaths:{Environment.NewLine}";
            for (int i = 0; i < settings.BackUpPaths.Length; i++)
            {
                r += $" - {settings.BackUpPaths[i]}{Environment.NewLine}";
            }
            return r;
        }
        public async Task<MessageHandler> Create(MessagePrinter messagePrinter)
        {
            settings = new Settings { BackUpPaths = new string[] { } };
            var save = await SaveSettings();
            if (!save.IsSuccess(messagePrinter))
                return save;

            var get = await GetSettings();
            if (!get.IsSuccess(messagePrinter))
                return get;

            return MessageProvider.Success();
        }
        private async IAsyncEnumerable<string?> ReadFromFile()
        {
            using StreamReader sr = new StreamReader(Path);
            while (!sr.EndOfStream)
            {
                string? line = await sr.ReadLineAsync();

                yield return line;
            }
            sr.Close();
        }
        public async Task<MessageHandler> GetSettings()
        {
            int state = 0;
            await foreach (string? line in ReadFromFile())
            {
                if (line is null)
                    return MessageProvider.InvalidMethodExecution(null, null, "ReadLine returned null, when EOF was not detected.");

                if (line.StartsWith(';'))
                    continue;

                switch (state)
                {
                    case 0:
                        {
                            if (line != FileIdentifier)
                                return MessageProvider.InvalidFileFormat(Path, 1);

                            state++;
                            break;
                        }
                    case 1:
                        {
                            if (!line.StartsWith('*'))
                                return MessageProvider.InvalidFileFormat(Path, 2);

                            string[] parameters = line.Substring(1).Split('|', StringSplitOptions.RemoveEmptyEntries);
                            int version = -1;
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                string[] param = parameters[i].Split(':');
                                if (param.Length != 2)
                                    return MessageProvider.InvalidFileFormat(Path, 2);

                                switch (param[0])
                                {
                                    case "version":
                                        version = Convert.ToInt32(param[1]);
                                        break;
                                    default:
                                        return MessageProvider.InvalidFileFormat(Path, 2);
                                }
                            }
                            if (version != RequiredVersion)
                                return MessageProvider.InvalidFileVersion(Path, RequiredVersion, version);

                            state++;
                            break;
                        }
                    case 2:
                        {
                            string[] setting = line.Split('?');
                            if (setting.Length != 2)
                                return MessageProvider.InvalidFileFormat(Path, line);

                            switch (setting[0])
                            {
                                case "paths":
                                    string[] paths = setting[1].Split('|');
                                    settings.BackUpPaths = paths;
                                    break;
                                default:
                                    return MessageProvider.InvalidFileFormat(Path, line);
                            }
                            break;
                        }
                }
            }
            return MessageProvider.Success();
        }
        public async Task<MessageHandler> SaveSettings()
        {
            if (!File.Exists(Path))
            {
                if (!Directory.Exists((new FileInfo(Path).Directory).FullName))
                    Directory.CreateDirectory((new FileInfo(Path).Directory).FullName);

                var f = File.Create(Path);
                f.Close();
            }

            using (StreamWriter sw = new StreamWriter(Path))
            {
                await sw.WriteAsync(ConstantValues.DEFAULT_BACKUP_SETTINGS_FILE);
                for (int i = 0; i < settings.BackUpPaths.Length; i++)
                {
                    await sw.WriteAsync(settings.BackUpPaths[i] + "|");
                }
                sw.Close();
            }
            return MessageProvider.Success();
        }
    }
}

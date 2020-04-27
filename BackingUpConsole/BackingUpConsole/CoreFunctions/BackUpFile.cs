using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    public class BackUpFile : ICloneable
    {
        private static readonly string FileIdentifier = $"[BackUpContainer]";
        //Attributes
        public string Path { get; }
        public BackUpSettings Settings { get; private set; }
        public DirectoryInfo SummaryDirectory { get; private set; }
        public DirectoryInfo LogDirectory { get; private set; }
        public DirectoryInfo SettingsPath { get; private set; }
        public DirectoryInfo BackUpPath { get; private set; }
        private int Version { get; }
        //Constructors
        private BackUpFile(string path, BackUpSettings settings, DirectoryInfo summaryDir, DirectoryInfo logDir, DirectoryInfo settingsDir, DirectoryInfo backupDir, int version, bool create)
        {
            Path = path;
            Settings = settings;
            SettingsPath = settingsDir;
            SummaryDirectory = summaryDir;
            LogDirectory = logDir;
            BackUpPath = backupDir;
            Version = version;

        }
        //static methods
        public static async Task<(MessageHandler, BackUpFile?)> GetFromFile(string path, MessagePrinter messagePrinter, bool firstCreation = false)
        {
            List<string?> results = new List<string?>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    results.Add(sr.ReadLine());
                    if (results[^1] is null)
                    {
                        var message = MessageProvider.InvalidMethodExecution(null, null, $"StreamReader.ReadLine returned null string when EOF was not detected in file '{path}' on line {results.Count}.");
                        return (message, null);
                    }
                    if (results[^1]!.StartsWith(';'))
                        results.RemoveAt(results.Count - 1);
                }
            }
            if (results.Count != 7)
                return (MessageProvider.InvalidFileFormat(path, 0), null);

            if (results[0] != FileIdentifier)
            {
                var message = MessageProvider.InvalidFileFormat(path, 1);
                return (message, null);
            }
            if (!results[1]!.StartsWith('*'))
            {
                var message = MessageProvider.InvalidFileFormat(path, 2);
                return (message, null);
            }
            string[] parameter = results[1]!.Substring(1).Split('|');
            if (parameter.Length != 1)
            {
                var message = MessageProvider.InvalidFileFormat(path, 2);
                return (message, null);
            }
            int version = -1;
            for (int i = 0; i < parameter.Length; i++)
            {
                string[] param = parameter[i].Split(':');
                if (param.Length != 2)
                {
                    var message = MessageProvider.InvalidFileFormat(path, 2);
                    return (message, null);
                }
                if (param[0] == "version")
                    version = Convert.ToInt32(param[1]);
                else
                {
                    var message = MessageProvider.InvalidFileFormat(path, 2);
                    return (message, null);
                }
            }
            if (version == -1)
            {
                var message = MessageProvider.InvalidFileFormat(path, 2);
                return (message, null);
            }
            DirectoryInfo? settingsPath, summaryDir, logDir, backupDir;
            settingsPath = summaryDir = logDir = backupDir = null;
            BackUpSettings? settings = null;
            for (int i = 2; i < results.Count; i++)
            {
                string[] value = results[i]!.Split('?');
                if (value.Length != 2)
                {
                    var message = MessageProvider.InvalidFileFormat(path, i + 1);
                    return (message, null);
                }
                switch (value[0])
                {
                    case "settings":
                        {
                            settingsPath = new DirectoryInfo(value[1]);
                            break;
                        }
                    case "selectedsettings":
                        {
                            //settings = new BackUpSettings(value[1]);
                            MessageHandler getFromFile;
                            if (firstCreation)
                            {
                                var lsettings = new BackUpSettings(value[1]);
                                await lsettings.Create(messagePrinter);
                                (settings, getFromFile) = await BackUpSettings.GetFromFile(value[1], messagePrinter);
                            }
                            else
                            {
                                (settings, getFromFile) = await BackUpSettings.GetFromFile(value[1], messagePrinter);
                            }
                            if (!getFromFile.IsSuccess(false, messagePrinter))
                                return (getFromFile, null);

                            break;
                        }
                    case "summaries":
                        {
                            summaryDir = new DirectoryInfo(value[1]);
                            break;
                        }
                    case "logs":
                        {
                            logDir= new DirectoryInfo(value[1]);
                            break;
                        }
                    case "backups":
                        {
                            backupDir = new DirectoryInfo(value[1]);
                            break;
                        }
                    default:
                        {
                            var message = MessageProvider.InvalidFileFormat(path, i + 1);
                            return (message, null);
                        }
                }
            }
            if (settings is null || settingsPath is null || summaryDir is null || logDir is null || backupDir is null)
            {
                var message = MessageProvider.InvalidFileFormat(path, 0);
                return (message, null);
            }
            var r = (MessageProvider.Success(), new BackUpFile(path, settings, summaryDir, logDir, settingsPath, backupDir, version, firstCreation));
            if (firstCreation)
                await r.Item2.Create(messagePrinter);

            return r;
        }
        //Methods
        public async Task Create(MessagePrinter messagePrinter)
        {
            SummaryDirectory.Create();
            LogDirectory.Create();
            SettingsPath.Create();
            if(!File.Exists(Settings.Path))
                _ = await Settings.Create(messagePrinter);

            BackUpPath.Create();
        }
        //Override methods
        public object Clone()
        {
            return new BackUpFile(this.Path, this.Settings, this.SummaryDirectory, this.LogDirectory, this.SettingsPath, this.BackUpPath, this.Version, false);
        }
    }
}

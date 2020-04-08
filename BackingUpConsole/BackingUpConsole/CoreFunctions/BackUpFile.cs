using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BackingUpConsole.CoreFunctions
{
    class BackUpFile
    {
        private static readonly string FileIdentifier = $"[BackUp]";
        //Attributes
        public BackUpSettings Settings { get; private set; }
        public DirectoryInfo SummaryDirectory { get; private set; }
        public DirectoryInfo LogDirectory { get; private set; }
        public DirectoryInfo SettingsPath { get; private set; }
        private int Version { get; }
        //Constructors
        private BackUpFile(string path, out MessageHandler message)
        {
            List<string?> results = new List<string?>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    results.Add(sr.ReadLine());
                    if (results[^1] is null)
                    {
                        message = MessageProvider.InvalidMethodExecution(null, null, $"StreamReader.ReadLine returned null string when EOF was not detected in file '{path}' on line {results.Count}.");
                        return;
                    }
                    if (results[^1]!.StartsWith(';'))
                        results.RemoveAt(results.Count - 1);
                }
            }
            if (results[0] != FileIdentifier)
            {
                message = MessageProvider.InvalidFileFormat(path, 1);
                return;
            }
            if (!results[1]!.StartsWith('*'))
            {
                message = MessageProvider.InvalidFileFormat(path, 2);
                return;
            }
            string[] parameter = results[1]!.Substring(1).Split('|');
            if (parameter.Length != 1)
            {
                message = MessageProvider.InvalidFileFormat(path, 2);
                return;
            }
            for (int i = 0; i < parameter.Length; i++)
            {
                string[] param = parameter[i].Split(':');
                if (param.Length != 2)
                {
                    message = MessageProvider.InvalidFileFormat(path, 2);
                    return;
                }
                if (param[0] == "version")
                    Version = Convert.ToInt32(param[1]);
                else
                {
                    message = MessageProvider.InvalidFileFormat(path, 2);
                    return;
                }
            }
            for (int i = 2; i < results.Count; i++)
            {
                string[] value = results[i]!.Split('?');
                if (value.Length != 2)
                {
                    message = MessageProvider.InvalidFileFormat(path, 2);
                    return;
                }
                switch (value[0])
                {
                    case "settings":
                        {
                            SettingsPath = new DirectoryInfo(value[1]);
                            break;
                        }
                    case "selectedsettings":
                        {
                            Settings = new BackUpSettings(value[1]);
                            break;
                        }
                    case "summaries":
                        {
                            SummaryDirectory = new DirectoryInfo(value[1]);
                            break;
                        }
                    case "logs":
                        {
                            LogDirectory = new DirectoryInfo(path);
                            break;
                        }
                    default:
                        {
                            message = MessageProvider.InvalidFileFormat(path, i + 1);
                            return;
                        }
                }
            }
            if (Settings is null || SettingsPath is null || SummaryDirectory is null || LogDirectory is null)
            {
                message = MessageProvider.InvalidFileFormat(path, 0);
                return;
            }
            message = MessageProvider.Success();
            return;
        }
        //Static methods
        public static (MessageHandler, BackUpFile?) GetFromFile(string path, UInt16 flags, MessagePrinter messagePrinter)
        {
            BackUpFile backUp = new BackUpFile(path, out MessageHandler message);
            if (!message.IsSuccess(false, messagePrinter))
                return (message, null);

            return (MessageProvider.Success(silent: !flags.IsSet(Flags.VERBOSE)), backUp);
        }
    }
}

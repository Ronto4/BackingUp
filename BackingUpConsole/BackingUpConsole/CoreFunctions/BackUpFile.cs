using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    public class BackUpFile : ICloneable
    {
        // Types
        internal class BackUpFileContainerHelper
        {
            public string FileType { get; set; } = string.Empty;
            public int FileVersion { get; set; } = 0;
            public string SelectedBackupSettings { get; set; } = string.Empty;
            public string SettingsDir { get; set; } = string.Empty;
            public string SummaryDir { get; set; } = string.Empty;
            public string LogDir { get; set; } = string.Empty;
            public string DataDir { get; set; } = string.Empty;
        }
        public class BackUpFileContainer
        {
            // Properties
            public string FileType { get => Fields[nameof(FileType)].Value; set => Fields[nameof(FileType)].Value = value; }
            public int FileVersion { get => Fields[nameof(FileVersion)].Value; set => Fields[nameof(FileVersion)].Value = value; }
            public string SelectedBackupSettings { get => Fields[nameof(SelectedBackupSettings)].Value; set => Fields[nameof(SelectedBackupSettings)].Value = value; }
            public string SettingsDir { get => Fields[nameof(SettingsDir)].Value; set => Fields[nameof(SettingsDir)].Value = value; }
            public string SummaryDir { get => Fields[nameof(SummaryDir)].Value; set => Fields[nameof(SummaryDir)].Value = value; }
            public string LogDir { get => Fields[nameof(LogDir)].Value; set => Fields[nameof(LogDir)].Value = value; }
            public string DataDir { get => Fields[nameof(DataDir)].Value; set => Fields[nameof(DataDir)].Value = value; }
            internal Dictionary<string, DynamicJsonProperty> Fields = new Dictionary<string, DynamicJsonProperty>()
            {
                {nameof(FileType), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(FileVersion), new DynamicJsonProperty(DynamicJsonProperty.UsedType.Integer, 0) },
                {nameof(SelectedBackupSettings), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(SettingsDir), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(SummaryDir), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(LogDir), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(DataDir), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) }
            };
            internal static int RequiredFileVersion => 2;
            internal static string RequiredFileType => "BackUpFile";
            // Constructors
            internal BackUpFileContainer(BackUpFileContainerHelper helper)
            {
                FileType = helper.FileType;
                FileVersion = helper.FileVersion;
                SelectedBackupSettings = helper.SelectedBackupSettings;
                SettingsDir = helper.SettingsDir;
                SummaryDir = helper.SummaryDir;
                LogDir = helper.LogDir;
                DataDir = helper.DataDir;
            }
            public BackUpFileContainer() { }
            // Methods
            internal MessageHandler Validate(string path)
            {
                if (RequiredFileVersion != FileVersion)
                    return MessageProvider.InvalidFileVersion(path, RequiredFileVersion, FileVersion);
                if (RequiredFileType != FileType)
                    return MessageProvider.InvalidFileType(path, RequiredFileType, FileType);
                return MessageProvider.Success();
            }
        }
        // Properties
        public BackUpFileContainer FileContainer { get; private set; }
        public string Path { get; }
        public BackUpSettings? Settings { get; private set; }
        // Constructors
        private BackUpFile(string path)
        {
            FileContainer = new BackUpFileContainer();
            Path = path;
        }
        private BackUpFile(string path, BackUpFileContainer fileContainer, BackUpSettings? settings)
        {
            FileContainer = fileContainer;
            Path = path;
            Settings = settings;
        }
        // Methods
        public async Task<(List<BackUpSettings>?, MessageHandler)> GetAllSettings(MessagePrinter messagePrinter) // Currently unused, may be returned to function if a method is implemented to select the best GetAllSettings method.
        {
            List<BackUpSettings> list = new List<BackUpSettings>();
            DirectoryInfo di = new DirectoryInfo(FileContainer.SettingsDir);
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                (BackUpSettings? settings, MessageHandler message) = await BackUpSettings.GetFromFile(fi.FullName, messagePrinter);
                if (message.IsSuccess(messagePrinter) == false)
                    return (null, message);

                list.Add(settings!);
            }
            return (list, MessageProvider.Success());
        }
        public async Task<(ConcurrentQueue<BackUpSettings>?, MessageHandler)> GetAllSettingsParallel(MessagePrinter messagePrinter)
        {
            ConcurrentQueue<BackUpSettings> settings = new ConcurrentQueue<BackUpSettings>();
            DirectoryInfo di = new DirectoryInfo(FileContainer.SettingsDir);
            MessageHandler? exitMessage = null;
            var tasks = di.EnumerateFiles().Select(async fi =>
            {
                (BackUpSettings? setting, MessageHandler message) = await BackUpSettings.GetFromFile(fi.FullName, messagePrinter);
                if (message.IsSuccess(messagePrinter) == false)
                    exitMessage = message;
                else
                    settings.Enqueue(setting!);
            });
            await Task.WhenAll(tasks);
            if ((exitMessage is null) == false)
            {
                return (null, exitMessage);
            }
            return (settings, MessageProvider.Success());
        }
        internal async Task<MessageHandler> SetSettingsFromFile(string buseName, MessagePrinter messagePrinter)
        {
            string currentSettings = FileContainer.SelectedBackupSettings;
            FileContainer.SelectedBackupSettings = $"{buseName}.buse";
            MessageHandler result = await GetSettings(messagePrinter);
            if (result.IsSuccess(messagePrinter) == false)
            {
                FileContainer.SelectedBackupSettings = buseName;
                result = await GetSettings(messagePrinter);
            }
            if (result.IsSuccess(messagePrinter) == false)
            {
                FileContainer.SelectedBackupSettings = currentSettings;
                return result;
            }
            MessageHandler message = await SaveFile();
            if (message.IsSuccess(messagePrinter) == false)
                return message;
            return result;
        }
        private async Task<MessageHandler> GetSettings(MessagePrinter messagePrinter)
        {
            (BackUpSettings? settings, MessageHandler result) = await BackUpSettings.GetFromFile(PathHandler.Combine(FileContainer.SettingsDir, FileContainer.SelectedBackupSettings), messagePrinter);
            if (result.IsSuccess(messagePrinter) == false)
                return result;

            Settings = settings!;
            return MessageProvider.Success();
        }
        private async Task<MessageHandler> SaveFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            using (FileStream fs = File.Create(Path))
            {
                await JsonSerializer.SerializeAsync(fs, FileContainer, options);
            }
            return MessageProvider.Success();
        }
        private async Task<MessageHandler> GetFile(MessagePrinter messagePrinter, bool skipValidation = false)
        {
            if (File.Exists(Path) == false)
            {
                FileInfo file = new FileInfo(Path);
                if (file.Directory.Exists == false)
                    file.Directory.Create();
                using FileStream fs = file.Create();
                await fs.WriteAsync(new byte[] { (byte)'{', (byte)'}' });
            }
            using (FileStream fs = File.OpenRead(Path))
            {
                try
                {
                    var helper = await JsonSerializer.DeserializeAsync<BackUpFileContainerHelper>(fs);
                    BackUpFileContainer oldBackUpFile = FileContainer;
                    FileContainer = new BackUpFileContainer(helper);
                    if (skipValidation == false)
                    {
                        var validate = FileContainer.Validate(Path);
                        if (validate.IsSuccess(messagePrinter) == false)
                            return validate;
                    }
                }
                catch (JsonException ex)
                {
                    return MessageProvider.InvalidJsonFileFormat(Path, ex.Message);
                }
            }
            return MessageProvider.Success();
        }
        public static async Task<(BackUpFile?, MessageHandler)> GetFromFile(string path, MessagePrinter messagePrinter, bool firstCreation = false)
        {
            Console.WriteLine($"1 firstCreation: {firstCreation}");
            var file = new BackUpFile(path);
            var message = await file.GetFile(messagePrinter, firstCreation);
            if (message.IsSuccess(messagePrinter) == false)
                return (null, message);

            if (firstCreation)
            {
                string basePath = string.Join('\\', file.Path.Split('\\')[0..^1]);
                string settings = $"settings";
                string logs = $"logs";
                string summaries = $"summaries";
                string data = $"data";
                string defaultSettings = $"default.buse";
                Console.WriteLine($"2 SettingsPath: {PathHandler.Combine(basePath, settings)}");
                Directory.CreateDirectory(PathHandler.Combine(basePath, settings));
                Directory.CreateDirectory(PathHandler.Combine(basePath, logs));
                Directory.CreateDirectory(PathHandler.Combine(basePath, summaries));
                Directory.CreateDirectory(PathHandler.Combine(basePath, data));
                file.FileContainer.DataDir = PathHandler.Combine(basePath, data);
                file.FileContainer.SummaryDir = PathHandler.Combine(basePath, summaries);
                file.FileContainer.LogDir = PathHandler.Combine(basePath, logs);
                file.FileContainer.SettingsDir = PathHandler.Combine(basePath, settings);
                var createSettings = await BackUpSettings.GetFromFile(PathHandler.Combine(file.FileContainer.SettingsDir, defaultSettings), messagePrinter, true);
                if (createSettings.Item2.IsSuccess(messagePrinter) == false)
                    return (null, createSettings.Item2);

                file.FileContainer.SelectedBackupSettings = defaultSettings;
                file.FileContainer.FileVersion = 2;
                file.FileContainer.FileType = "BackUpFile";
                MessageHandler save = await file.SaveFile();
                if (save.IsSuccess(messagePrinter) == false)
                    return (null, save);

                var reget = await file.GetFile(messagePrinter);
                if (reget.IsSuccess(messagePrinter) == false)
                    return (null, reget);
            }
            var getSettings = await file.GetSettings(messagePrinter);
            if (getSettings.IsSuccess(messagePrinter) == false)
                return (null, getSettings);

            return (file, MessageProvider.Success());
        }
        // TODO: Improve Clone quality
        public object Clone() => new BackUpFile(Path, FileContainer, Settings);
    }
}

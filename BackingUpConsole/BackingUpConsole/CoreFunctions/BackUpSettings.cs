using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    public class BackUpSettings
    {
        //Types
        internal class SettingsContainerHelper
        {
            public string FileType { get; set; } = string.Empty;
            public int FileVersion { get; set; } = 0;
            public string SettingsName { get; set; } = string.Empty;
            public List<string> Paths { get; set; } = new List<string>();
            public List<string> BlacklistedExtensions { get; set; } = new List<string>();
        }
        public class SettingsContainer
        {
            // Properties
            public string FileType { get => Fields[nameof(FileType)].Value; set => Fields[nameof(FileType)].Value = value; }
            public int FileVersion { get => Fields[nameof(FileVersion)].Value; set => Fields[nameof(FileVersion)].Value = value; }
            public string SettingsName { get => Fields[nameof(SettingsName)].Value; set => Fields[nameof(SettingsName)].Value = value; }
            public List<string> Paths { get => ((IEnumerable<DynamicJsonProperty>)Fields[nameof(Paths)].Value).Select<DynamicJsonProperty, string>(prop => prop.Value).ToList();  set => Fields[nameof(Paths)].Value = value; }
            public List<string> BlacklistedExtensions { get => ((IEnumerable<DynamicJsonProperty>)Fields[nameof(BlacklistedExtensions)].Value).Select<DynamicJsonProperty, string>(prop => prop.Value).ToList(); set => Fields[nameof(BlacklistedExtensions)].Value = value; }
            internal Dictionary<string, DynamicJsonProperty> Fields = new Dictionary<string, DynamicJsonProperty>()
            {
                {nameof(Paths), new DynamicJsonProperty(DynamicJsonProperty.UsedType.Array, new string[0], DynamicJsonProperty.UsedType.String) },
                {nameof(BlacklistedExtensions), new DynamicJsonProperty(DynamicJsonProperty.UsedType.Array, new string[0], DynamicJsonProperty.UsedType.String) },
                {nameof(FileType), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(SettingsName), new DynamicJsonProperty(DynamicJsonProperty.UsedType.String, string.Empty) },
                {nameof(FileVersion), new DynamicJsonProperty(DynamicJsonProperty.UsedType.Integer, 0) }
            };
            internal static Dictionary<string, string> NameTranslate = new Dictionary<string, string>()
            {
                {"paths", nameof(Paths) },
                {"excluded-extensions", nameof(BlacklistedExtensions) },
                {"name", nameof(SettingsName) }
            };
            internal static int RequiredFileVersion => 1;
            internal static string RequiredFileType => "BackUpSettings";
            // Constructors
            internal SettingsContainer(SettingsContainerHelper helper)
            {
                Paths = helper.Paths;
                BlacklistedExtensions = helper.BlacklistedExtensions;
                FileType = helper.FileType;
                SettingsName = helper.SettingsName;
                FileVersion = helper.FileVersion;
            }
            public SettingsContainer() { } 
            // Methods
            internal MessageHandler Validate(string path)
            {
                if (RequiredFileVersion != FileVersion)
                    return MessageProvider.InvalidFileVersion(path, RequiredFileVersion, FileVersion);
                if (RequiredFileType != FileType)
                    return MessageProvider.InvalidFileType(path, RequiredFileType, FileType);
                return MessageProvider.Success();
            }
            public string PropertyToString(string name)
            {
                return name switch
                {
                    "paths" => $"{Environment.NewLine}\t{Paths.CustomToString($"{Environment.NewLine}\t")}",
                    "excluded-extensions" => BlacklistedExtensions.CustomToString(),
                    _ => throw new ArgumentException($"Unknown property '{name}'.")
                };
            }
        }
        public enum EditType
        {
            AddValueToField,
            SetFieldToValue,
            RemoveValueFromField
        }
        //Attributes
        public static readonly Dictionary<string, EditType> EditTypes = new Dictionary<string, EditType>()
        {
            {"add", EditType.AddValueToField },
            {"set", EditType.SetFieldToValue },
            {"remove", EditType.RemoveValueFromField } };
        public SettingsContainer Settings { get; private set; }
        public string Path { get; }
        //Constructors
        private BackUpSettings(string path)
        {
            Settings = new SettingsContainer();
            Path = path;
        }
        //Methods
        public bool ParameterExists(string name) => SettingsContainer.NameTranslate.ContainsKey(name);
        public async Task<MessageHandler> UpdateSettings(string value, string fieldName, string editType, MessagePrinter messagePrinter)
        {
            bool success = SettingsContainer.NameTranslate.TryGetValue(fieldName, out string? fieldNameQualified);
            if (success == false)
                return MessageProvider.ParameterDoesNotExist(fieldName);
            DynamicJsonProperty field = Settings.Fields[fieldNameQualified!];
            success = EditTypes.TryGetValue(editType, out EditType type);
            if (success == false)
                return MessageProvider.UnknownSettingsUsage(editType);
            return await UpdateSettings(value, field, type, messagePrinter);
        }
        private async Task<MessageHandler> UpdateSettings(string value, DynamicJsonProperty field, EditType editType, MessagePrinter messagePrinter)
        {
            if (((editType == EditType.AddValueToField || editType == EditType.RemoveValueFromField) && field.Type != DynamicJsonProperty.UsedType.Array) || (editType == EditType.SetFieldToValue && field.Type == DynamicJsonProperty.UsedType.Array))
                return MessageProvider.InvalidEditType(field.Type, editType);
            try
            {
                field.Value = editType switch
                {
                    EditType.AddValueToField => ((DynamicJsonProperty[])field.Value).Cast<dynamic>().Contains(new DynamicJsonProperty(field.TypeOfArray ?? DynamicJsonProperty.UsedType.Integer, value)) ? field.Value : ((DynamicJsonProperty[])field.Value).Cast<dynamic>().Append(value).ToArray(),
                    EditType.RemoveValueFromField => (from entry in (DynamicJsonProperty[])field.Value
                                                        where entry.Value
                                                        != (field.TypeOfArray switch
                                                        {
                                                            DynamicJsonProperty.UsedType.String => (dynamic)value,
                                                            DynamicJsonProperty.UsedType.Integer => (dynamic)Convert.ToInt32(value),
                                                            DynamicJsonProperty.UsedType.FloatingPoint => (dynamic)Convert.ToDouble(value),
                                                            DynamicJsonProperty.UsedType.Boolean => (dynamic)Convert.ToBoolean(value),
                                                            _ => throw new ArgumentException("Unsupported ArrayType.")
                                                        })
                                                        select entry).ToArray(),
                    EditType.SetFieldToValue => value,
                    _ => throw new ArgumentOutOfRangeException(nameof(editType), editType, "Unknown editType.")
                };
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                return MessageProvider.InvalidType("value", field.TypeOfArray ?? field.Type);
            }
            var save = await SaveSettings();
            if (save.IsSuccess(messagePrinter) == false)
                return save;
            return MessageProvider.Success();
        }
        private async Task<MessageHandler> SaveSettings()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            using (FileStream fs = File.Create(Path))
            {
                await JsonSerializer.SerializeAsync(fs, Settings, options);
            }
            return MessageProvider.Success();
        }
        private async Task<MessageHandler> GetSettings(MessagePrinter messagePrinter)
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
                    var helper = await JsonSerializer.DeserializeAsync<SettingsContainerHelper>(fs);
                    SettingsContainer oldSettings = Settings;
                    Settings = new SettingsContainer(helper);
                    var validate = Settings.Validate(Path);
                    if (validate.IsSuccess(messagePrinter) == false)
                        return validate;
                }
                catch (JsonException ex)
                {
                    return MessageProvider.InvalidJsonFileFormat(Path, ex.Message);
                }
            }
            return MessageProvider.Success();
        }
        public static async Task<(BackUpSettings?, MessageHandler)> GetFromFile(string path, MessagePrinter messagePrinter)
        {
            var settings = new BackUpSettings(path);
            var message = await settings.GetSettings(messagePrinter);
            if (message.IsSuccess(messagePrinter) == false)
                return (null, message);
            return (settings, MessageProvider.Success());
        }
        public override string ToString()
        {
            string result = string.Empty;
            result += $"Path: {Path}{Environment.NewLine}";
            result += $"Selected paths:{Environment.NewLine}\t{Settings.Paths.CustomToString($"{Environment.NewLine}\t")}{Environment.NewLine}";
            result += $"Blacklisted extensions:{Environment.NewLine}\t{Settings.BlacklistedExtensions.CustomToString($"{Environment.NewLine}\t")}{Environment.NewLine}";
            return result;
        }
    }
}

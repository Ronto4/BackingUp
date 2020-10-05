﻿using System;
using System.Collections.Generic;

namespace BackingUpConsole.Utilities.Messages
{
    public static class MessageCollections
    {
        public enum Codes
        {
            //QuitProgram,
            UnknownCommand,
            Success,
            IncorrectArgumentCount,
            FileNotFound,
            ParseError,
            ParseSuccess,
            RuntimeError,
            InvalidMethodExecution,
            ExecutionDebug,
            DirectoryNotFound,
            ChangedDirectory,
            ParseChangedDirectory,
            ExecutionSuccess,
            Message,
            CompatibilityMode,
            UnknownFlagIdentifier,
            InvalidFlagNotation,
            InvalidFlagValue,
            UnknownReportLevel,
            ReportLevelChanged,
            MixedArguments,
            UnknownArgument,
            InvalidArgumentNotation,
            BackingUpUnknownMode,
            InvalidExtension,
            InvalidFileFormat,
            DoubledName,
            BackupEntryAdded,
            BackupNotFound,
            BackupEntryRemoved,
            BackupChanged,
            ParsingBackupChanged,
            DirectoryNotEmpty,
            BackingUpUnknownUsage,
            BackupCreated,
            InvalidFileVersion,
            NoBackupSelected,
            ParameterDoesNotExist,
            UnknownSettingsUsage,
            SettingsUpdated,
            InvalidEditType,
            InvalidFileType,
            InvalidJsonFileFormat,
            InvalidType,
            BackUpSettingsSelected
        }
        public enum Levels
        {
            Fatal = 0,
            Error = 1,
            Warning = 2,
            Information = 3,
            Debug = 4
        }

        public static readonly Dictionary<Levels, ConsoleColor> Colors = new Dictionary<Levels, ConsoleColor>()
        {
            {Levels.Fatal, ConsoleColor.DarkRed },
            {Levels.Error, ConsoleColor.Red },
            {Levels.Warning, ConsoleColor.Yellow },
            {Levels.Information, ConsoleColor.White },
            {Levels.Debug, ConsoleColor.Gray }
        };
    }
}

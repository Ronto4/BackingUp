using BackingUpConsole.Utilities.Commands;
using System;
using System.Runtime.CompilerServices;

namespace BackingUpConsole.Utilities.Messages
{
    public static class MessageProvider
    {
        //public static MessageHandler QuitProgram() => new MessageHandler(MessageCollections.Codes.QuitProgram,
        //                                                                 $"Quit Program",
        //                                                                 MessageCollections.Levels.Information);
        public static MessageHandler UnknownCommand(string cmd) => new MessageHandler(MessageCollections.Codes.UnknownCommand,
                                                                                      $"Unknown command '{cmd}'",
                                                                                      MessageCollections.Levels.Error);
        public static MessageHandler Success() => new MessageHandler(MessageCollections.Codes.Success,
                                                                     "Task performed successfully",
                                                                     MessageCollections.Levels.Debug);
        public static MessageHandler IncorrectArgumentCount() => new MessageHandler(MessageCollections.Codes.IncorrectArgumentCount,
                                                                                    $"Incorrect number of arguments given",
                                                                                    MessageCollections.Levels.Error);
        public static MessageHandler FileNotFound(string path) => new MessageHandler(MessageCollections.Codes.FileNotFound,
                                                                                     $"Cannot find file '{path}'",
                                                                                     MessageCollections.Levels.Error);
        public static MessageHandler ParseError(MessageHandler message, string position) => new MessageHandler(MessageCollections.Codes.ParseError,
                                                                                                               $"Parse error at {position}:\n{message.Message}",
                                                                                                               message.Level < MessageCollections.Levels.Error ? message.Level : MessageCollections.Levels.Error);
        public static MessageHandler ParseSuccess() => new MessageHandler(MessageCollections.Codes.ParseSuccess,
                                                                          "Parsing successful",
                                                                          MessageCollections.Levels.Information,
                                                                          ConsoleColor.Green);
        public static MessageHandler RuntimeError(MessageHandler message, string position) => new MessageHandler(MessageCollections.Codes.RuntimeError,
                                                                                                                 $"Error while executing code at {position}:{Environment.NewLine}{message.Message}",
                                                                                                                 message.Level < MessageCollections.Levels.Error ? message.Level : MessageCollections.Levels.Error);
        public static MessageHandler InvalidMethodExecution(UInt16? flags, string[]? args, string description,
                                                            [CallerLineNumber] int lineNumber = 0,
                                                            [CallerMemberName] string caller = "",
                                                            [CallerFilePath] string filePath = "")
        {
            string messageString = $"Fatal error while executing '{caller}' at line {lineNumber} in file '{filePath}{Environment.NewLine}";
            if (flags != null)
                messageString += $"Flags (Base 2): {Convert.ToString((UInt16)flags, toBase: 2)}{Environment.NewLine}";
            if (args != null && args.Length > 0)
            {
                messageString += $"Arguments:";
                for (int i = 0; i < args.Length; i++)
                {
                    messageString += $"{Environment.NewLine}{args[i]}";
                }
                messageString += $"{Environment.NewLine}";
            }
            messageString += $"Description: {description}";
            return new MessageHandler(MessageCollections.Codes.InvalidMethodExecution, messageString, MessageCollections.Levels.Fatal);
        }
        public static MessageHandler ExecutionDebug(Command cmd, UInt16 flags, string[] args, Paths paths)
        {
            string messageString = $"Executing command '{cmd.cmd}'{Environment.NewLine}";
            messageString += $" Flags (Base 2): {Convert.ToString(flags, 2)}{Environment.NewLine}";
            messageString += $" Arguments:";
            for (int i = 0; i < args.Length; i++)
            {
                messageString += $"{Environment.NewLine}  {args[i]}";
            }
            messageString += $"{Environment.NewLine} Paths: {paths}";
            return new MessageHandler(MessageCollections.Codes.ExecutionDebug, messageString, MessageCollections.Levels.Debug);
        }
        public static MessageHandler DirectoryNotFound(string path) => new MessageHandler(MessageCollections.Codes.DirectoryNotFound,
                                                                                     $"Cannot find directory '{path}'",
                                                                                     MessageCollections.Levels.Error);
        public static MessageHandler DirectoryChanged(string path) => new MessageHandler(MessageCollections.Codes.ChangedDirectory,
                                                                                         $"Working directory updated to '{path}'",
                                                                                         MessageCollections.Levels.Information);
        public static MessageHandler DirectoryChanged() => DirectoryChanged(String.Empty);
        public static MessageHandler ParseDirectoryChanged() => new MessageHandler(MessageCollections.Codes.ParseChangedDirectory,
                                                                                              $"Working directory can be updated",
                                                                                              MessageCollections.Levels.Debug);
        public static MessageHandler ExecutionSuccess() => new MessageHandler(MessageCollections.Codes.ExecutionSuccess,
                                                                         "Execution successful",
                                                                         MessageCollections.Levels.Information,
                                                                         ConsoleColor.Green);
        public static MessageHandler Message(string message, MessageCollections.Levels level = MessageCollections.Levels.Information, ConsoleColor? color = null)
            => new MessageHandler(MessageCollections.Codes.Message, message, level, color);
        public static MessageHandler CompatibilityMode() => new MessageHandler(MessageCollections.Codes.CompatibilityMode,
                                                                               "You are entering the compatibility mode. This mode contains the old commands from BackUp_0_3. These are diiferent than the new ones. There is no support for these commands.",
                                                                               MessageCollections.Levels.Warning);
    }
}

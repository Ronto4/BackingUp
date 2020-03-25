#nullable enable

using System;

namespace BackingUpConsole.Utilities.Messages
{
    public class MessageProvider
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
                                                                          System.ConsoleColor.Green);
        public static MessageHandler RuntimeError(MessageHandler message, string position) => new MessageHandler(MessageCollections.Codes.RuntimeError,
                                                                                                                 $"Error while executing code at {position}:{Environment.NewLine}{message.Message}",
                                                                                                                 message.Level < MessageCollections.Levels.Error ? message.Level : MessageCollections.Levels.Error);
        public static MessageHandler InvalidMethodExecution(string method, UInt16? flags, string[]? args, string description)
        {
            string messageString = $"Fatal error while executing method '{method}'{Environment.NewLine}";
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
        public static MessageHandler ExecutionDebug(Commands.Command cmd, UInt16 flags, string[] args)
        {
            string messageString = $"Executing command '{cmd.cmd}'{Environment.NewLine}";
            messageString += $" Flags (Base 2): {Convert.ToString(flags, 2)}{Environment.NewLine}";
            messageString += $" Arguments:";
            for (int i = 0; i < args.Length; i++)
            {
                messageString += $"{Environment.NewLine}  {args[i]}";
            }
            return new MessageHandler(MessageCollections.Codes.ExecutionDebug, messageString, MessageCollections.Levels.Debug);
        }
    }
}

namespace BackingUpConsole.Utilities.Messages
{
    public class MessageProvider
    {
        public static MessageHandler QuitProgram() => new MessageHandler(MessageCollections.Codes.QuitProgram,
                                                                         $"Quit Program",
                                                                         MessageCollections.Levels.Information);
        public static MessageHandler UnknownCommand(string cmd) => new MessageHandler(MessageCollections.Codes.UnknownCommand,
                                                                                      $"Unknown command '{cmd}'",
                                                                                      MessageCollections.Levels.Error,
                                                                                      System.ConsoleColor.Red);
        public static MessageHandler Success() => new MessageHandler(MessageCollections.Codes.Success,
                                                                     "Task performed successfully",
                                                                     MessageCollections.Levels.Debug);
        public static MessageHandler IncorrectArgumentCount() => new MessageHandler(MessageCollections.Codes.IncorrectArgumentCount,
                                                                                    $"Incorrect number of arguments given",
                                                                                    MessageCollections.Levels.Error,
                                                                                    System.ConsoleColor.Red);
        public static MessageHandler FileNotFound(string path) => new MessageHandler(MessageCollections.Codes.FileNotFound,
                                                                                     $"Cannot find file {path}",
                                                                                     MessageCollections.Levels.Error,
                                                                                     System.ConsoleColor.Red);
        public static MessageHandler ParseError(MessageHandler message, string position) => new MessageHandler(MessageCollections.Codes.ParseError,
                                                                                                               $"Parse error at {position}:\n{message.Message}",
                                                                                                               MessageCollections.Levels.Error,
                                                                                                               System.ConsoleColor.Red);
        public static MessageHandler ParseSuccess() => new MessageHandler(MessageCollections.Codes.ParseSuccess,
                                                                          "Parsing successful",
                                                                          MessageCollections.Levels.Information,
                                                                          System.ConsoleColor.Green);
    }
}

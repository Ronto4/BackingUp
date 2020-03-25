namespace BackingUpConsole.Utilities.Messages
{
    public class MessageCollections
    {
        public enum Codes
        {
            QuitProgram,
            UnknownCommand,
            Success,
            IncorrectArgumentCount,
            FileNotFound
        }
        public enum Levels
        {
            Fatal,
            Error,
            Warning,
            Information,
            Debug
        }
    }
}

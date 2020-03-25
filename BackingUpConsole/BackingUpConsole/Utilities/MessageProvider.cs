namespace BackingUpConsole.Utilities.Messages
{
    public class MessageProvider
    {
        public static MessageHandler QuitProgram() => new MessageHandler(MessageCollections.Codes.QuitProgram, $"Quit Program", MessageCollections.Levels.Information);
    }
}

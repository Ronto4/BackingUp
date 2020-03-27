namespace BackingUpConsole.Utilities.Commands
{
    public static class CommandCollections
    {
        public static Command RunFile => new Command("run");
        public static Command Exit => new Command("exit");
        public static Command Cd => new Command("cd");

        public static Command GetCommand(string cmd) => new Command(cmd);
    }
}

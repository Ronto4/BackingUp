namespace BackingUpConsole.Utilities.Commands
{
    public class Command
    {
        //Attributes
        public readonly string cmd;
        //Constructors
        public Command(string cmd)
        {
            this.cmd = cmd;
        }

        //overrides
        public override bool Equals(object obj)
        {
            return this == (Command)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return cmd;
        }


        public static bool operator ==(Command left, Command right) => left.cmd == right.cmd;
        public static bool operator !=(Command left, Command right) => !(left == right);
    }
}

using System;

namespace BackingUpConsole.Utilities.Commands
{
    public class Command
    {
        //Attributes
        public readonly string cmd;

        public readonly CommandProperties properties;

        public bool IsComment => cmd.StartsWith(";");
        public bool IsInvalid => properties.Parse == null;

        //Constructors
        public Command(string cmd)
        {
            this.cmd = cmd;
            CommandCollections.Properties.TryGetValue(cmd, out properties);
        }

        //overrides
        public override bool Equals(object? command)
        {
            if (command is Command c)
            {
                return this == c;
            }
            throw new ArgumentException($"'{nameof(command)}' is not an '{typeof(Command)}' object.");
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(cmd);
        }

        public override string ToString()
        {
            return cmd;
        }


        public static bool operator ==(Command left, Command right) => left.cmd == right.cmd;
        public static bool operator !=(Command left, Command right) => !(left == right);
    }
}

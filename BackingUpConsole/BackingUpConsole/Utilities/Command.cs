using System;

namespace BackingUpConsole.Utilities.Commands
{
    public class Command
    {
        //Attributes
        public readonly string cmd;

        public readonly CommandProperties properties;

        public readonly string[]? ArgOrder;

        public bool IsComment => cmd.StartsWith(";");
        //public bool IsInvalid => properties.Parse == null && properties.Parse_Async is null;
        public bool IsInvalid => properties.ParseIsAsync ? properties.Parse_Async is null : properties.Parse is null;

        //Constructors
        public Command(string cmd)
        {
            this.cmd = cmd;
            CommandCollections.Properties.TryGetValue(cmd, out properties);
            CommandCollections.ArgumentOrder.TryGetValue(cmd, out ArgOrder);
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

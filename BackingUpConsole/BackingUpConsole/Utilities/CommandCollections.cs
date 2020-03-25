using System;
using System.Collections.Generic;
using System.Text;

namespace BackingUpConsole.Utilities.Commands
{
    public class CommandCollections
    {
        public static Command RunFile => new Command("run");

        public static Command GetCommand(string cmd) => new Command(cmd);
    }
}

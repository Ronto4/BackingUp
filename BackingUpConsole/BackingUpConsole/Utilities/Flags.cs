using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackingUpConsole.Utilities
{
    public static class Flags
    {
        public static UInt16 RUN => 0b0000_0000_0000_0001;
        public static UInt16 COMPILE => 0b0000_0000_0000_0010;
        public static UInt16 CHAIN_COMPILE => 0b0000_0000_0000_0100;
        public static UInt16 VERBOSE => 0b0000_0000_0000_1000;

        public static UInt16 DEFAULT_FLAGS => (UInt16)(0x0 | RUN | COMPILE | CHAIN_COMPILE);

        private static readonly Dictionary<string, UInt16> flagIdentifier = new Dictionary<string, ushort>()
        {
            {"run", RUN },
            {"compile", COMPILE },
            {"chain-compile", CHAIN_COMPILE },
            {"verbose", VERBOSE }
        };

        public static bool IsSet(this UInt16 flags, UInt16 checkFlag) => (flags & checkFlag) != 0;
       
        public static (MessageHandler message, UInt16 flags) SetFromArgs(this UInt16 flags, ref string[] args)
        {
            var relevantArgs = from arg in args where arg.StartsWith("--") select arg.Substring(2);
            foreach(var arg in relevantArgs)
            {
                string[] parts = arg.Split(':');
                if (parts.Length != 2)
                    return (MessageProvider.InvalidFlagNotation(arg), flags);

                string flagId = parts[0];
                string flagValue = parts[1];

                bool validFlag = flagIdentifier.TryGetValue(flagId, out UInt16 flag);
                if (!validFlag)
                    return (MessageProvider.UnknownFlagIdentifier(flagId, flagValue), flags);

                bool validValue = uint.TryParse(flagValue, out uint flagSet);
                if (!validValue || flagSet > 1)
                    return (MessageProvider.InvalidFlagValue(flagId, flagValue), flags);

                flags = (UInt16)(flagSet == 0 ? flags & ~flag : flags | flag);
            }
            args = (from arg in args where !arg.StartsWith("--") select arg).ToArray();
            return (MessageProvider.Success(), flags);

        }
    }
}

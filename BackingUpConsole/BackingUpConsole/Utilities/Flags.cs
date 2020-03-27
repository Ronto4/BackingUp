using System;

namespace BackingUpConsole.Utilities
{
    public class Flags
    {
        public static UInt16 RUN => 0b0000_0000_0000_0001;
        public static UInt16 COMPILE => 0b0000_0000_0000_0010;
        public static UInt16 CHAIN_COMPILE => 0b0000_0000_0000_0100;

        public static UInt16 DEFAULT_FLAGS => (UInt16)(0x0 | RUN | COMPILE | CHAIN_COMPILE);
       
    }
}

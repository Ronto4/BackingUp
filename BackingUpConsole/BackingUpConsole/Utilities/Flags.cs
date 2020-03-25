﻿using System;

namespace BackingUpConsole.Utilities
{
    public class Flags
    {
        public static UInt16 ONLY_COMPILE => 0b0000_0000_0000_0001;
        public static UInt16 CHAIN_COMPILE => 0b0000_0000_0000_0010;
        public static UInt16 NO_COMPILE => 0b0000_0000_0000_0100;
    }
}

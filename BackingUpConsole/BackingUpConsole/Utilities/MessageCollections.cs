using System;
using System.Collections.Generic;

namespace BackingUpConsole.Utilities.Messages
{
    public class MessageCollections
    {
        public enum Codes
        {
            //QuitProgram,
            UnknownCommand,
            Success,
            IncorrectArgumentCount,
            FileNotFound,
            ParseError,
            ParseSuccess,
            RuntimeError,
            InvalidMethodExecution,
            ExecutionDebug
        }
        public enum Levels
        {
            Fatal = 0,
            Error = 1,
            Warning = 2,
            Information = 3,
            Debug = 4
        }

        public static readonly Dictionary<Levels, ConsoleColor> Colors = new Dictionary<Levels, ConsoleColor>()
        {
            {Levels.Fatal, ConsoleColor.DarkRed },
            {Levels.Error, ConsoleColor.Red },
            {Levels.Warning, ConsoleColor.Yellow },
            {Levels.Information, ConsoleColor.White },
            {Levels.Debug, ConsoleColor.Gray }
        };
    }
}

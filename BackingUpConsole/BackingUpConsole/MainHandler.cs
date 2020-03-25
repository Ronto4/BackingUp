#define DEBUG_MSG

using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Runtime.InteropServices;

namespace BackingUpConsole
{
    class MainHandler
    {
        static void Main(string[] args)
        {
#if DEBUG_MSG
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Debug, System.ConsoleColor.Gray);
#else
            MessagePrinter messagePrinter = new MessagePrinter(MessageCollections.Levels.Information, System.ConsoleColor.Gray);
#endif
            if (args.Length > 0)
            {
                MessageHandler result = CLIInterpreter(args, messagePrinter);
                messagePrinter.Print(result);
                if (result == MessageProvider.QuitProgram())
                    return;
            }
            bool exit = false;
            while (!exit)
            {
                string input = Console.ReadLine();
                string[] arg = CommandLineToArgs(input);
                MessageHandler result = CLIInterpreter(arg, messagePrinter);
                messagePrinter.Print(result);
                if (result == MessageProvider.QuitProgram())
                    exit = true;
            }
        }

        private static MessageHandler CLIInterpreter(string[] args, MessagePrinter messagePrinter)
        {
            Command cmd = CommandCollections.GetCommand(args[0]);
            string[] arg = new string[args.Length - 1];
            for (int i = 0; i < arg.Length; i++)
            {
                arg[i] = args[i + 1];
            }
            return Interpreter.Interprete(cmd, arg, messagePrinter, (UInt16)(0x0 | Flags.CHAIN_COMPILE));
            //return MessageProvider.QuitProgram();
        }


        //Source: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
    [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}
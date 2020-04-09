using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class Create
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!args.CheckLength(2, 4))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            DirectoryInfo dir = new DirectoryInfo(args[1]);
            if (dir.Exists)
            {
                if (dir.GetDirectories().Length + dir.GetFiles().Length != 0)
                {
                    MessageHandler m = MessageProvider.DirectoryNotEmpty(dir.FullName, silent: !flags.IsSet(Flags.VERBOSE));
                    if (!m.IsSuccess(true, messagePrinter))
                        return m;
                }
            }
            if (args.Length > 2)
            {
                (MessageHandler readSuccess, Dictionary<string, string>? entries) = Utilities.ScanList();
                if (!readSuccess.IsSuccess(true, messagePrinter))
                    return readSuccess;

                if (entries!.ContainsKey(args[2]))
                    return MessageProvider.DoubledName(args[2], !flags.IsSet(Flags.VERBOSE));
            }
            if (args.Length == 4)
                if (args[3] != "select")
                    return MessageProvider.BackingUpUnknownUsage(args[3], !flags.IsSet(Flags.VERBOSE));

            return MessageProvider.Success();
        }
        public static async Task<MessageHandler> Run(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            DirectoryInfo dir = new DirectoryInfo(args[1]);
            if (!dir.Exists)
                dir.Create();

            string origContent = ConstantValues.DEFAULT_BACKUP_FILE;
            //Console.WriteLine($"---{Environment.NewLine}{origContent}{Environment.NewLine}---");
            origContent = origContent.Replace("*\\", $"{dir.FullName}\\");

            using (FileStream fs = File.Create(PathHandler.Combine(dir.FullName, @"container.bu")))
            {
                await fs.WriteAsync(origContent.ToCharArray().Select(c => (byte)c).ToArray().AsMemory());
            }
            return MessageProvider.Success();
        }
    }
}

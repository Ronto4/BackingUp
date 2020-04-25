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

                if (args.Length == 4)
                    if (args[3] != "select")
                        return MessageProvider.BackingUpUnknownUsage(args[3], !flags.IsSet(Flags.VERBOSE));
            }

            return MessageProvider.Success();
        }
        public static async Task<MessageHandler> Run(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            DirectoryInfo dir = new DirectoryInfo(args[1]);
            if (!dir.Exists)
                dir.Create();

            string origContent = ConstantValues.DEFAULT_BACKUP_FILE;
            origContent = origContent.Replace("*\\", $"{dir.FullName}\\");

            string path = PathHandler.Combine(dir.FullName, @"container.bu");

            using (FileStream fs = File.Create(path))
            {
                await fs.WriteAsync(origContent.ToCharArray().Select(c => (byte)c).ToArray().AsMemory());
            }
            BackUpFile.GetFromFile(path, true);

            if (args.Length > 2)
            {
                MessageHandler addResult = await Commands.Add.RunAsync(new string[] { "add", path, args[2] }, flags, paths, messagePrinter);
                if (addResult.Code != MessageCollections.Codes.BackupEntryAdded)
                    return addResult;

                if (flags.IsSet(Flags.VERBOSE))
                    messagePrinter.Print(addResult);

                if (args.Length == 4)
                {
                    //(MessageHandler selectResult, BackUpFile? bu) = BackUpFile.GetFromFile(path);
                    //if (!selectResult.IsSuccess(false, messagePrinter))
                    //    return selectResult;

                    //paths.SelectedBackup = bu!;

                    MessageHandler selectResult = await Commands.Select.RunAsync(new string[] { "select", args[2] }, flags, paths, messagePrinter);
                    if (selectResult.Code != MessageCollections.Codes.BackupChanged)
                        return selectResult;

                    if (flags.IsSet(Flags.VERBOSE))
                        messagePrinter.Print(selectResult);
                }

            }
            return MessageProvider.BackupCreated(path, !flags.IsSet(Flags.VERBOSE));
        }
    }
}

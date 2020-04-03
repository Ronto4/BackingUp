using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions.Commands
{
    internal static class List
    {
        public static MessageHandler Parse(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            if (!args.CheckLength(1, 1))
                return MessageProvider.IncorrectArgumentCount(!flags.IsSet(Flags.VERBOSE));

            (MessageHandler message, _) = Utilities.ScanList();
            if (message.IsSuccess(true, messagePrinter))
                return MessageProvider.Success(!flags.IsSet(Flags.VERBOSE));

            return message;
        }
        public async static Task<MessageHandler> RunAsync(string[] args, UInt16 flags, Paths paths, MessagePrinter messagePrinter)
        {
            string listDir = PathHandler.Combine(Environment.CurrentDirectory, "data");
            string listPath = PathHandler.Combine(listDir, "backups.bul");
            if (!File.Exists(listPath))
            {
                MessageHandler result = await Utilities.WriteListFileAsync(new Dictionary<string, string>());
                if (!result.IsSuccess(false, messagePrinter))
                    return result;
                //if (!Directory.Exists(listDir))
                //    Directory.CreateDirectory(listDir);

                //File.Create(listPath).Close();
                //string basis = $"[BackUps]";
                //using StreamWriter sw = new StreamWriter(listPath);
                ////Parallel.ForEach<char>(basis.ToCharArray(), (c) => sw.WriteAsync(c));
                //await sw.WriteAsync(basis);

                //sw.Close();
            }
            string message = String.Empty;
            message += $"Name | Path{Environment.NewLine}";
            ////List<Task<string?>> tasks = new List<Task<string?>>();
            //List<string?> results = new List<string?>();
            //using (StreamReader sr = new StreamReader(listPath))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        //tasks.Add(Task.Run(() => sr.ReadLineAsync()));
            //        results.Add(await sr.ReadLineAsync());
            //    }
            //    //results = await Task.WhenAll(tasks);
            //}
            //for (int i = 1; i < results.Count; i++)
            //{
            //    if (results[i] is null)
            //        continue;

            //    string[] parts = results[i]!.Split('?');
            //    if (!File.Exists(parts[1]))
            //        return MessageProvider.FileNotFound(parts[1], flags.IsSet(Flags.VERBOSE));

            //    if (!(new FileInfo(parts[1]).Extension == ".bu"))
            //        return MessageProvider.InvalidExtension(parts[1], "bu", flags.IsSet(Flags.VERBOSE));

            //    message += $"{parts[0]} | {parts[1]} {Environment.NewLine}";
            //}

            (MessageHandler error, Dictionary<string, string>? entries) = await Utilities.ScanListAsync();
            if (entries is null)
                return MessageProvider.InvalidMethodExecution(flags, args, $"'BackingUpConsole.CoreFunctions.Utilities.ScanListAsync' returned null Dictionary in mode 'run' {Environment.NewLine} Error message: '{error.Message}'", silent: !flags.IsSet(Flags.VERBOSE));

            foreach (var entry in entries)
            {
                message += $"{entry.Key} | {entry.Value} {Environment.NewLine}";
            }

            return MessageProvider.Message(message, silent: !flags.IsSet(Flags.VERBOSE));
        }
    }
}

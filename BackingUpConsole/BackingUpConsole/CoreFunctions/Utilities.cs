using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    internal static class Utilities
    {
        private static readonly string ListIndentifier = $"BackUpList";
        internal static (MessageHandler message, Dictionary<string, string>? entries) ScanList()
        {
            string path = PathHandler.Combine(Environment.CurrentDirectory, @"data\backups.bul");
            if (!File.Exists(path))
                return (MessageProvider.Success(), new Dictionary<string, string>());

            List<string?> results = new List<string?>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    results.Add(sr.ReadLine());
                }
            }
            //Console.WriteLine($"'{Print(results[0]!)}' | '{Print(ListIndentifier)}'");
            if (results[0] != $"[{ListIndentifier}]")
                return (MessageProvider.InvalidFileFormat(path, 1), null);

            if (results[1] != "*version:1")
                return (MessageProvider.InvalidFileFormat(path, 2), null);

            Dictionary<string, string> splitRes = new Dictionary<string, string>();

            for (int i = 2; i < results.Count; i++)
            {
                if (results[i] is null || !IsMatch(results[i]!))
                    return (MessageProvider.InvalidFileFormat(path, i + 1), null);

                string[] split = results[i]!.Split('?');
                if(split.Length != 2 || !splitRes.TryAdd(split[0], split[1]))
                    return (MessageProvider.InvalidFileFormat(path, i + 1), null);
            }
            foreach (var file in splitRes)
            {
                if (!File.Exists(file.Value) || new FileInfo(file.Value).Extension != ".bu")
                    return (MessageProvider.FileNotFound(file.Value), null);
            }
            return (MessageProvider.Success(), splitRes); 
        }

        internal static async Task<(MessageHandler message, Dictionary<string, string>? entries)> ScanListAsync()
        {
            string path = PathHandler.Combine(Environment.CurrentDirectory, @"data\backups.bul");
            if (!File.Exists(path))
                return (MessageProvider.Success(), new Dictionary<string, string>());

            List<string?> results = new List<string?>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    results.Add(await sr.ReadLineAsync());
                }
            }
            if (results[0] != $"[{ListIndentifier}]")
                return (MessageProvider.InvalidFileFormat(path, 1), null);

            if (results[1] != "*version:1")
                return (MessageProvider.InvalidFileFormat(path, 2), null);

            Dictionary<string, string> splitRes = new Dictionary<string, string>();

            bool errorOccured = false;
            string? errorLineContent = null;
            Parallel.For(2, results.Count, body: (pos) =>
           {
               if (results[pos] is null || !IsMatch(results[pos]!))
               {
                   errorOccured = true;
                   errorLineContent = results[pos];
                   return;
               }
               string[] split = results[pos]!.Split('?');
               Console.WriteLine($"splits: split[0]={split[0]} ; split[1]={split[1]}");
               if (!splitRes.TryAdd(split[0], split[1]))
               {
                   errorOccured = true;
                   errorLineContent = results[pos];
               }
           });
            if (errorOccured)
                return (MessageProvider.InvalidFileFormat(path, errorLineContent!), null);

            errorOccured = false;
            errorLineContent = null;
            Parallel.ForEach(splitRes, (file) =>
            {
                if (!File.Exists(file.Value) || new FileInfo(file.Value).Extension != ".bu")
                {
                    errorOccured = true;
                    errorLineContent = file.Value;
                    return;
                }
            });
            if (errorOccured)
                return (MessageProvider.FileNotFound(errorLineContent!), null);

            return (MessageProvider.Success(), splitRes);
        }

        private static bool IsMatch(string line)
        {
            string[] parts = line.Split('?');
            if (parts.Length != 2)
                return false;
            try
            {
                Path.GetFullPath(parts[1]);
                if (!Path.IsPathRooted(parts[1]))
                    throw new Exception();
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static async Task<MessageHandler> WriteListFileAsync(Dictionary<string, string> content)
        {
            try
            {
                string listDir = PathHandler.Combine(Environment.CurrentDirectory, "data");
                string listPath = PathHandler.Combine(listDir, "backups.bul");
                if (!Directory.Exists(listDir))
                    Directory.CreateDirectory(listDir);

                if (!File.Exists(listPath))
                    File.Create(listPath).Close();

                string basis = $"[{ListIndentifier}]\n*version:1";
                using StreamWriter sw = new StreamWriter(listPath);
                //Parallel.ForEach<char>(basis.ToCharArray(), (c) => sw.WriteAsync(c));
                await sw.WriteAsync(basis);

                foreach (var line in content)
                {
                    await sw.WriteAsync($"{Environment.NewLine}{line.Key}?{line.Value}");
                }

                sw.Close();
            }
            catch (Exception ex)
            {
                return MessageProvider.Message(ex.Message, MessageCollections.Levels.Error);
            }
            return MessageProvider.Success();
        } 
    }
}

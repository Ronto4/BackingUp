using BackingUpConsole.Utilities;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    class Utilities
    {
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
            if (results[0] != "[BackUps]")
                return (MessageProvider.InvalidFileFormat(path, 1), null);

            Dictionary<string, string> splitRes = new Dictionary<string, string>();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i] is null || !IsMatch(results[i]!))
                    return (MessageProvider.InvalidFileFormat(path, i + 1), null);

                string[] split = results[i]!.Split('?');
                if(!splitRes.TryAdd(split[0], split[1]))
                    return (MessageProvider.InvalidFileFormat(path, i + 1), null);
            }
            foreach (var file in splitRes)
            {
                if (!File.Exists(file.Value) || new FileInfo(file.Value).Extension != ".bu")
                    return (MessageProvider.FileNotFound(file.Value), null);
            }
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
    }
}

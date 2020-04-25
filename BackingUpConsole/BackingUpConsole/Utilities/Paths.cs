using BackingUpConsole.CoreFunctions;
using System;
using System.IO;

namespace BackingUpConsole.Utilities
{
    public class Paths : ICloneable
    {
        public string CurrentWorkingDirectory { get; set; }
        public BackUpFile? SelectedBackup { get; set; }

        public Paths(string currWorkDir)
        {
            CurrentWorkingDirectory = currWorkDir;
            SelectedBackup = null;
        }

        public Paths(Paths p) : this(p.CurrentWorkingDirectory) {
            SelectedBackup = p.SelectedBackup is null ? null : (BackUpFile)p.SelectedBackup.Clone();
        }

        public override string ToString()
        {
            return CurrentWorkingDirectory;
        }

        public object Clone()
        {
            return new Paths(this);
        }
    }

    public static class PathHandler
    {
        public static string Combine(params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (i < paths.Length - 1)
                    paths[i] += paths[i].EndsWith('\\') ? String.Empty : "\\";

                paths[i] = paths[i].StartsWith('\\') ? paths[i].Substring(1) : paths[i];
            }
            string path = String.Empty;
            for (int i = 0; i < paths.Length; i++)
            {
                path += paths[i];
            }
            return Flatten(path);
        }
        private static string Flatten(string path) => path.LastIndexOf(':') != 1 ? Combine(path.Split(':')[1][^1].ToString() + ":", path.Split(':')[2]) : Path.GetFullPath(path);
    }
}
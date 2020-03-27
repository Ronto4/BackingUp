using System;
using System.IO;

namespace BackingUpConsole.Utilities
{
    public struct Paths
    {
        public string currentWorkingDirectory;

        public override string ToString()
        {
            return currentWorkingDirectory;
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
            //Console.WriteLine("Combined string: " + path);
            return path;
        }
        public static string Flatten(string path) => Path.GetFullPath(path);
    }
}
using System;
using System.IO;
using BackingUpConsole.Utilities.Exceptions;

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
        public static string Combine(params string[] paths) => Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => UnixCombine(paths),
            PlatformID.Win32NT => WindowsCombine(paths),
            _ => throw new OSNotSupportedException(Environment.OSVersion)
        };

        private static string UnixCombine(params string[] paths)
        {
            for (int i = paths.Length - 1; i >= 0; i--)
            {
                if (paths[i].IsFullyQualifiedPath())
                    return UnixFlatten(paths[i..].CustomToString("/"));
            }

            return UnixFlatten(string.Concat(paths));
        }
        private static string WindowsCombine(params string[] paths)
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
            return WindowsFlatten(path);
        }

        private static string UnixFlatten(string path) => Path.GetFullPath(path);
        private static string WindowsFlatten(string path) => path.LastIndexOf(':') != 1 ? WindowsCombine(path.Split(':')[1][^1].ToString() + ":", path.Split(':')[2]) : Path.GetFullPath(path);

        public static bool IsFullyQualifiedPath(this string path) => Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => path[0] == '/',
            PlatformID.Win32NT => path[1] == ':',
            _ => throw new OSNotSupportedException(Environment.OSVersion)
        };
    }
}
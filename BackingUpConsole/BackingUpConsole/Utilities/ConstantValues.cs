using System;
using System.Collections.Generic;
using System.Text;

namespace BackingUpConsole.Utilities
{
    internal static class ConstantValues
    {
        public static string DEFAULT_BACKUP_FILE => 
            $"[BackUpContainer]{Environment.NewLine}" +
            $"*version:2{Environment.NewLine}" +
            $"settings?*\\settings{Environment.NewLine}" +
            $"selectedsettings?*\\settings\\default.buse{Environment.NewLine}" +
            $"summaries?*\\summaries{Environment.NewLine}" +
            $"logs?*\\logs{Environment.NewLine}" + 
            $"backups?*\\backup";

        public static string DEFAULT_BACKUP_SETTINGS_FILE =>
            $"[BackUpSettings]{Environment.NewLine}" +
            $"*version:1{Environment.NewLine}" +
            $"paths?";
    }
}

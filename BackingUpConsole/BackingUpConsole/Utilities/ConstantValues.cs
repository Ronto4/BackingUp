using System;
using System.Collections.Generic;
using System.Text;

namespace BackingUpConsole.Utilities
{
    internal static class ConstantValues
    {
        public static string DEFAULT_BACKUP_FILE => 
            $"[BackUp]{Environment.NewLine}" +
            $"*version:2{Environment.NewLine}" +
            $"settings?*\\settings{Environment.NewLine}" +
            $"selectedsettings?*\\settings\\default.buse{Environment.NewLine}" +
            $"summaries?*\\summaries{Environment.NewLine}" +
            $"logs?*\\logs{Environment.NewLine}" + 
            $"backups?*\\backup";
    }
}

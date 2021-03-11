using System;

namespace BackingUpConsole.Utilities.Exceptions
{
    public class OSNotSupportedException : Exception
    {
        public OSNotSupportedException(OperatingSystem os) : base(
            $"The current OS {os.VersionString} is not supported by this application.")
        {
        }
    }
}
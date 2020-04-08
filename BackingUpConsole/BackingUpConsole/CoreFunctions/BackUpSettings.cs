using System;
using System.Collections.Generic;
using System.Text;

namespace BackingUpConsole.CoreFunctions
{
    class BackUpSettings
    {
        //Attributes
        public string Path { get; }
        //Constructors
        public BackUpSettings(string path)
        {
            Path = path;
        }
    }
}

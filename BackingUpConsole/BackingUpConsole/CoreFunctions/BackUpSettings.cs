using BackingUpConsole.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingUpConsole.CoreFunctions
{
    //Note: This class is just a placeholder for now, will be implemented later.
    public class BackUpSettings
    {
        //Attributes
        public string Path { get; }
        //Constructors
        public BackUpSettings(string path)
        {
            Path = path;
        }
        //Methods
        public async Task Create()
        {
            using FileStream fs = new FileStream(Path, FileMode.CreateNew);
            await fs.WriteAsync(ConstantValues.DEFAULT_BACKUP_SETTINGS_FILE.ToCharArray().Select(c => (byte)c).ToArray().AsMemory());
            fs.Close();
        }
    }
}

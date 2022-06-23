using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace File_manager
{
    class API
    {
        public const uint MAX_PATH = 255;
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
        static public extern int GetShortPathName(string pathName, System.Text.StringBuilder shortName, int cbShortName);
    }
}

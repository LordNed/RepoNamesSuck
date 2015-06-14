using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.WindWaker;
using WEditor.WindWaker.Loaders;

namespace CLIInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            Map map = MapLoader.Load(@"C:\Users\Matt\Documents\Wind Editor\ma2room");
        }
    }
}

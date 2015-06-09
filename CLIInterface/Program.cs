using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EditorCore.WindWaker;
using EditorCore.WindWaker.Loaders;

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

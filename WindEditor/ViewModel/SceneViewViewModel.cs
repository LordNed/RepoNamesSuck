using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindEditor.UI
{
    public class SceneViewViewModel
    {
        public string TestProperty
        {
            get { return m_testProperty; }
            set { m_testProperty = value; }
        }

        private string m_testProperty = "Hello World";

        public SceneViewViewModel()
        {
            Console.WriteLine("Constructed.");
        }
    }
}

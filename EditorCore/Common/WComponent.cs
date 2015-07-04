using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor
{
    public abstract class WComponent : WObject
    {
        internal Input Input { get; set; }

        public virtual void Tick(float deltaTime) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor
{
    public class Component
    {
        public Transform Transform { get; private set; }

        public Component()
        {
            this.Transform = new Transform();
            EditorCore.HackyInstance.HackyComponents.Add(this);
        }

        public virtual void Update() { }
    }
}

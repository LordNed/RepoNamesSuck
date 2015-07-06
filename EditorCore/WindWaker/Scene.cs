using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Maps;
using WEditor.Rendering;

namespace WEditor.WindWaker
{
    public abstract class Scene
    {
        public BindingList<MapEntityData> Entities { get; set; }
        public BindingList<Mesh> Meshes { get; set; }

        public Scene()
        {
            Entities = new BindingList<MapEntityData>();
            Meshes = new BindingList<Mesh>();
        }
    }
}

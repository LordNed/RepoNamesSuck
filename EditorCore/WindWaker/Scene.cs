using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Maps;

namespace WEditor.WindWaker
{
    public abstract class Scene
    {
        public List<MapEntityData> Entities { get; set; }
    }
}

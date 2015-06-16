using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Rendering
{
    public class Mesh
    {
        public List<MeshBatch> SubMeshes { get; private set; }

        public Mesh()
        {
            SubMeshes = new List<MeshBatch>();
        }
    }
}

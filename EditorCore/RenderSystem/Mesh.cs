using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Rendering
{
    public class Mesh
    {
        /// <summary> A list of sub-meshes. Each sub-mesh has its own set of buffers for the GPU, and a material/shader. </summary>
        public List<MeshBatch> SubMeshes { get; private set; }

        /// <summary> A list of bones in this mesh. Each bone has a reference to its parent, null for no parent. </summary>
        public List<SkeletonBone> Skeleton;

        /// <summary> Bind poses for the skeleton, if specified by the file. </summary>
        public List<Matrix3x4> BindPoses;

        public Mesh()
        {
            SubMeshes = new List<MeshBatch>();
            Skeleton = new List<SkeletonBone>();
            BindPoses = new List<Matrix3x4>();
        }
    }
}

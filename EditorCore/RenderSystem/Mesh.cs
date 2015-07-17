using OpenTK;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class Mesh : IDisposable
    {
        /// <summary> A list of sub-meshes. Each sub-mesh has its own set of buffers for the GPU, and a material/shader. </summary>
        public List<MeshBatch> SubMeshes { get; private set; }

        /// <summary> A list of bones in this mesh. Each bone has a reference to its parent, null for no parent. </summary>
        public List<SkeletonBone> Skeleton;

        /// <summary> Bind poses for the skeleton, if specified by the file. </summary>
        public List<Matrix3x4> BindPoses;

        private bool m_disposed;

        public Mesh()
        {
            SubMeshes = new List<MeshBatch>();
            Skeleton = new List<SkeletonBone>();
            BindPoses = new List<Matrix3x4>();
        }

        ~Mesh()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if(disposing)
            {
                // Free any *managed* objects here.
                for (int i = 0; i < SubMeshes.Count; i++)
                    SubMeshes[i].Dispose();

                SubMeshes.Clear();
            }

            // Free any *unmanaged* objects here.
            m_disposed = true;
        }
    }
}

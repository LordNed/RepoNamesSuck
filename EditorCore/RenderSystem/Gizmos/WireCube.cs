using OpenTK;

namespace WEditor.Rendering.Gizmos
{
    public static class WireCube
    {
        public static Mesh GetMesh()
        {
            Vector3[] meshVerts = 
            { 
                new Vector3(-.5f, -.5f,  -.5f),
                new Vector3(.5f, -.5f,  -.5f),
                new Vector3(.5f, .5f,  -.5f),
                new Vector3(-.5f, .5f,  -.5f),
                new Vector3(-.5f, -.5f,  .5f),
                new Vector3(.5f, -.5f,  .5f),
                new Vector3(.5f, .5f,  .5f),
                new Vector3(-.5f, .5f,  .5f),
            };

            int[] meshIndexes =
            {
                //front
                0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };


            Mesh newMesh = new Mesh();
            MeshBatch meshBach = new MeshBatch();
            meshBach.Vertices = meshVerts;
            meshBach.Indexes = meshIndexes;
            newMesh.SubMeshes.Add(meshBach);

            return newMesh;
        }
    }
}

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
                0, 3, 7,
                0, 7, 4,
                //back
                1, 6, 2,
                6, 1, 5,
                //left
                0, 1, 2,
                0, 2, 3,
                //right
                4, 6, 5,
                6, 4, 7,
                //top
                2, 6, 3,
                6, 7, 3,
                //bottom
                0, 5, 1,
                0, 4, 5
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

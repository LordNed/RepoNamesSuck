using OpenTK;

namespace WEditor.Rendering.Gizmos
{
    public static class WireCube
    {
        public static Mesh GenerateMesh()
        {
            Vector3[] meshVerts = 
            { 
                new Vector3(-.5f, -.5f,  -.5f), // Bot - Back Left
                new Vector3(.5f, -.5f,  -.5f), // Bot - Back Right
                new Vector3(.5f, .5f,  -.5f),  // Top - Back Right
                new Vector3(-.5f, .5f,  -.5f), // Top - Back Left
                new Vector3(-.5f, -.5f,  .5f), // Bot - Front Left
                new Vector3(.5f, -.5f,  .5f), // Bot - Front Right
                new Vector3(.5f, .5f,  .5f), // Top - Front Right
                new Vector3(-.5f, .5f,  .5f), // Top - Front left
            };

            int[] meshIndexes =
            {
                // Top
                3, 2,
                2, 6,
                6, 7,
                7, 3,

                // Bottom
                0, 1,
                1, 5,
                5, 4,
                4, 0,

                // Supports
                0, 3,
                1, 2,
                5, 6,
                4, 7
            };


            Mesh newMesh = new Mesh();
            MeshBatch meshBach = new MeshBatch();
            meshBach.Vertices = meshVerts;
            meshBach.Indexes = meshIndexes;
            meshBach.PrimitveType = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
            newMesh.SubMeshes.Add(meshBach);

            return newMesh;
        }
    }
}

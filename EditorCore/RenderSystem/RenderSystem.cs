using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SharpGL;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private OpenGL m_glContext;
        private List<Camera> m_cameraList;


        float rotatePyramid = 0;
        float rquad = 0; 

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();

            // Create a Default camera
            //Camera editorCamera = new Camera();
            //m_cameraList.Add(editorCamera);

            Camera leftCamera = new Camera();
            leftCamera.ViewportRect = new Rect(0, 0, 0.5f, 1f);
            leftCamera.ClearColor = new Color(1f, 0.5f, 0, 1f);

            Camera rightCamera = new Camera();
            rightCamera.ViewportRect = new Rect(0.5f, 0f, 0.5f, 1f);
            rightCamera.ClearColor = new Color(0.5f, 0, 1f, 1f);

            m_cameraList.Add(leftCamera);
            m_cameraList.Add(rightCamera);
        }

        internal void RenderFrame()
        {
            // Update the internal editor core state.
            //  Get the OpenGL instance that's been passed to us.
            GL.ClearColor(1f, 1f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Flush();

            OpenGL gl = m_glContext;
            if (gl == null)
                return;

            gl.Enable(OpenGL.GL_SCISSOR_TEST);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                /* SETUP THE VIEWPORT FOR THE CAMERA */
                Camera camera = m_cameraList[i];

                Rect pixelRect = camera.PixelRect;
                gl.Viewport((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);
                gl.Scissor((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);

                // Clear the backbuffer
                Color clearColor = camera.ClearColor;
                gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);


                //  Reset the modelview matrix.
                gl.LoadIdentity();

                //  Move the geometry into a fairly central position.
                gl.Translate(-1.5f, 0.0f, -6.0f);

                //  Draw a pyramid. First, rotate the modelview matrix.
                gl.Rotate(rotatePyramid, 0.0f, 1.0f, 0.0f);

                //  Start drawing triangles.
                gl.Begin(OpenGL.GL_TRIANGLES);

                gl.Color(1.0f, 0.0f, 0.0f);
                gl.Vertex(0.0f, 1.0f, 0.0f);
                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Vertex(-1.0f, -1.0f, 1.0f);
                gl.Color(0.0f, 0.0f, 1.0f);
                gl.Vertex(1.0f, -1.0f, 1.0f);

                gl.Color(1.0f, 0.0f, 0.0f);
                gl.Vertex(0.0f, 1.0f, 0.0f);
                gl.Color(0.0f, 0.0f, 1.0f);
                gl.Vertex(1.0f, -1.0f, 1.0f);
                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Vertex(1.0f, -1.0f, -1.0f);

                gl.Color(1.0f, 0.0f, 0.0f);
                gl.Vertex(0.0f, 1.0f, 0.0f);
                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Vertex(1.0f, -1.0f, -1.0f);
                gl.Color(0.0f, 0.0f, 1.0f);
                gl.Vertex(-1.0f, -1.0f, -1.0f);

                gl.Color(1.0f, 0.0f, 0.0f);
                gl.Vertex(0.0f, 1.0f, 0.0f);
                gl.Color(0.0f, 0.0f, 1.0f);
                gl.Vertex(-1.0f, -1.0f, -1.0f);
                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Vertex(-1.0f, -1.0f, 1.0f);

                gl.End();

                //  Reset the modelview.
                gl.LoadIdentity();

                //  Move into a more central position.
                gl.Translate(1.5f, 0.0f, -7.0f);

                //  Rotate the cube.
                gl.Rotate(rquad, 1.0f, 1.0f, 1.0f);

                //  Provide the cube colors and geometry.
                gl.Begin(OpenGL.GL_QUADS);

                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Vertex(1.0f, 1.0f, -1.0f);
                gl.Vertex(-1.0f, 1.0f, -1.0f);
                gl.Vertex(-1.0f, 1.0f, 1.0f);
                gl.Vertex(1.0f, 1.0f, 1.0f);

                gl.Color(1.0f, 0.5f, 0.0f);
                gl.Vertex(1.0f, -1.0f, 1.0f);
                gl.Vertex(-1.0f, -1.0f, 1.0f);
                gl.Vertex(-1.0f, -1.0f, -1.0f);
                gl.Vertex(1.0f, -1.0f, -1.0f);

                gl.Color(1.0f, 0.0f, 0.0f);
                gl.Vertex(1.0f, 1.0f, 1.0f);
                gl.Vertex(-1.0f, 1.0f, 1.0f);
                gl.Vertex(-1.0f, -1.0f, 1.0f);
                gl.Vertex(1.0f, -1.0f, 1.0f);

                gl.Color(1.0f, 1.0f, 0.0f);
                gl.Vertex(1.0f, -1.0f, -1.0f);
                gl.Vertex(-1.0f, -1.0f, -1.0f);
                gl.Vertex(-1.0f, 1.0f, -1.0f);
                gl.Vertex(1.0f, 1.0f, -1.0f);

                gl.Color(0.0f, 0.0f, 1.0f);
                gl.Vertex(-1.0f, 1.0f, 1.0f);
                gl.Vertex(-1.0f, 1.0f, -1.0f);
                gl.Vertex(-1.0f, -1.0f, -1.0f);
                gl.Vertex(-1.0f, -1.0f, 1.0f);

                gl.Color(1.0f, 0.0f, 1.0f);
                gl.Vertex(1.0f, 1.0f, -1.0f);
                gl.Vertex(1.0f, 1.0f, 1.0f);
                gl.Vertex(1.0f, -1.0f, 1.0f);
                gl.Vertex(1.0f, -1.0f, -1.0f);

                gl.End();
            }
            gl.Disable(OpenGL.GL_SCISSOR_TEST);
            gl.Disable(OpenGL.GL_DEPTH_TEST);

            //  Flush OpenGL.
            gl.Flush();

            //  Rotate the geometry a bit.
            rotatePyramid += 60.0f * Time.DeltaTime;
            rquad -= 60.0f * Time.DeltaTime;
        }

        internal void SetGraphicsContext(OpenGL context)
        {
            m_glContext = context;

            /*Mesh testMesh = new Mesh(m_glContext);
            Vector3 size = new Vector3(2f, 2f, 2f);

            Vector3[] meshVerts =
            {
                new Vector3(-size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  size.Z / 2f),
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

            testMesh.Vertices = meshVerts;
            testMesh.Indexes = meshIndexes;*/
        }

        internal void SetOutputSize(float width, float height)
        {
            // Re-Calculate perspective camera ratios here.
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                Camera camera = m_cameraList[i];
                camera.PixelWidth = width;
                camera.PixelHeight = height;
            }

            // Load and clear the projection matrix.
            /*context.MatrixMode(OpenGL.GL_PROJECTION);
            context.LoadIdentity();

            // Perform a perspective transformation
            context.Perspective(45.0f, width / height,
                0.1f, 100.0f);

            // Load the modelview.
            context.MatrixMode(OpenGL.GL_MODELVIEW);*/
        }
    }
}

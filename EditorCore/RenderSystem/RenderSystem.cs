using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private OpenGL m_GLContext;
        private List<Camera> m_cameraList;
        private ShaderProgram m_shader;


        float rotatePyramid = 0;
        float rquad = 0; 

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();
            m_shader = new ShaderProgram("RenderSystem/Shaders/vert.glsl", "RenderSystem/Shaders/frag.glsl");

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
            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.DepthTest);
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                /* SETUP THE VIEWPORT FOR THE CAMERA */
                Camera camera = m_cameraList[i];

                Rect pixelRect = camera.PixelRect;
                GL.Viewport((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);
                GL.Scissor((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);

                // Clear the backbuffer
                Color clearColor = camera.ClearColor;
                GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
                GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);


                //  Reset the modelview matrix.
                GL.LoadIdentity();

                //  Move the geometry into a fairly central position.
                GL.Translate(-1.5f, 0.0f, -6.0f);

                //  Draw a pyramid. First, rotate the modelview matrix.
                GL.Rotate(rotatePyramid, 0.0f, 1.0f, 0.0f);

                //  Start drawing trianGLes.
                GL.Begin(BeginMode.Triangles);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(0.0f, 1.0f, 0.0f);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(-1.0f, -1.0f, 1.0f);
                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(1.0f, -1.0f, 1.0f);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(0.0f, 1.0f, 0.0f);
                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(1.0f, -1.0f, 1.0f);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, -1.0f);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(0.0f, 1.0f, 0.0f);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, -1.0f);
                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(-1.0f, -1.0f, -1.0f);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(0.0f, 1.0f, 0.0f);
                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(-1.0f, -1.0f, -1.0f);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(-1.0f, -1.0f, 1.0f);

                GL.End();

                //  Reset the modelview.
                GL.LoadIdentity();

                //  Move into a more central position.
                GL.Translate(1.5f, 0.0f, -7.0f);

                //  Rotate the cube.
                GL.Rotate(rquad, 1.0f, 1.0f, 1.0f);

                //  Provide the cube colors and geometry.
                GL.Begin(BeginMode.Quads);

                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(1.0f, 1.0f, -1.0f);
                GL.Vertex3(-1.0f, 1.0f, -1.0f);
                GL.Vertex3(-1.0f, 1.0f, 1.0f);
                GL.Vertex3(1.0f, 1.0f, 1.0f);

                GL.Color3(1.0f, 0.5f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, 1.0f);
                GL.Vertex3(-1.0f, -1.0f, 1.0f);
                GL.Vertex3(-1.0f, -1.0f, -1.0f);
                GL.Vertex3(1.0f, -1.0f, -1.0f);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(1.0f, 1.0f, 1.0f);
                GL.Vertex3(-1.0f, 1.0f, 1.0f);
                GL.Vertex3(-1.0f, -1.0f, 1.0f);
                GL.Vertex3(1.0f, -1.0f, 1.0f);

                GL.Color3(1.0f, 1.0f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, -1.0f);
                GL.Vertex3(-1.0f, -1.0f, -1.0f);
                GL.Vertex3(-1.0f, 1.0f, -1.0f);
                GL.Vertex3(1.0f, 1.0f, -1.0f);

                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(-1.0f, 1.0f, 1.0f);
                GL.Vertex3(-1.0f, 1.0f, -1.0f);
                GL.Vertex3(-1.0f, -1.0f, -1.0f);
                GL.Vertex3(-1.0f, -1.0f, 1.0f);

                GL.Color3(1.0f, 0.0f, 1.0f);
                GL.Vertex3(1.0f, 1.0f, -1.0f);
                GL.Vertex3(1.0f, 1.0f, 1.0f);
                GL.Vertex3(1.0f, -1.0f, 1.0f);
                GL.Vertex3(1.0f, -1.0f, -1.0f);

                GL.End();
            }
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);

            //  Flush OpenGL.
            GL.Flush();

            //  Rotate the geometry a bit.
            rotatePyramid += 60.0f * Time.DeltaTime;
            rquad -= 60.0f * Time.DeltaTime;
        }

        internal void SetGraphicsContext(OpenGL context)
        {
            m_GLContext = context;

            /*Mesh testMesh = new Mesh(m_GLContext);
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
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            // Perform a perspective transformation
            Matrix4 m_perspective = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f, width / height, 0.1f, 100.0f);
            GL.LoadMatrix(ref m_perspective);

            // Load the modelview.
            GL.MatrixMode(MatrixMode.Modelview);

        }
    }
}

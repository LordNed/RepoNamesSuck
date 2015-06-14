using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private List<Camera> m_cameraList;
        private ShaderProgram m_shader;
        private Mesh m_testMesh;

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();
            m_shader = new ShaderProgram("RenderSystem/Shaders/vert.glsl", "RenderSystem/Shaders/frag.glsl");

            // Create a Default camera
            //Camera editorCamera = new Camera();
            //m_cameraList.Add(editorCamera);

            Camera leftCamera = new Camera();
            leftCamera.ClearColor = new Color(1f, 0.5f, 0, 1f);

            Camera rightCamera = new Camera();
            rightCamera.ViewportRect = new Rect(0.5f, 0f, 0.5f, 1f);
            rightCamera.ClearColor = new Color(0.5f, 0, 1f, 1f);

            m_cameraList.Add(leftCamera);
            //m_cameraList.Add(rightCamera);

            /* Create a default cube */
            m_testMesh = new Mesh();
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

            m_testMesh.Vertices = meshVerts;
            m_testMesh.Indexes = meshIndexes;
        }

        internal void RenderFrame()
        {
            // Solid Fill the Back Buffer, until I can figure out what's going on with resizing
            // windows and partial camera viewport rects.
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

                // Start using our Shader program
                GL.UseProgram(m_shader.ProgramId);

                GL.Enable(EnableCap.DepthTest);

                Matrix4 viewMatrix = Matrix4.LookAt(new Vector3(25, 15, 25), Vector3.Zero, Vector3.UnitY);
                Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), 1.6f, 10, 1000);
                Matrix4 modelMatrix = Matrix4.Identity; //Identity = doesn't change anything when multiplied.

                Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
                Matrix4 finalMatrix = modelMatrix * viewProjMatrix;



                // Bind the Mesh objects
                GL.BindBuffer(BufferTarget.ArrayBuffer, m_testMesh.VBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_testMesh.EBO);

                // Set the layout and enable attributes.
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

                // Upload the MVP to the GPU
                //GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);
                //GL.Uniform3(m_shader.UniformColor, new Vector3(1f, 0f, 0f));

                //Matrix4 finalMatrix = modelMatrix * viewMatrix * projMatrix;
                GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);

                // Draw it
                GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            }
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);

            //  Flush OpenGL.
            GL.Flush();
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

        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            //Gets an id from OpenGL
            address = GL.CreateShader(type);
            using (var streamReader = new StreamReader(fileName))
            {
                GL.ShaderSource(address, streamReader.ReadToEnd());
            }
            //Compiles the shader code
            GL.CompileShader(address);
            //Tells OpenGL that this shader (be it vertex of fragment) belongs to the specified program
            GL.AttachShader(program, address);

            //Error checking.
            int compileSuccess;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compileSuccess);

            if (compileSuccess == 0)
                Console.WriteLine(GL.GetShaderInfoLog(address));
        }
    }
}

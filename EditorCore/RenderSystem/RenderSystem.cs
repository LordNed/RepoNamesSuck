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
        private OpenGL m_GLContext;
        private List<Camera> m_cameraList;
        private ShaderProgram m_shader;
        private Mesh m_testMesh;

        //The OpenGL "program" 's id. You use this to tell the GPU which Vertex/Fragment shader to use.
        private int _programId;

        //This is an id to a "Uniform". A uniform is set each frame and is constant for the entire drawcall.
        private int _uniformMVP;

        //This is an identifier that points to a specific buffer in GPU memory. You'd realistically need one of them
        //per object you're drawing, but I'm only drawing one atm so meh.
        private int _glVbo;
        private int _glEbo;

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();
            m_shader = new ShaderProgram("RenderSystem/Shaders/vert.glsl", "RenderSystem/Shaders/frag.glsl");
            _programId = GL.CreateProgram();

            //Create the Vertex and Fragment shader from file using our helper function
            int vertShaderId, fragShaderId;
            LoadShader("RenderSystem/Shaders/vert.glsl", ShaderType.VertexShader, _programId, out vertShaderId);
            LoadShader("RenderSystem/Shaders/frag.glsl", ShaderType.FragmentShader, _programId, out fragShaderId);

            GL.BindAttribLocation(_programId, (int)ShaderAttributeIds.Position, "vertexPos");
            GL.LinkProgram(_programId);
            _uniformMVP = GL.GetUniformLocation(_programId, "modelview");
            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(_programId));

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
            //m_testMesh = new Mesh();
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

            GL.GenBuffers(1, out _glVbo);

            //This "binds" the buffer. Once a buffer is bound, all actions are relative to it until another buffer is bound.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glVbo);

            //This uploads data to the currently bound buffer from the CPU -> GPU. This only needs to be done with the data changes (ie: you edited a vertexes position on the cpu side)
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(meshVerts.Length * Vector3.SizeInBytes), meshVerts,
                BufferUsageHint.StaticDraw);

            //Now we're going to repeat the same process for the Element buffer, which is what OpenGL calls indicies. (Notice how it's basically identical?)
            GL.GenBuffers(1, out _glEbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _glEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(meshIndexes.Length * 4), meshIndexes,
                BufferUsageHint.StaticDraw);
        }

        private void Draw()
        {
            //This is called *every* frame. Every time we want to draw, we do the following.
            GL.ClearColor(0f, 0f, 1f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Tell OpenGL which program to use (our Vert Shader (VS) and Frag Shader (FS))
            GL.UseProgram(_programId);

            //Enable depth-testing which keeps models from rendering inside out.
            GL.Enable(EnableCap.DepthTest);

            //Clear any previously bound buffer so we have no leftover data or anything.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            /* Anything below this point would technically be done per object you draw */

            //Build a Model View Projection Matrix. This is where you would add camera movement (modifiying the View matrix), Perspective rendering (perspective matrix) and model position/scale/rotation (Model)
            Matrix4 viewMatrix = Matrix4.LookAt(new Vector3(25, 15, 25), Vector3.Zero, Vector3.UnitY);
            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), 1.6f, 10, 1000);
            Matrix4 modelMatrix = Matrix4.Identity; //Identity = doesn't change anything when multiplied.

            //Bind the buffers that have the data you want to use
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glVbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _glEbo);

            //Then, you have to tell the GPU what the contents of the Array buffer look like. Ie: Is each entry just a position, or does it have a position, color, normal, etc.
            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            //Upload the WVP to the GPU
            Matrix4 finalMatrix = modelMatrix * viewMatrix * projMatrix;
            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            //Now we tell the GPU to actually draw the data we have
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            //This is cleanup to undo the changes to the OpenGL state we did to draw this model.
            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
        }

        internal void RenderFrame()
        {
            //Draw();
            //return;
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
                GL.UseProgram(_programId);

                GL.Enable(EnableCap.DepthTest);

                Matrix4 viewMatrix = Matrix4.LookAt(new Vector3(25, 15, 25), Vector3.Zero, Vector3.UnitY);
                Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), 1.6f, 10, 1000);
                Matrix4 modelMatrix = Matrix4.Identity; //Identity = doesn't change anything when multiplied.

                Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
                Matrix4 finalMatrix = modelMatrix * viewProjMatrix;



                // Bind the Mesh objects
                GL.BindBuffer(BufferTarget.ArrayBuffer, _glVbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _glEbo);

                // Set the layout and enable attributes.
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

                // Upload the MVP to the GPU
                //GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);
                //GL.Uniform3(m_shader.UniformColor, new Vector3(1f, 0f, 0f));

                //Matrix4 finalMatrix = modelMatrix * viewMatrix * projMatrix;
                GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

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

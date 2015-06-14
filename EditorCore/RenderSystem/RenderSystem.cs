using SharpGL;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private OpenGL m_glContext;
        private List<Camera> m_cameraList;

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


        float rotatePyramid = 0;
        float rquad = 0; 

        internal void RenderFrame()
        {
            // Update the internal editor core state.
            //  Get the OpenGL instance that's been passed to us.
            OpenGL gl = m_glContext;

            gl.Enable(OpenGL.GL_SCISSOR_TEST);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            for (int i = 0; i < m_cameraList.Count; i++)
            {
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

            //  Enable the OpenGL depth testing functionality.
            context.Enable(OpenGL.GL_DEPTH_TEST);
        }

        internal void SetOutputSize(OpenGL context, float width, float height)
        {
            // Re-Calculate perspective camera ratios here.
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                Camera camera = m_cameraList[i];
                camera.PixelWidth = width;
                camera.PixelHeight = height;
            }

            // Load and clear the projection matrix.
            context.MatrixMode(OpenGL.GL_PROJECTION);
            context.LoadIdentity();

            // Perform a perspective transformation
            context.Perspective(45.0f, width / height,
                0.1f, 100.0f);

            // Load the modelview.
            context.MatrixMode(OpenGL.GL_MODELVIEW);
        }
    }
}

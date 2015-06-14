using SharpGL;
using System;
using System.Diagnostics;
using WEditor.Rendering;

namespace WEditor
{
    public class EditorCore
    {
        /// Sub-Systems that run various parts of the Editor 
        private RenderSystem m_renderSystem;

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;


        public EditorCore()
        {
            m_renderSystem = new RenderSystem();

            m_dtStopwatch = new Stopwatch();
            Console.WriteLine("[EditorCore] Initialized.");
        }


        private OpenGL m_context;

        public void Tick()
        {
            Time.Internal_UpdateTime(m_dtStopwatch.ElapsedMilliseconds / 1000f);
            m_dtStopwatch.Restart();

            m_renderSystem.RenderFrame();

           
        }

        public void SetGraphicsContext(OpenGL context)
        {
            m_context = context;
            m_renderSystem.SetGraphicsContext(context);
        }

        public void OnOutputResized(float width, float height)
        {
            m_renderSystem.SetOutputSize(width, height);
        }
    }
}

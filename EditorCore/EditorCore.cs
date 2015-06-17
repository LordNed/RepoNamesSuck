using System;
using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Rendering;

namespace WEditor
{
    public class EditorCore
    {
        public static EditorCore HackyInstance;

        /// Sub-Systems that run various parts of the Editor 
        private RenderSystem m_renderSystem;

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;

        public List<Component> HackyComponents;

        public EditorCore()
        {
            HackyInstance = this;
            m_dtStopwatch = new Stopwatch();
            HackyComponents = new List<Component>();
            Console.WriteLine("[EditorCore] Initialized.");
        }

        public void Tick()
        {
            // Calculate a new deltaTime for this frame (amount of time it took the last frame to render)
            Time.Internal_UpdateTime(m_dtStopwatch.ElapsedMilliseconds / 1000f);
            m_dtStopwatch.Restart();

            // Update the internal input state so can compare the results of the previous frame.
            Input.Internal_UpdateInputState();

            for (int i = 0; i < HackyComponents.Count; i++)
            {
                HackyComponents[i].Update();
            }

            m_renderSystem.RenderFrame();
        }

        public void OnGraphicsContextInitialized()
        {
            Console.WriteLine("[EditorCore] Initializing RenderSystem.");
            m_renderSystem = new RenderSystem();
            Console.WriteLine("[EditorCore] RenderSystem Initialized.");
        }

        public void OnOutputResized(float width, float height)
        {
            if (m_renderSystem == null)
                return;

            m_renderSystem.SetOutputSize(width, height);
        }
    }
}

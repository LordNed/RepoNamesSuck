using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Rendering;

namespace WEditor
{
    public class WWorld
    {
        /// <summary> Handles rendering for objects associated with this WWorld. </summary>
        public RenderSystem RenderSystem
        {
            get { return m_renderSystem; }
        }

        public Input Input
        {
            get { return m_input; }
        }

        public string Name
        {
            get { return m_worldName; }
        }

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;

        /// <summary> List of components in the WWorld which need to recieve update ticks. </summary>
        private List<WComponent> m_componentList;

        private RenderSystem m_renderSystem;

        private Input m_input;
        private string m_worldName;

        public WWorld(string worldName)
        {
            m_worldName = worldName;
            m_componentList = new List<WComponent>();
            m_dtStopwatch = new Stopwatch();
            m_input = new Input();

            m_renderSystem = new RenderSystem(this);
        }

        public void InitializeSystem()
        {
            m_renderSystem.InitializeSystem();
        }

        public void ShutdownSystem()
        {
            m_renderSystem.ShutdownSystem();
        }

        public void Tick()
        {
            float deltaTime = m_dtStopwatch.ElapsedMilliseconds / 1000f;
            m_dtStopwatch.Restart();

            // Update all of the entities in this world so they can move around, etc.
            foreach(WComponent component in m_componentList)
            {
                component.Tick(deltaTime);
            }

            // Update the render system this frame and draw everything.
            m_renderSystem.RenderFrame();

            // Update the internal input state so that we can compare against the previous state, ie: for first press, etc.
            m_input.Internal_UpdateInputState();
        }

        public void RegisterComponent(WComponent component)
        {
            component.World = this;
            component.Input = m_input;
            m_componentList.Add(component);
        }

        public void UnregisterComponent(WComponent component)
        {
            m_componentList.Remove(component);
        }


        public void UnloadAll()
        {
            m_componentList.Clear();

            RenderSystem.UnloadAll();
        }
    }
}

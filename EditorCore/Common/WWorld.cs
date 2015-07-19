using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Rendering;
using WEditor.WindWaker;

namespace WEditor
{
    public class WWorld
    {
        /// <summary> Handles rendering for objects associated with this WWorld. </summary>
        public RenderSystem RenderSystem
        {
            get { return m_renderSystem; }
        }

        public DebugDrawing Gizmos
        {
            get { return m_gizmos; }
        }

        public Input Input
        {
            get { return m_input; }
        }

        public string Name
        {
            get { return m_worldName; }
        }

        public Map Map { get; set; }

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;

        /// <summary> List of components in the WWorld which need to recieve update ticks. </summary>
        private List<WComponent> m_componentList;
        private List<WObject> m_objectList;

        private RenderSystem m_renderSystem;
        private DebugDrawing m_gizmos;

        private Input m_input;
        private string m_worldName;

        public WWorld(string worldName)
        {
            m_worldName = worldName;
            m_componentList = new List<WComponent>();
            m_objectList = new List<WObject>();
            m_dtStopwatch = new Stopwatch();
            m_input = new Input();

            m_gizmos = new DebugDrawing();
            m_renderSystem = new RenderSystem(this);
        }

        public void InitializeSystem()
        {
            m_renderSystem.InitializeSystem();
            m_gizmos.InitializeSystem();
        }

        public void ShutdownSystem()
        {
            m_renderSystem.ShutdownSystem();
            m_gizmos.ShutdownSystem();
        }

        public void Tick()
        {
            float deltaTime = m_dtStopwatch.ElapsedMilliseconds / 1000f;
            m_dtStopwatch.Restart();
            m_gizmos.ResetList();

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

        public void RegisterObject(WObject obj)
        {
            obj.World = this;
            m_objectList.Add(obj);
        }

        public void UnregisterObject(WObject obj)
        {
            m_objectList.Remove(obj);
        }

        public void UnloadWorld()
        {
            m_componentList.Clear();
            m_objectList.Clear();
            m_gizmos.ResetList();
            RenderSystem.UnloadAll();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WEditor.Rendering;
using WEditor.WindWaker;
using WEditor.WindWaker.Loaders;

namespace WEditor
{
    public class EditorCore : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Used to print all log messages to the Console for now. </summary>
        private StandardOutLogger m_stdOutLogger;

        private List<WWorld> m_editorWorlds;
        private WWorld m_mainWorld;

        public Map LoadedScene
        {
            get { return m_loadedScene; }
            set
            {
                m_loadedScene = value;
                OnPropertyChanged("LoadedScene");
            }
        }

        private Map m_loadedScene;

        public EditorCore()
        {
            m_stdOutLogger = new StandardOutLogger();
            m_editorWorlds = new List<WWorld>();
            m_mainWorld = new WWorld("main");
            m_editorWorlds.Add(m_mainWorld);

            m_mainWorld.InitializeSystem();

            WLog.Info(LogCategory.EditorCore, null, "Initialized.");
        }

        public void Shutdown()
        {
            UnloadMap();

            for (int i = 0; i < m_editorWorlds.Count; i++)
                m_editorWorlds[i].ShutdownSystem();
        }

        public void Tick()
        {
            foreach (WWorld world in m_editorWorlds)
            {
                world.Tick();
            }
        }

        public WWorld GetWorldByName(string worldName)
        {
            // Find the right world for this output
            foreach (WWorld world in m_editorWorlds)
            {
                if (string.Compare(world.Name, worldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return world;
                }
            }

            WLog.Warning(LogCategory.Rendering, null, "Recieved GetWorldByName for world {0}, but no world of that name exists. Ignoring.", worldName);
            return null;
        }


        public void LoadMapFromDirectory(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("You must specify a folder to load from directory!");

            if (LoadedScene != null)
                throw new InvalidOperationException("There is already a map loaded, call UnloadMap() first!");

            MapLoader mapLoader = new MapLoader();
            Map newMap = null;
#if DEBUG
            newMap = mapLoader.CreateFromDirectory(m_mainWorld, folderPath);
#else
            try
            {
                newMap = mapLoader.CreateFromDirectory(m_mainWorld, folderPath);
            }
            catch(Exception ex)
            {
                WLog.Error(LogCategory.EditorCore, null, "Exception while loading map: " + ex.ToString());
            }
#endif

            LoadedScene = newMap;
        }

        public void UnloadMap()
        {
            // We're going to clear the contents of each currently loaded world. This means freeing GPU resources,
            // unloading archives, etc, etc. 
            foreach (WWorld world in m_editorWorlds)
            {
                world.UnloadWorld();
            }

            LoadedScene = null;

            // Force a GC collect so we're sure everything got disposed and can actually test against it.
            GC.Collect();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

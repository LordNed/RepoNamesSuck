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

        public static EditorCore HackyInstance;

        /// Sub-Systems that run various parts of the Editor 
        private RenderSystem m_renderSystem;

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;

        /// <summary> Used to print all log messages to the Console for now. </summary>
        private StandardOutLogger m_stdOutLogger;

        public List<Component> HackyComponents;

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

            HackyInstance = this;
            m_dtStopwatch = new Stopwatch();
            HackyComponents = new List<Component>();
            WLog.Info(LogCategory.EditorCore, null, "Initialized.");
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
            WLog.Info(LogCategory.EditorCore, null, "Initializing RenderSystem.");
            m_renderSystem = new RenderSystem();
            WLog.Info(LogCategory.EditorCore, null, "[EditorCore] RenderSystem Initialized.");
        }

        public void OnOutputResized(float width, float height)
        {
            if (m_renderSystem == null)
                return;

            m_renderSystem.SetOutputSize(width, height);
        }

        public void LoadMapFromDirectory(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("You must specify a folder to load from directory!");

            LoadedScene = MapLoader.Load(folderPath);
        }


        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UnloadMap()
        {
            throw new NotImplementedException();
        }
    }
}

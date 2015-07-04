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
            m_editorWorlds.Add(new WWorld("main"));
            WLog.Info(LogCategory.EditorCore, null, "Initialized.");
        }

        public void Tick()
        {
            foreach(WWorld world in m_editorWorlds)
            {
                world.Tick();
            }
        }

        public void OnOutputResized(string worldName, float width, float height)
        {
            // Find the right world for this output
            foreach(WWorld world in m_editorWorlds)
            {
                if(string.Compare(world.Name, worldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    world.RenderSystem.SetOutputSize(width, height);
                    return;
                }
            }

            WLog.Warning(LogCategory.Rendering, null, "Recieved Display Resize event for world {0}, but no world of that name exists. Ignoring.", worldName);
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

        public void SetMouseState(string worldName, System.Windows.Input.MouseButton mouseButton, bool down)
        {
            // Find the right world for this output
            foreach (WWorld world in m_editorWorlds)
            {
                if (string.Compare(world.Name, worldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    world.Input.SetMouseState(mouseButton, down);
                    return;
                }
            }

            WLog.Warning(LogCategory.Rendering, null, "Recieved SetMouseState event for world {0}, but no world of that name exists. Ignoring.", worldName);
        }

        public void SetKeyboardState(string worldName, System.Windows.Input.Key key, bool down)
        {
            // Find the right world for this output
            foreach (WWorld world in m_editorWorlds)
            {
                if (string.Compare(world.Name, worldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    world.Input.SetkeyboardState(key, down);
                    return;
                }
            }

            WLog.Warning(LogCategory.Rendering, null, "Recieved SetKeyboardState event for world {0}, but no world of that name exists. Ignoring.", worldName);
        }

        public void SetMousePosition(string worldName, OpenTK.Vector2 position)
        {
            // Find the right world for this output
            foreach (WWorld world in m_editorWorlds)
            {
                if (string.Compare(world.Name, worldName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    world.Input.SetMousePosition(position);
                    return;
                }
            }

            WLog.Warning(LogCategory.Rendering, null, "Recieved SetMousePosition event for world {0}, but no world of that name exists. Ignoring.", worldName);
        }
    }
}

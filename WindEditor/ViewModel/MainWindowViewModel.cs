using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using WEditor;
using OpenTK;
using System.ComponentModel;
using WEditor.WindWaker;
using WEditor.WindWaker.MapEntities;

namespace WindEditor.UI
{
    /// <summary>
    /// This is constructed automatically by the MainWindow view and is bound to the MainWindow's data context.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanSave { get { return false; /* return m_editorCore.LoadedScene != null;*/ } }
        public bool CanClose { get { return m_editorCore.LoadedScene != null; } }
        public bool CanUndo { get { return false; } }
        public bool CanRedo { get { return false; } }

        public string GCMemAmount
        {
            get { return string.Format("GC Heap Size: {0:N2}mb", (GC.GetTotalMemory(false)/1024/1024f)); }
        }

        public SceneViewViewModel SceneView { get; private set; }
        public EntityOutlinerViewModel EntityOutliner { get; private set; }
        public OutputLogViewModel OutputLog { get; private set; }

        private EditorCore m_editorCore;
        private System.Windows.Forms.Timer m_intervalTimer;
        private GLControl m_control;

        public MainWindowViewModel()
        {
            SceneView = new SceneViewViewModel(this);
            EntityOutliner = new EntityOutlinerViewModel(this);
            OutputLog = new OutputLogViewModel();
        }

        void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoadedScene")
            {
                SceneView.SetMap(m_editorCore.LoadedScene);
            }
        }

        internal void OnGraphicsContextInitialized(GLControl context)
        {
            m_control = context;

            m_editorCore = new EditorCore();
            m_intervalTimer = new System.Windows.Forms.Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            {
                var newMousePosition = System.Windows.Forms.Control.MousePosition;
                m_editorCore.SetMousePosition("main", new OpenTK.Vector2((float)newMousePosition.X, (float)newMousePosition.Y));
                m_editorCore.Tick();
                if (m_control != null)
                    m_control.SwapBuffers();

                OnPropertyChanged("GCMemAmount");
            };

            m_editorCore.PropertyChanged += OnEditorPropertyChanged;

            m_editorCore.LoadMapFromDirectory(@"C:\Users\Matt\Documents\Wind Editor\ma2room_slim");
        }

        internal void OnOutputResized(float width, float height)
        {
            m_editorCore.OnOutputResized("main", width, height);
        }

        internal void SetMouseState(MouseButton mouseButton, bool down)
        {
            m_editorCore.SetMouseState("main", mouseButton, down);
        }

        internal void SetKeyboardState(Key key, bool down)
        {
            m_editorCore.SetKeyboardState("main", key, down);
        }

        internal void Exit()
        {
            if (m_editorCore.LoadedScene != null)
            {
                if (System.Windows.MessageBox.Show("Are you sure to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        internal void Open()
        {
            var ofd = new CommonOpenFileDialog();
            ofd.Title = "Choose Directory";
            ofd.IsFolderPicker = true;
            ofd.AddToMostRecentlyUsedList = false;
            ofd.AllowNonFileSystemItems = false;
            ofd.EnsureFileExists = true;
            ofd.EnsurePathExists = true;
            ofd.EnsureReadOnly = false;
            ofd.EnsureValidNames = true;
            ofd.Multiselect = false;
            ofd.ShowPlacesList = true;

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Just assume the folder paths are valid now.
                var folderPath = ofd.FileName;

                m_editorCore.LoadMapFromDirectory(folderPath);
            }
        }

        internal void Save()
        {
            throw new NotImplementedException();
        }

        internal void Close()
        {
            m_editorCore.UnloadMap();
        }

        internal void Undo()
        {
            WLog.Info(LogCategory.UI, null, "Undo (Not Implemented)");
        }

        internal void Redo()
        {
            WLog.Info(LogCategory.UI, null, "Redo (Not Implemented)");
        }

        internal void SetSelectedEntityFile(MapEntityResource entityFile)
        {
            if (entityFile != null)
            {
                EntityOutliner.EntityList = entityFile.Objects;
            }
            else
            {
                EntityOutliner.EntityList = null;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

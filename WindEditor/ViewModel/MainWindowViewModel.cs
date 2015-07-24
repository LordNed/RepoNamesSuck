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
using WEditor.Maps;
using SelectedItemsBindingDemo;
using System.Windows.Forms.Integration;

namespace WindEditor.UI
{
    /// <summary>
    /// This is constructed automatically by the MainWindow view and is bound to the MainWindow's data context.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Command Callbacks
        /// <summary> The user has requested to open a new map, ask which map and then unload current if needed. </summary>
        public ICommand OnRequestMapOpen
        {
            get { return new RelayCommand(x => Open()); }
        }

        /// <summary> The user has requested to save the currently open map. Only available if a map is currently opened. </summary>
        public ICommand OnRequestMapSave
        {
            get { return new RelayCommand(x => Save(), x => m_editorCore.LoadedScene != null); }
        }

        /// <summary> The user has requested to unload the currently open map. Only available if a map is currently opened. Ask user if they'd like to save. </summary>
        public ICommand OnRequestMapClose
        {
            get { return new RelayCommand(x => Close(), x => m_editorCore.LoadedScene != null); }
        }

        /// <summary> The user has requested to undo the last action. Only available if they've made an undoable action. </summary>
        public ICommand OnRequestUndo
        {
            get { return new RelayCommand(x => { return; }, x => false); }
        }

        /// <summary> The user has requested to redo the last undo action. Only available if they've undone an action. </summary>
        public ICommand OnRequestRedo
        {
            get { return new RelayCommand(x => { return; }, x => false); }
        }

        /// <summary> Delete the currently selected objects in the world. Only available if there is a one or more currently selected objects. </summary>
        public ICommand OnRequestDelete
        {
            get { return new RelayCommand(x => m_editorCore.GetWorldByName("main").DeleteSelectedObjects(), x => m_editorCore.GetWorldByName("main").SelectedEntities.Count > 0); }
        }

        public ICommand OnRequestApplicationClose
        {
            get { return new RelayCommand(x => Application.Current.MainWindow.Close()); }
        }
        #endregion

        public SceneViewViewModel SceneView { get; private set; }
        public EntityOutlinerViewModel EntityOutliner { get; private set; }
        public OutputLogViewModel OutputLog { get; private set; }
        public InspectorViewModel InspectorView { get; private set; }

        public string WindowTitle
        {
            get { return m_windowTitle; }
            private set
            {
                m_windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        public Map LoadedScene
        {
            get
            {
                if (m_editorCore != null)
                    return m_editorCore.LoadedScene;

                return null;
            }
        }

        private EditorCore m_editorCore;
        private System.Windows.Forms.Timer m_intervalTimer;
        private GLControl m_control;
        private string m_windowTitle;

        public MainWindowViewModel()
        {
            SceneView = new SceneViewViewModel(this);
            EntityOutliner = new EntityOutlinerViewModel(this);
            OutputLog = new OutputLogViewModel();
            InspectorView = new InspectorViewModel();
            UpdateWindowTitle();
        }

        void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoadedScene")
            {
                SceneView.SetMap(m_editorCore.LoadedScene);
                UpdateWindowTitle();

                OnPropertyChanged("LoadedScene");
            }
        }

        internal void OnGraphicsContextInitialized(GLControl context, WindowsFormsHost host)
        {
            m_control = context;

            m_editorCore = new EditorCore();
            m_intervalTimer = new System.Windows.Forms.Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            {
                Vector2 mousePosGlobal = new Vector2(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
                Vector2 glControlPosGlobal = new Vector2((float)host.PointToScreen(new Point(0, 0)).X, (float)host.PointToScreen(new Point(0, 0)).Y);

                var delta = mousePosGlobal - glControlPosGlobal;

                delta.X = MathE.Clamp(delta.X, 0, m_control.Width);
                delta.Y = MathE.Clamp(delta.Y, 0, m_control.Height);

                m_editorCore.GetWorldByName("main").Input.SetMousePosition(delta);
                m_editorCore.Tick();

                if (m_control != null)
                    m_control.SwapBuffers();
            };

            m_editorCore.PropertyChanged += OnEditorPropertyChanged;
            EntityOutliner.m_world = m_editorCore.GetWorldByName("main");
        }

        internal void OnOutputResized(float width, float height)
        {
            m_editorCore.GetWorldByName("main").RenderSystem.SetOutputSize(width, height);
        }

        internal void SetMouseState(MouseButton mouseButton, bool down)
        {
            m_editorCore.GetWorldByName("main").Input.SetMouseState(mouseButton, down);
        }

        internal void SetKeyboardState(Key key, bool down)
        {
            m_editorCore.GetWorldByName("main").Input.SetKeyboardState(key, down);
        }

        internal void SetMouseScrollDelta(int delta)
        {
            m_editorCore.GetWorldByName("main").Input.SetMouseScrollDelta(delta);
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

                m_editorCore.UnloadMap();
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

        internal void SetSelectedSceneFile(Scene sceneFile)
        {
            if (sceneFile != null)
            {
                EntityOutliner.EntityList = sceneFile.Entities;
            }
            else
            {
                EntityOutliner.EntityList = null;
            }
        }

        internal void SetSelectedEntity(MapEntity newEntity)
        {
            InspectorView.SelectedEntity = newEntity;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Confirm exit.
            if (m_editorCore.LoadedScene != null)
            {
                if (System.Windows.MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            m_editorCore.Shutdown();
        }

        private void UpdateWindowTitle()
        {
            if (m_editorCore == null || m_editorCore.LoadedScene == null)
            {
                WindowTitle = "Wind Editor";
            }
            else
            {
                WindowTitle = string.Format("{0} - Wind Editor", m_editorCore.LoadedScene.Name);
            }
        }
    }
}

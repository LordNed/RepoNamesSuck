using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WEditor;

namespace WindEditor.UI
{
    /// <summary>
    /// This is constructed automatically by the MainWindow view and is bound to the MainWindow's data context.
    /// </summary>
    public class MainWindowViewModel
    {
        public bool CanSave { get { return m_editorCore.LoadedScene != null; } }
        public bool CanClose { get { return m_editorCore.LoadedScene != null; } }
        public bool CanUndo { get { return false; } }
        public bool CanRedo { get { return false; } }

        private EditorCore m_editorCore;

        public MainWindowViewModel()
        {
            m_editorCore = new EditorCore();
        }

        internal void Exit()
        {
            if(m_editorCore.LoadedScene != null)
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
            throw new NotImplementedException();
        }


        internal void Save()
        {
            throw new NotImplementedException();
        }

        internal void Close()
        {
            throw new NotImplementedException();
        }


        internal void Undo()
        {
            WLog.Info(LogCategory.UI, null, "Undo (Not Implemented)");
        }

        internal void Redo()
        {
            WLog.Info(LogCategory.UI, null, "Redo (Not Implemented)");
        }
    }
}

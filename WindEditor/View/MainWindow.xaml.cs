using OpenTK;
using OpenTK.Graphics;
using System;
using System.Windows;
using System.Windows.Input;
using WEditor.UI.View;
using WindEditor.UI;

namespace WindEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel m_viewModel;
        private GLControl m_glControl;

        public MainWindow()
        {
            m_viewModel = new MainWindowViewModel();
            InitializeComponent();
            DataContext = m_viewModel;
        }

        private void OnExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Exit();
        }

        private void OnOpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Open();
        }

        private void OnSaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanSave;
        }

        private void OnSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Save();
        }

        private void OnCloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanClose;
        }

        private void OnCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Close();
        }

        private void OnUndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanUndo;
        }

        private void OnUndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Undo();
        }

        private void OnRedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanRedo;
        }

        private void OnRedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Redo();
        }

        private void winFormsHost_Initialized(object sender, EventArgs e)
        {
            m_glControl = new GLControl(new GraphicsMode(32, 24), 3, 0, GraphicsContextFlags.Default);
            m_glControl.MakeCurrent();
            m_glControl.Paint += m_glControl_Paint;
            m_glControl.MouseDown += m_glControl_MouseDown;
            m_glControl.MouseUp += m_glControl_MouseUp;
            m_glControl.MouseWheel += m_glControl_MouseWheel;
            m_glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            m_viewModel.OnGraphicsContextInitialized(m_glControl);

            winFormsHost.Child = m_glControl;
        }

        void m_glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_viewModel.SetMouseScrollDelta(e.Delta);
        }

        private void m_glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // This dummy stub is needed otherwise it never displays anything, even though we're
            // manually calling swap buffers. It's a bit of a hack okay.
        }

        void m_glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseButton btn = MouseButton.Left;
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    btn = MouseButton.Left;
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    btn = MouseButton.Middle;
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    btn = MouseButton.Right;
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    btn = MouseButton.XButton1;
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    btn = MouseButton.XButton2;
                    break;
            }

            m_viewModel.SetMouseState(btn, false);
        }

        void m_glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseButton btn = MouseButton.Left;
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    btn = MouseButton.Left;
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    btn = MouseButton.Middle;
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    btn = MouseButton.Right;
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    btn = MouseButton.XButton1;
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    btn = MouseButton.XButton2;
                    break;
            }

            m_viewModel.SetMouseState(btn, true);
        }

        private void winFormsHost_LayoutUpdated(object sender, EventArgs e)
        {
            m_viewModel.OnOutputResized((float)m_glControl.Width, (float)m_glControl.Height);
        }

        private void winFormsHost_KeyDown(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState(e.Key, true);
        }

        private void winFormsHost_KeyUp(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState(e.Key, false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_viewModel.OnWindowClosing(sender, e);
        }

        private void OnAboutWindowClicked(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnDeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanDelete;
        }

        private void OnDeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Delete();
        }
    }
}

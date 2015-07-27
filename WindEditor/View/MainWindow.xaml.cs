using OpenTK;
using OpenTK.Graphics;
using System;
using System.Windows;
using System.Windows.Input;
using WEditor.WindWaker.Entities;
using WindEditor.UI.View;
using WindEditor.UI.ViewModel;

namespace WindEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel m_viewModel;
        private GLControl m_glControl;

        private bool m_isDragDropHovering;
        private System.Windows.Forms.IDataObject m_dragDropData;


        public MainWindow()
        {
            m_viewModel = new MainWindowViewModel();
            InitializeComponent();
            DataContext = m_viewModel;
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
            m_glControl.AllowDrop = true;
            m_glControl.DragEnter += m_glControl_DragEnter;
            m_glControl.DragLeave += m_glControl_DragLeave;
            m_glControl.BackColor = System.Drawing.Color.Black;
            m_viewModel.OnGraphicsContextInitialized(m_glControl, winFormsHost);

            winFormsHost.Child = m_glControl;
            winFormsHost.AllowDrop = true;
        }

        public void Tick()
        {
            if (Mouse.LeftButton == MouseButtonState.Released && m_isDragDropHovering)
            {
                m_isDragDropHovering = false;

                MapObjectSpawnDescriptor dragData = m_dragDropData.GetData("actorSpawnData") as MapObjectSpawnDescriptor;
                if (dragData == null)
                    return;

                m_viewModel.OnDragAndDrop(dragData);
            }
        }

        void m_glControl_DragLeave(object sender, EventArgs e)
        {
            m_isDragDropHovering = false;
        }

        void m_glControl_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            m_isDragDropHovering = true;
            m_dragDropData = e.Data;
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
    }
}

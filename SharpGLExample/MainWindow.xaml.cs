using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms.Integration;

namespace SharpGLExample
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

        private void winFormsHost_Initialized(object sender, EventArgs e)
        {
            m_glControl = new GLControl(new GraphicsMode(32, 24), 3, 0, GraphicsContextFlags.Default);
            m_glControl.MakeCurrent();
            m_glControl.Paint += m_glControl_Paint;
            m_glControl.MouseDown += m_glControl_MouseDown;
            m_glControl.MouseUp += m_glControl_MouseUp;
            m_glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            m_viewModel.OnGraphicsContextInitialized(m_glControl);

            winFormsHost.Child = m_glControl;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            m_viewModel.MousePosition = Mouse.GetPosition(winFormsHost);
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

        void m_glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            /*GL.ClearColor(Color4.Red);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Flush();
            m_glControl.SwapBuffers();*/
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
    }
}

using SharpGL;
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

namespace SharpGLExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel m_viewModel;

        public MainWindow()
        {
            InitializeComponent();
            m_viewModel = new MainWindowViewModel();
            DataContext = m_viewModel;
        }

        private void OpenGLControl_OpenGLInitialized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            m_viewModel.OnGraphicsContextInitialized(args.OpenGL);
        }

        private void OpenGLControl_Resized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            m_viewModel.OnOutputResized(gl, (float)gl.RenderContextProvider.Width, (float)gl.RenderContextProvider.Height);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            m_viewModel.MousePosition = Mouse.GetPosition(glControl);
        }

        private void glControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_viewModel.SetMouseState(e.ChangedButton, true);
        }

        private void glControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_viewModel.SetMouseState(e.ChangedButton, false);
        }

        private void glControl_KeyDown(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState(e.Key, true);
        }

        private void glControl_KeyUp(object sender, KeyEventArgs e)
        {
            m_viewModel.SetKeyboardState(e.Key, false);
        }

        private void glControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            // Only exists as otherwise the WPF form never seems to present.
        }
    }
}

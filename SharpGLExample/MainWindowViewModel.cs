using SharpGL;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WEditor;

namespace SharpGLExample
{
    public class MainWindowViewModel
    {
        public Point MousePosition {get; set;}

        private EditorCore m_editorCore;
        private Timer m_intervalTimer;



        public MainWindowViewModel()
        {
            m_editorCore = new EditorCore();
            m_intervalTimer = new Timer();
            m_intervalTimer.Interval = 16; // 60 FPS roughly
            m_intervalTimer.Enabled = true;
            m_intervalTimer.Tick += (args, o) =>
            { 
                Input.SetMousePosition(new OpenTK.Vector2((float)MousePosition.X, (float)MousePosition.Y));
                m_editorCore.Tick();
            };
        }

        internal void OnGraphicsContextInitialized(SharpGL.OpenGL context)
        {
            m_editorCore.SetGraphicsContext(context);
        }

        internal void OnOutputResized(SharpGL.OpenGL context, float width, float height)
        {
            m_editorCore.OnOutputResized(context, width, height);
        }

        internal void SetMouseState(MouseButton mouseButton, bool down)
        {
            Input.SetMouseState(mouseButton, down);
        }

        internal void SetKeyboardState(Key key, bool down)
        {
            Input.SetkeyboardState(key, down);
        }
    }
}

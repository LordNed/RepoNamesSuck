using OpenTK;
using System;
using System.Windows.Input;

namespace WEditor
{
    public class Input
    {
        /// <summary> Mouse position in pixel coordinates. Read only. </summary>
        public static Vector3 MousePosition { get; private set; }
        /// <summary> Delta position in pixel coordinates between frames. Read only. </summary>
        public static Vector3 MouseDelta { get; private set; }

        /// <summary> Keys currently down this frame. </summary>
        private static readonly bool[] _keysDown = new bool[256];
        /// <summary> Keys that were down last frame. </summary>
        private static readonly bool[] _prevKeysDown = new bool[256];

        private static readonly bool[] _mouseBtnsDown = new bool[3];
        private static readonly bool[] _prevMouseBtnsDown = new bool[3];
        private static Vector3 _prevMousePos;

        public static bool GetKey(Key key)
        {
            return _keysDown[(int)key];
        }

        public static bool GetKeyDown(Key key)
        {
            return _keysDown[(int)key] && !_prevKeysDown[(int)key];
        }

        public static bool GetKeyUp(Key key)
        {
            return _prevKeysDown[(int)key] && !_keysDown[(int)key];
        }

        public static bool GetMouseButton(int button)
        {
            return _mouseBtnsDown[button];
        }

        public static bool GetMouseButtonDown(int button)
        {
            return _mouseBtnsDown[button] && !_prevMouseBtnsDown[button];
        }

        public static bool GetMouseButtonUp(int button)
        {
            return _prevMouseBtnsDown[button] && !_mouseBtnsDown[button];
        }

        internal static void Internal_UpdateInputState()
        {
            for (int i = 0; i < 256; i++)
                _prevKeysDown[i] = _keysDown[i];

            for (int i = 0; i < 3; i++)
                _prevMouseBtnsDown[i] = _mouseBtnsDown[i];

            MouseDelta = MousePosition - _prevMousePos;
            _prevMousePos = MousePosition;
        }

        public static void SetkeyboardState(Key keyCode, bool bPressed)
        {
            _keysDown[(int)keyCode] = bPressed;
        }

        public static void SetMouseState(MouseButton button, bool bPressed)
        {
            _mouseBtnsDown[MouseButtonEnumToInt(button)] = bPressed;
        }

        public static void SetMousePosition(Vector2 mousePos)
        {
            MousePosition = new Vector3(mousePos.X, mousePos.Y, 0);
        }

        private static int MouseButtonEnumToInt(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return 0;
                case MouseButton.Right:
                    return 1;
                case MouseButton.Middle:
                    return 2;
            }

            WLog.Warning(LogCategory.EditorCore, null, "Unknown Mouse Button enum {0}, returning Left!", button);
            return 0;
        }
    }
}

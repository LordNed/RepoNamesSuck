using OpenTK;
using WEditor.Rendering;

namespace WEditor
{
    public class EditorCameraMovement : WComponent
    {
        public float MoveSpeed = 1500f;
        public float MouseSensitivity = 20f;
        public Camera Camera;

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            Vector3 moveDir = Vector3.Zero;
            if (Input.GetKey(System.Windows.Input.Key.W))
            {
                moveDir += Vector3.UnitZ;
            }
            if (Input.GetKey(System.Windows.Input.Key.S))
            {
                moveDir -= Vector3.UnitZ;
            }
            if (Input.GetKey(System.Windows.Input.Key.D))
            {
                moveDir += Vector3.UnitX;
            }
            if (Input.GetKey(System.Windows.Input.Key.A))
            {
                moveDir -= Vector3.UnitX;
            }
            if(Input.GetKey(System.Windows.Input.Key.Q))
            {
                moveDir -= Vector3.UnitY;
            }
            if(Input.GetKey(System.Windows.Input.Key.E))
            {
                moveDir += Vector3.UnitY;
            }

            // If they're holding down the shift key adjust their FOV when they scroll, otherwise adjust move speed.
            if (Input.GetKey(System.Windows.Input.Key.LeftShift) || Input.GetKey(System.Windows.Input.Key.RightShift))
            {
                Camera.NearClipPlane = MathE.Clamp(Camera.NearClipPlane + Input.MouseScrollDelta * 50 * deltaTime * 1.0f, 100, 10000);
                Camera.FarClipPlane = MathE.Clamp(Camera.FarClipPlane + Input.MouseScrollDelta * 10 * deltaTime * 1.2f, 5000, 100000);
            }
            else
            {
                MoveSpeed += Input.MouseScrollDelta * 100 * deltaTime;
                MoveSpeed = MathE.Clamp(MoveSpeed, 100, 8000);
            }

            if (Input.GetMouseButton(1))
            {
                Rotate(deltaTime, Input.MouseDelta.X, Input.MouseDelta.Y);
            }

            // Early out if we're not moving this frame.
            if (moveDir.LengthFast < 0.1f)
                return;

            float moveSpeed = Input.GetKey(System.Windows.Input.Key.LeftShift) ? MoveSpeed * 3f : MoveSpeed;

            // Normalize the move direction
            moveDir.NormalizeFast();

            // Make it relative to the current rotation.
            moveDir = Camera.Transform.Rotation.Multiply(moveDir);

            Camera.Transform.Position += Vector3.Multiply(moveDir, moveSpeed * deltaTime);
        }

        private void Rotate(float deltaTime, float x, float y)
        {
            Camera.Transform.Rotate(Vector3.UnitY, x * deltaTime * MouseSensitivity);
            Camera.Transform.Rotate(Camera.Transform.Right, y * deltaTime * MouseSensitivity);

            // Clamp them from looking over the top point.
            Vector3 up = Vector3.Cross(Camera.Transform.Forward, Camera.Transform.Right);
            if (Vector3.Dot(up, Vector3.UnitY) < 0.01f)
            {
                Camera.Transform.Rotate(Camera.Transform.Right, -y * deltaTime * MouseSensitivity);
            }
        }
    }
}

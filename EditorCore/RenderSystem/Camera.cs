using OpenTK;
using System;
using WEditor.Common;

namespace WEditor.Rendering
{
    public class Camera : Component
    {
        /// <summary> The near clipping plane distance. </summary>
        public float NearClipPlane
        {
            get { return m_nearClipPlane; }
            set
            {
                m_nearClipPlane = value;
                CalculateProjectionMatrix();
            }
        }

        /// <summary> The far clipping plane distance. </summary>
        public float FarClipPlane
        {
            get { return m_farClipPlane; }
            set
            {
                m_farClipPlane = value;
                CalculateProjectionMatrix();
            }
        }

        /// <summary> Vertical field of view in degrees. </summary>
        public float FieldOfView
        {
            get { return m_fieldOfView; }
            set
            {
                m_fieldOfView = value;
                CalculateProjectionMatrix();
            }
        }

        /// <summary> Width of the camera output in Pixels. </summary>
        public float PixelWidth
        {
            get { return m_pixelWidth; }
            internal set
            {
                m_pixelWidth = value;
                CalculateProjectionMatrix();
                CalculatePixelRect();
            }
        }

        /// <summary> Height of the camera output in Pixels. </summary>
        public float PixelHeight
        {
            get { return m_pixelHeight; }

            internal set
            {
                m_pixelHeight = value;
                CalculateProjectionMatrix();
                CalculatePixelRect();
            }
        }

        /// <summary> Viewport width/height. Read only. </summary>
        public float AspectRatio { get { return (PixelWidth*m_viewportRect.Width) / (PixelHeight*m_viewportRect.Height); } }

        /// <summary> Color to clear the backbuffer with. </summary>
        public Color ClearColor;

        /// <summary> Current Projection Matrix of camera. </summary>
        public Matrix4 ProjectionMatrix { get { return m_projectionMatrix; } }
        
        /// <summary> Rectangle (in normalized viewport/output target coordinates) this camera draws onto. </summary>
        public Rect ViewportRect
        {
            get { return m_viewportRect; }
            set
            {
                m_viewportRect.Width = MathE.ClampNormalized(value.Width);
                m_viewportRect.Height = MathE.ClampNormalized(value.Height);
                m_viewportRect.X = MathE.ClampNormalized(value.X);
                m_viewportRect.Y = MathE.ClampNormalized(value.Y);

                CalculateProjectionMatrix();
                CalculatePixelRect();
            }
        }

        public Rect PixelRect
        {
            get { return m_pixelRect; }
        }

        /// <summary> View Matrix of the Camera. Calculated every frame as there's no way to see when a Transform has been dirtied. </summary>
        public Matrix4 ViewMatrix
        {
            get
            {
                Matrix4 rhView = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Forward, Vector3.UnitY);
                return rhView;
            }
        }

        private float m_nearClipPlane = 100f;
        private float m_farClipPlane = 10000f;
        private float m_fieldOfView = 45f;
        private float m_pixelWidth;
        private float m_pixelHeight;
        private Matrix4 m_projectionMatrix;
        private Rect m_viewportRect;
        private Rect m_pixelRect;

        public Camera()
        {
            m_viewportRect = new Rect(0f, 0f, 1f, 1f);
        }

        private void CalculateProjectionMatrix()
        {
            m_projectionMatrix = Matrix4.CreateScale(-1f, 1f, 1f) * Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), AspectRatio, NearClipPlane, FarClipPlane);
        }

        private void CalculatePixelRect()
        {
            m_pixelRect.X = ViewportRect.X * PixelWidth;
            m_pixelRect.Y = ViewportRect.Y * PixelHeight;
            m_pixelRect.Width = ViewportRect.Width * PixelWidth;
            m_pixelRect.Height = ViewportRect.Height * PixelHeight;
        }

        public Ray ViewportPointToRay(Vector3 mousePos)
        {
            Vector3 mousePosA = new Vector3(mousePos.X, mousePos.Y, -1f);
            Vector3 mousePosB = new Vector3(mousePos.X, mousePos.Y, 1f);


            Vector4 nearUnproj = UnProject(ProjectionMatrix, ViewMatrix, mousePosA);
            Vector4 farUnproj = UnProject(ProjectionMatrix, ViewMatrix, mousePosB);

            Vector3 dir = farUnproj.Xyz - nearUnproj.Xyz;
            dir.Normalize();

            return new Ray(nearUnproj.Xyz, dir);
        }


        public Vector4 UnProject(Matrix4 projection, Matrix4 view, Vector3 mouse)
        {
            Vector4 vec = new Vector4();

            vec.X = 2.0f * mouse.X / PixelWidth - 1;
            vec.Y = -(2.0f * mouse.Y / PixelHeight - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec;
        }


        public float MoveSpeed = 350f;
        public float MouseSensitivity = 20f;

        public override void Update()
        {
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

            if (Input.GetMouseButton(1))
            {
                Rotate(Input.MouseDelta.X, Input.MouseDelta.Y);
            }

            // Early out if we're not moving this frame.
            if (moveDir.LengthFast < 0.1f)
                return;

            float moveSpeed = Input.GetKey(System.Windows.Input.Key.LeftShift) ? MoveSpeed * 3f : MoveSpeed;

            // Normalize the move direction
            moveDir.NormalizeFast();

            // Make it relative to the current rotation.
            moveDir = Transform.Rotation.Multiply(moveDir);

            Transform.Position += Vector3.Multiply(moveDir, moveSpeed * Time.DeltaTime);
        }

        private void Rotate(float x, float y)
        {
            Transform.Rotate(Vector3.UnitY, x * Time.DeltaTime * MouseSensitivity);
            Transform.Rotate(Transform.Right, y * Time.DeltaTime * MouseSensitivity);

            // Clamp them from looking over the top point.
            Vector3 up = Vector3.Cross(Transform.Forward, Transform.Right);
            if (Vector3.Dot(up, Vector3.UnitY) < 0.01f)
            {
                Transform.Rotate(Transform.Right, -y * Time.DeltaTime);
            }
        }
    }
}

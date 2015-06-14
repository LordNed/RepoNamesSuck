using OpenTK;
using WEditor.Common;

namespace WEditor.RenderSystem
{
    public class Camera
    {
        /// <summary> The near clipping plane distance. </summary>
        public float NearClipPlane = 0.1f;

        /// <summary> The far clipping plane distance. </summary>
        public float FarClipPlane = 100f;

        /// <summary> Vertical field of view in degrees. </summary>
        public float FieldOfView = 45f;

        /// <summary> Viewport width/height. Read only. </summary>
        public float AspectRatio { get { return ViewportRect.Width / ViewportRect.Height; } }

        /// <summary> Color to clear the backbuffer with. </summary>
        public Color24 ClearColor;

        /// <summary> Current Projection Matrix of camera. Updated when <see cref="ViewportRect"/> is modified. </summary>
        public Matrix4 ProjectionMatrix { get { return m_projectionMatrix; } private set; }
        
        /// <summary> Rectangle (in normalized viewport/output target coordinates) this camera draws onto. </summary>
        public Rect ViewportRect
        {
            get { return m_viewportRect; }
            set
            {
                m_viewportRect.Width = MathE.Clamp(value.Width, 0f, 1f);
                m_viewportRect.Height = MathE.Clamp(value.Height, 0f, 1f);
                m_viewportRect.X = MathE.Clamp(value.X, 0f, 1f);
                m_viewportRect.Y = MathE.Clamp(value.Y, 0f, 1f);

                CalculateProjectionMatrix();
            }
        }

        public Matrix4 ViewMatrix
        {
            get
            {
                Matrix4 rhView = Matrix4.LookAt(Vector3.Zero, Vector3.Zero + Vector3.One, Vector3.UnitY);
                return rhView;
            }
        }

        private Rect m_viewportRect;
        private Matrix4 m_projectionMatrix;

        private void CalculateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4.CreateScale(-1f, 1f, 1f) * Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), AspectRatio, NearClipPlane, FarClipPlane);
        }

        public Ray ViewportPointToRay(Vector3 mousePos)
        {
            Vector3 mousePosA = new Vector3(mousePos.X, mousePos.Y, -1f);
            Vector3 mousePosB = new Vector3(mousePos.X, mousePos.Y, 1f);


            Vector4 nearUnproj = UnProject(ProjMatrix, ViewMatrix, mousePosA);
            Vector4 farUnproj = UnProject(ProjMatrix, ViewMatrix, mousePosB);

            Vector3 dir = farUnproj.Xyz - nearUnproj.Xyz;
            dir.Normalize();

            return new Ray(nearUnproj.Xyz, dir);
        }

        public Vector4 UnProject(Matrix4 projection, Matrix4 view, Vector3 mouse)
        {
            Vector4 vec;

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
    }
}

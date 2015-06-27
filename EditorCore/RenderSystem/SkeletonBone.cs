using OpenTK;

namespace WEditor.Rendering
{
    public class SkeletonBone
    {
        /// <summary> Human-readable name for this bone. </summary>
        public string Name;

        public ushort Unknown1;
        public ushort Unknown2;
        public Vector3 Scale;
        public Quaternion Rotation;
        public Vector3 Translation;
        public float BoundingSphereDiameter;
        public Vector3 BoundingBoxMin;
        public Vector3 BoundingBoxMax;

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "Joint" : Name;
        }
    }
}

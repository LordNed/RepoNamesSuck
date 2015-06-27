using OpenTK;

namespace WEditor.Rendering
{
    public class SkeletonBone
    {
        /// <summary> Human-readable name for this bone. </summary>
        public string Name;

        /// <summary> This bones parent if it has one. Null if root bone. </summary>
        public SkeletonBone Parent;

        public Vector3 Scale;
        public Quaternion Rotation;
        public Vector3 Translation;
        public float BoundingSphereDiameter;
        public Vector3 BoundingBoxMin;
        public Vector3 BoundingBoxMax;

        public SkeletonBone()
        {
            Name = "Joint";
            Parent = null;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
            Translation = Vector3.Zero;
            BoundingSphereDiameter = 0f;
            BoundingBoxMin = Vector3.Zero;
            BoundingBoxMax = Vector3.Zero;
        }

        public SkeletonBone(SkeletonBone other)
        {
            Name = other.Name;
            Parent = other.Parent;
            Scale = other.Scale;
            Rotation = other.Rotation;
            Translation = other.Translation;
            BoundingSphereDiameter = other.BoundingSphereDiameter;
            BoundingBoxMin = other.BoundingBoxMin;
            BoundingBoxMax = other.BoundingBoxMax;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

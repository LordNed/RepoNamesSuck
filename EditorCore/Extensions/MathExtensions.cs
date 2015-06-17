using OpenTK;

namespace WEditor
{
    public static class MathE
    {
        /// <summary>
        /// Multiplies the rotation by the vector and returns the rotated vector. Useful for converting from
        /// local space to world space of a Transform via Rotation.Multiply(localDirection).
        /// </summary>
        /// <param name="quat"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 Multiply(this Quaternion quat, Vector3 vec)
        {
            float num = quat.X * 2f;
            float num2 = quat.Y * 2f;
            float num3 = quat.Z * 2f;
            float num4 = quat.X * num;
            float num5 = quat.Y * num2;
            float num6 = quat.Z * num3;
            float num7 = quat.X * num2;
            float num8 = quat.X * num3;
            float num9 = quat.Y * num3;
            float num10 = quat.W * num;
            float num11 = quat.W * num2;
            float num12 = quat.W * num3;

            Vector3 result;
            result.X = (1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z;
            result.Y = (num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z;
            result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z;
            return result;
        }

        public static int Clamp(int value, int minValue, int maxValue)
        {
            if (value < minValue)
                return minValue;
            if (value > maxValue)
                return maxValue;

            return value;
        }

        public static float Clamp(float value, float minValue, float maxValue)
        {
            if (value < minValue)
                return minValue;
            if (value > maxValue)
                return maxValue;

            return value;
        }

        public static float ClampNormalized(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static Vector3 Mult(this Quaternion value, Vector3 vec)
        {
            Quaternion vectorQuat, inverseQuat, resultQuat;
            Vector3 resultVector;

            vectorQuat = new Quaternion(vec.X, vec.Y, vec.Z, 0f);
            inverseQuat = value.Invert_Custom();
            resultQuat = vectorQuat * inverseQuat;
            resultQuat = value * resultQuat;

            resultVector = new Vector3(resultQuat.X, resultQuat.Y, resultQuat.Z);
            return resultVector;
        }

        public static Quaternion Invert_Custom(this Quaternion value)
        {
            Quaternion newQuat = new Quaternion(value.X, value.Y, value.Z, value.W);
            float length = 1.0f / ((newQuat.X * newQuat.X) + (newQuat.Y * newQuat.Y) + (newQuat.Z * newQuat.Z) + (newQuat.W * newQuat.W));
            newQuat.X *= -length;
            newQuat.Y *= -length;
            newQuat.Z *= -length;
            newQuat.W *= length;

            return newQuat;
        }
    }
}

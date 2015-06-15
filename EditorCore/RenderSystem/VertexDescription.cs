using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class VertexDescription
    {
        private List<ShaderAttributeIds> m_enabledAttributes;

        public VertexDescription()
        {
            m_enabledAttributes = new List<ShaderAttributeIds>();
        }

        internal bool AttributeIsEnabled(ShaderAttributeIds attribute)
        {
            return m_enabledAttributes.Contains(attribute);
        }

        internal int GetAttributeSize(ShaderAttributeIds attribute)
        {
            switch (attribute)
            {
                case ShaderAttributeIds.Position:
                case ShaderAttributeIds.Normal:
                    return 3;
                case ShaderAttributeIds.Color0:
                case ShaderAttributeIds.Color1:
                    return 4;
                case ShaderAttributeIds.TexCoord0:
                case ShaderAttributeIds.TexCoord1:
                    return 2;
                default:
                    return 0;
            }
        }

        internal VertexAttribPointerType GetAttributePointerType(ShaderAttributeIds attribute)
        {
            switch (attribute)
            {
                case ShaderAttributeIds.Position:
                case ShaderAttributeIds.Normal:
                case ShaderAttributeIds.Color0:
                case ShaderAttributeIds.Color1:
                case ShaderAttributeIds.TexCoord0:
                case ShaderAttributeIds.TexCoord1:
                    return VertexAttribPointerType.Float;

                default:
                    Console.WriteLine("[VertexDescription] Unsupported ShaderAttributeId: " + attribute);
                    return VertexAttribPointerType.Float;
            }
        }

        internal int GetStride(ShaderAttributeIds attribute)
        {
            switch (attribute)
            {
                case ShaderAttributeIds.Position:
                case ShaderAttributeIds.Normal:
                    return 4 * 3;
                case ShaderAttributeIds.Color0:
                case ShaderAttributeIds.Color1:
                    return 4 * 4;
                case ShaderAttributeIds.TexCoord0:
                case ShaderAttributeIds.TexCoord1:
                    return 4 * 2;
                default:
                    Console.WriteLine("[VertexDescription] Unsupported ShaderAttributeId: " + attribute);
                    return 0;
            }
        }

        internal void EnableAttribute(ShaderAttributeIds attribute)
        {
            m_enabledAttributes.Add(attribute);
        }
    }
}

using SharpGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Rendering
{
    public class Shader
    {
        protected enum ShaderType
        {
            VertexShader,
            FragmentShader
        }

        public enum ShaderAttributeIds
        {
            Position = 0,
        }

        public uint ProgramId;

        private OpenGL m_context;

        public Shader(OpenGL context)
        {
            m_context = context;
        }
        
        public void CreateShaderFromFile(string vertShaderPath, string fragShaderPath)
        {
            // Initialize an OpenGL program for the shader.
            ProgramId = m_context.CreateProgram();

            uint vertShaderId, fragShaderId;
            LoadShader(vertShaderPath, ShaderType.VertexShader, ProgramId, out vertShaderId);
            LoadShader(fragShaderPath, ShaderType.FragmentShader, ProgramId, out fragShaderId);

            // Once the shaders are created and linked to the program we can technically delete the shader.
            // This de-increments the internal reference count so that only the Program is holding onto it.
            // That way, once we delete the Program, it'll destroy itself.
            m_context.DeleteShader(vertShaderId);
            m_context.DeleteShader(fragShaderId);

            m_context.BindAttribLocation(ProgramId, (uint)ShaderAttributeIds.Position, "vertexPos");

            // Link the program now that we've compiled it.
            m_context.LinkProgram(ProgramId);

            //shader.UniformMVP = GL.GetUniformLocation(shader.ProgramId, "modelview");
            //shader.UniformColor = GL.GetUniformLocation(shader.ProgramId, "inColor");
        }

        protected void LoadShader(string fileName, ShaderType type, uint program, out uint address)
        {
            uint shaderType = OpenGL.GL_VERTEX_SHADER;
            if(type == ShaderType.VertexShader)
                shaderType = OpenGL.GL_VERTEX_SHADER;
            else
                shaderType = OpenGL.GL_FRAGMENT_SHADER;

            address = m_context.CreateShader(shaderType);
            try
            {
                using (var streamReader = new System.IO.StreamReader(fileName))
                {
                    m_context.ShaderSource(address, streamReader.ReadToEnd());

                    m_context.CompileShader(address);
                    m_context.AttachShader(program, address);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WEditor.Core] Caught exception while loading shader {0}. Ex: {1}", fileName, ex);
            }
        }
    }
}

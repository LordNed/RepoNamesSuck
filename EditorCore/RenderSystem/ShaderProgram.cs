using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public enum ShaderAttributeIds
    {
        None = 0,
        Position = 1,
        Normal = 2,
        Color0 = 3,
        Color1 = 4,
        TexCoord0 = 5,
        TexCoord1 = 6,
    }

    public class ShaderProgram
    {
        public readonly int ProgramId = -1;
        public readonly int UniformMVP = -1;
        public readonly int UniformColor = -1;
        
        public ShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            ProgramId = GL.CreateProgram();

            LoadShader(vertexShaderPath, ShaderType.VertexShader, ProgramId);
            LoadShader(fragmentShaderPath, ShaderType.FragmentShader, ProgramId);

            GL.BindAttribLocation(ProgramId, (int)ShaderAttributeIds.Position, "vertexPos");
            GL.BindAttribLocation(ProgramId, (int)ShaderAttributeIds.Color0, "color");

            GL.LinkProgram(ProgramId);

            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[EditorCore.Rendering] Error linking shader. Result: {0}", GL.GetProgramInfoLog(ProgramId));
            }

            UniformMVP = GL.GetUniformLocation(ProgramId, "modelview");
            UniformColor = GL.GetUniformLocation(ProgramId, "inColor");

            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[EditorCore.Rendering] Failed to get modelview uniform for shader. Result: {0}", GL.GetProgramInfoLog(ProgramId));
            }
        }

        protected void LoadShader(string fileName, ShaderType type, int program)
        {
            int address = GL.CreateShader(type);
            try
            {
                using (var streamReader = new System.IO.StreamReader(fileName))
                {
                    GL.ShaderSource(address, streamReader.ReadToEnd());

                    GL.CompileShader(address);
                    GL.AttachShader(program, address);

                    int compileStatus;
                    GL.GetShader(address, ShaderParameter.CompileStatus, out compileStatus);

                    if (compileStatus != 1)
                    {
                        Console.WriteLine("[EditorCore.Rendering] Failed to compile shader {0}. Log:\n{1}", fileName, GL.GetShaderInfoLog(address));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[EditorCore.Rendering] Caught exception while loading shader {0}. Ex: {1}", fileName, ex);
            }

            // De-increment the reference count once it's been attached to the program
            GL.DeleteShader(address);
        }
    }
}

using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public enum ShaderAttributeIds
    {
        None = 0,
        Position = 1,
        Color = 2,
        TexCoord = 3,
    }

    public class ShaderProgram
    {
        public readonly int ProgramId;
        public readonly int UniformMVP;
        public readonly int UniformColor;
        
        public ShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            ProgramId = GL.CreateProgram();

            int vertShaderId, fragShaderId;
            LoadShader(vertexShaderPath, ShaderType.VertexShader, ProgramId, out vertShaderId);
            LoadShader(fragmentShaderPath, ShaderType.FragmentShader, ProgramId, out fragShaderId);

            // Once the shaders have been created and linked to the program (via LoadShader) we no
            // longer need the reference to the shader as the program will refcount it for us, so
            // we free it here.
            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(ProgramId, (int)ShaderAttributeIds.Position, "vertexPos");

            GL.LinkProgram(ProgramId);

            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Error linking shader. Result: {0}", GL.GetProgramInfoLog(ProgramId));
            }

            UniformMVP = GL.GetUniformLocation(ProgramId, "modelview");
            UniformColor = GL.GetUniformLocation(ProgramId, "inColor");

            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Failed to get modelview uniform for shader. Result: {0}", GL.GetProgramInfoLog(ProgramId));
            }
        }

        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
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
                        Console.WriteLine("[WEditor.Core] Failed to compile shader {0}. Log:\n{1}", fileName, GL.GetShaderInfoLog(address));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WEditor.Core] Caught exception while loading shader {0}. Ex: {1}", fileName, ex);
            }

            // De-increment the reference count once it's been attached to the program
            //GL.DeleteShader(address);
        }
    }
}

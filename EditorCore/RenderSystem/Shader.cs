using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Rendering
{
    public class Shader : IDisposable
    {
        public readonly string Name;
        public int UniformMVPBlock { get; private set; }
        public int UniformVertexBlock {get; private set;}
        //public int UniformModelMtx { get; private set; }
        //public int UniformViewMtx { get; private set; }
        //public int UniformProjMtx { get; private set; }
        //public int UniformTexMtx { get; private set; }
        //public int UniformPostMtx { get; private set; }
        //public int UniformColor0Amb { get; private set; }
        //public int UniformColor0Mat { get; private set; }
        //public int UniformColor1Amb { get; private set; }
        //public int UniformColor1Mat { get; private set; }
        public int UniformNumLights { get; private set; }
        public int UniformLightBlock { get; private set; }

        private int m_vertexAddress = -1;
        private int m_fragmentAddress = -1;
        private int m_programAddress = -1;

        public Shader(string name)
        {
            Name = name;
        }

        public bool CompileSource(string code, ShaderType type)
        {
            // Generate a new shader and clean up the old shader with a warning if they forgot to link before
            // trying to compile again.
            int shaderAddress = -1;
            switch (type)
            {
                case ShaderType.FragmentShader:
                    if (m_fragmentAddress >= 0)
                    {
                        Console.WriteLine("[ShaderCompiler] Shader \"{0}\" called CompileSource for ShaderType: {1} twice before linking! Disposing old shader.", Name, type);
                        GL.DeleteShader(m_fragmentAddress);
                    }
                    m_fragmentAddress = GL.CreateShader(type);
                    shaderAddress = m_fragmentAddress;
                    break;
                case ShaderType.VertexShader:
                    if (m_vertexAddress >= 0)
                    {
                        Console.WriteLine("[ShaderCompiler] Shader \"{0}\" called CompileSource for ShaderType: {1} twice before linking! Disposing old shader.", Name, type);
                        GL.DeleteShader(m_fragmentAddress);
                    }
                    m_vertexAddress = GL.CreateShader(type);
                    shaderAddress = m_vertexAddress;
                    break;
            }

            GL.ShaderSource(shaderAddress, code);

            GL.CompileShader(shaderAddress);
            //GL.AttachShader(program, address);

            int compileStatus;
            GL.GetShader(shaderAddress, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus != 1)
            {
                Console.WriteLine("[ShaderCompiler] Failed to compile shader {0}. Log:\n{1}", Name, GL.GetShaderInfoLog(shaderAddress));
                return false;
            }

            return true;
        }


        public void LinkShader()
        {
            if(m_programAddress >= 0)
            {
                Console.WriteLine("[ShaderCompiler] Shader \"{0}\" called LinkShader for already linked shader! Disposing old program.", Name);
                GL.DeleteProgram(m_programAddress);
            }

            if (m_fragmentAddress < 0 || m_vertexAddress < 0)
                throw new Exception("Shader does not have both a Vertex and Fragment shader!");

            // Initialize a program and link the already compiled shaders
            m_programAddress = GL.CreateProgram();
            GL.AttachShader(m_programAddress, m_vertexAddress);
            GL.AttachShader(m_programAddress, m_fragmentAddress);


            // Bind our Attribute locations before we link the program.
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Position, "RawPosition");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Normal, "RawNormal");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Color0, "RawColor0");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Color1, "RawColor1");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex0, "RawTex0");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex1, "RawTex1");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex2, "RawTex2");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex3, "RawTex3");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex4, "RawTex4");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex5, "RawTex5");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex6, "RawTex6");
            GL.BindAttribLocation(m_programAddress, (int)ShaderAttributeIds.Tex7, "RawTex7");

            GL.LinkProgram(m_programAddress);
            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[ShaderCompiler] Error linking shader. Result: {0}", GL.GetProgramInfoLog(m_programAddress));
            }

            // Now that the program is linked, bind to our uniform locations.
            UniformMVPBlock = GL.GetUniformLocation(m_programAddress, "MVPBlock");
            UniformVertexBlock = GL.GetUniformLocation(m_programAddress, "VertexBlock");
            UniformLightBlock = GL.GetUniformLocation(m_programAddress, "LightBlock");
            UniformNumLights = GL.GetUniformLocation(m_programAddress, "NumLights");



            // Now that we've (presumably) set both a vertex and a fragment shader and linked them to the program,
            // we're going to clean up the reference to the shaders as the Program now keeps its own reference.
            GL.DeleteShader(m_vertexAddress);
            GL.DeleteShader(m_fragmentAddress);
            m_vertexAddress = -1;
            m_fragmentAddress = -1;
        }

        public void Dispose()
        {
            if(m_vertexAddress >= 0)
                GL.DeleteShader(m_vertexAddress);
            if (m_fragmentAddress >= 0)
                GL.DeleteShader(m_fragmentAddress);

            if (m_programAddress >= -1)
                GL.DeleteProgram(m_programAddress);
        }
    }
}

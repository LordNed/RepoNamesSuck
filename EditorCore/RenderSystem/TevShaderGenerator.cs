using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WEditor.Common.Nintendo.J3D;

namespace WEditor.Rendering
{
    public class TEVShaderGenerator
    {
        private static readonly string[] m_kTexCoordSrc = new string[]
        {
            "RawPosition.xyz",
            "RawNormal.xyz",
            "RawBinormal.xyz",
            "RawTangent.xyz",
            "RawTex0.xy, 1.0",
            "RawTex1.xy, 1.0",
            "RawTex2.xy, 1.0",
            "RawTex3.xy, 1.0",
            "RawTex4.xy, 1.0",
            "RawTex5.xy, 1.0",
            "RawTex6.xy, 1.0",
            "RawTex7.xy, 1.0",
            "RawTexCoord0.xy, 1.0",
            "RawTexCoord1.xy, 1.0",
            "RawTexCoord2.xy, 1.0",
            "RawTexCoord3.xy, 1.0",
            "RawTexCoord4.xy, 1.0",
            "RawTexCoord5.xy, 1.0",
            "RawTexCoord6.xy, 1.0",
            "RawColor0.xyz",
            "RawColor1.xyz",
        };

        public static Shader GenerateShader(Material fromMat)
        {
            Shader shader = new Shader(fromMat.Name);
            bool success = GenerateVertexShader(shader, fromMat);
            if (success)
                success = GenerateFragmentShader(shader, fromMat);

            if(!success)
            {
                Console.WriteLine("[ShaderCompiler] Failed to generate shader for material {0}", fromMat.Name);
                return null;
            }

            shader.LinkShader();

            return shader;
        }

        private static bool GenerateFragmentShader(Shader shader, Material mat)
        {
            StringBuilder stream = new StringBuilder();

            // Shader Header
            stream.AppendLine("#version 330 core");
            stream.AppendLine();

            // Configure inputs to match our outputs from VS
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Position))
                stream.AppendLine("in vec3 Position;");

            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Normal))
                stream.AppendLine("in vec3 Normal;");

            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0))
                stream.AppendLine("in vec4 Color0;");

            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
                stream.AppendLine("in vec4 Color1;");

            for (int texGen = 0; texGen < mat.NumTexGens; texGen++)
            {
                if (mat.TexGenInfos[texGen] != null)
                    stream.AppendLine(string.Format("in vec3 Tex{0};", texGen));
            }

            stream.AppendLine("in vec4 COLOR0A0;");
            stream.AppendLine("in vec4 COLOR1A1;");
            stream.AppendLine();
            stream.AppendLine("out vec4 PixelColor;");
            stream.AppendLine();
            stream.AppendLine("layout(std140) uniform PixelBlock");
            stream.AppendLine("{");
            stream.AppendLine("    vec4 KonstColors[4];");
            stream.AppendLine("    vec4 TevColor;");
            stream.AppendLine("    vec4 TintColor;");
            stream.AppendLine("};");
            stream.AppendLine();

            // Texture Inputs
            // ...
            stream.AppendLine("uniform sampler2D Texture;");

            // Main Function blahblah
            stream.AppendLine("void main()");
            stream.AppendLine("{");
            stream.AppendLine("    vec4 TevInA = vec4(0, 0, 0, 0), TevInB = vec4(0, 0, 0, 0), TevInC = vec4(0, 0, 0, 0), TevInD = vec4(0, 0, 0, 0);");
            stream.AppendLine("    vec4 Prev = vec4(0, 0, 0, 0), C0 = TevColor, C1 = C0, C2 = C0;");
            stream.AppendLine("    vec4 Ras = vec4(0, 0, 0, 1), Tex = vec4(0, 0, 0, 0);");
            stream.AppendLine("    vec4 Kosnt = vec4(1, 1, 1, 1);");
            stream.AppendLine("    vec2 TevCoord = vec2(0, 0);");
            stream.AppendLine();

            for(int tevStage = 0; tevStage < mat.NumTevStages; tevStage++)
            {
                stream.AppendLine(string.Format("    // TEV Stage {0} - ClrOp: {1}", tevStage, mat.TevStageInfos[tevStage].ColorOp));
                
            }

            stream.AppendLine("    PixelColor = texture(Texture, Tex0.xy)" + (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0) ? " * Color0;" : ";"));
            stream.AppendLine("}");
            stream.AppendLine();

            // Compile the Fragment Shader and return whether it compiled sucesfully or not.
            System.IO.File.WriteAllText("ShaderDump/" + mat.Name + "_frag_output", stream.ToString());
            return shader.CompileSource(stream.ToString(), OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);
        }



        public static bool GenerateVertexShader(Shader shader, Material mat)
        {
            StringBuilder stream = new StringBuilder();
            
            // Shader Header
            stream.AppendLine("#version 330 core");
            stream.AppendLine();

            // Input Format
            stream.AppendLine("// Input");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Position))
                stream.AppendLine("in vec3 RawPosition;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Normal))
                stream.AppendLine("in vec3 RawNormal;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0))
                stream.AppendLine("in vec4 RawColor0;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
                stream.AppendLine("in vec4 RawColor1;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex0))
                stream.AppendLine("in vec2 RawTex0;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex1))
                stream.AppendLine("in vec2 RawTex1;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex2))
                stream.AppendLine("in vec2 RawTex2;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex3))
                stream.AppendLine("in vec2 RawTex3;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex4))
                stream.AppendLine("in vec2 RawTex4;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex5))
                stream.AppendLine("in vec2 RawTex5;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex6))
                stream.AppendLine("in vec2 RawTex6;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Tex7))
                stream.AppendLine("in vec2 RawTex7;");

            stream.AppendLine();

            // Output Format
            stream.AppendLine("// Output");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Normal))
                stream.AppendLine("out vec3 Normal;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0))
                stream.AppendLine("out vec4 Color0;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
                stream.AppendLine("out vec4 Color1");

            for(int texGen = 0; texGen < mat.NumTexGens; texGen++)
            {
                if (mat.TexGenInfos[texGen] != null)
                    stream.AppendLine(string.Format("out vec3 Tex{0};", texGen));
            }

            stream.AppendLine("out vec4 COLOR0A0;");
            stream.AppendLine("out vec4 COLOR1A1;");

            // Uniforms
            stream.AppendLine();
            stream.AppendLine("// Uniforms");
            stream.AppendLine(
                "   uniform mat4 ModelMtx;\n" +
                "   uniform mat4 ViewMtx;\n" +
                "   uniform mat4 ProjMtx;\n" +
                "\n" +

                "   uniform mat4 TexMtx[10];\n" +
                "   uniform mat4 PostMtx[20];\n" +
                "   uniform vec4 COLOR0_Amb;\n" +
                "   uniform vec4 COLOR0_Mat;\n" +
                "   uniform vec4 COLOR1_Amb;\n" +
                "   uniform vec4 COLOR1_Mat;\n" +
                "\n" +
                "struct GXLight\n" +
                "{\n" +
                "   vec4 Position;\n" +
                "   vec4 Direction;\n" +
                "   vec4 Color;\n" +
                "   vec4 DistAtten;\n" +
                "   vec4 AngleAtten;\n" +
                "};\n" +
                "\n" + 

                "   GXLight Lights[8];\n" +
                "\n" +
                "uniform int NumLights;\n");

            // Main Shader Code
            stream.AppendLine("// Main");
            stream.AppendLine("void main()");
            stream.AppendLine("{");
            stream.AppendLine("    mat4 MVP = ProjMtx * ViewMtx * ModelMtx;");
            stream.AppendLine("    mat4 MV = ViewMtx * ModelMtx;");

            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Position))
                stream.AppendLine("    gl_Position = MVP * vec4(RawPosition, 1);");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Normal))
                stream.AppendLine("    Normal = normalize(RawNormal.xyz * inverse(transpose(mat3(MV))));");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0))
                stream.AppendLine("    Color0 = RawColor0;");
            if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
                stream.AppendLine("    Color1 = RawColor1;");
            
            // No Dynamic Lighting (yet)
            stream.AppendLine("    COLOR0A0 = COLOR0_Mat;");
            stream.AppendLine("    COLOR1A1 = COLOR1_Mat;");
            stream.AppendLine();


            // Texture Coordinate Generation
            stream.AppendLine("    // TexGen");
            for(int texGen = 0; texGen < mat.NumTexGens; texGen++)
            {
                if (mat.TexGenInfos[texGen] == null)
                    continue;

                // No Animation for right now, but texture matrix animations would come here.
                stream.AppendLine(string.Format("    Tex{0} = vec3({1});", pass, m_kTexCoordSrc[(int)mat.TexGenInfos[pass].Source]));
            }

            stream.AppendLine("}");
            stream.AppendLine();

            // Compile the Vertex Shader and return whether it compiled sucesfully or not.
            System.IO.File.WriteAllText("ShaderDump/" + mat.Name + "_vert_output", stream.ToString());
            return shader.CompileSource(stream.ToString(), OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
        }
    }
}

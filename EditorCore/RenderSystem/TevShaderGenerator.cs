using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WEditor.Common.Nintendo.J3D;

namespace WEditor.Rendering
{
    public class TEVShaderGenerator
    {
        private static readonly string[] m_kCoordSrc = new string[]
        {
            "RawPosition.xyz",
            "RawNormal.xyz",
            "0.0, 0.0, 0.0",
            "0.0, 0.0, 0.0",
            "RawTex0.xy, 1.0",
            "RawTex1.xy, 1.0",
            "RawTex2.xy, 1.0",
            "RawTex3.xy, 1.0",
            "RawTex4.xy, 1.0",
            "RawTex5.xy, 1.0",
            "RawTex6.xy, 1.0",
            "RawTex7.xy, 1.0"
        };

        public static Shader GenerateShader(Material fromMat)
        {
            Shader shader = new Shader(fromMat.Name);
            bool success = GenerateVertexShader(shader, fromMat);


            return shader;
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

            for(int texGenStage = 0; texGenStage < mat.NumTexGens; texGenStage++)
            {
                if (mat.TexGenInfos[texGenStage] != null)
                    stream.AppendLine(string.Format("out vec3 Tex{0};", texGenStage));
            }

            stream.AppendLine("out vec4 COLOR0A0;");
            stream.AppendLine("out vec4 COLOR1A1;");

            // Uniforms
            stream.AppendLine();
            stream.AppendLine("// Uniforms");
            stream.AppendLine(
                "layout(std140) uniform MVPBlock\n" +
                "{\n" +
                "   mat4 ModelMtx;\n" +
                "   mat4 ViewMtx;\n" +
                "   mat4 ProjMtx;\n" +
                "};\n" +
                "\n" +
                "layout(std140) uniform VertexBlock\n" +
                "{\n" +
                "   mat4 TexMtx[10];\n" +
                "   mat4 PostMtx[20];\n" +
                "   vec4 COLOR0_Amb;\n" +
                "   vec4 COLOR0_Mat;\n" +
                "   vec4 COLOR1_Amb;\n" +
                "   vec4 COLOR1_Mat;\n" +
                "};\n" +
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
                "layout(std140) uniform LightBlock\n" +
                "{\n" +
                "   GXLight Lights[8];\n" +
                "};\n" +
                "\n" +
                "uniform int NumLights;\n");

            // Main Shader Code
            stream.AppendLine("// Main");
            stream.AppendLine("void main()");
            stream.AppendLine("{");
            stream.AppendLine("    mat4 MVP = ModelMtx * ViewMtx * ProjMtx;");
            stream.AppendLine("    mat4 MV = ModelMtx * ViewMtx;");
            
            // No Dynamic Lighting (yet)
            stream.AppendLine("    COLOR0A0 = COLOR0_Mat;");
            stream.AppendLine("    COLOR1A1 = COLOR1_Mat;");
            stream.AppendLine();


            // Texture Coordinate Generation
            stream.AppendLine("    // TexGen");
            for(int pass = 0; pass < mat.NumTexGens; pass++)
            {
                if (mat.TexGenInfos[pass] == null)
                    continue;

                // No Animation for right now, but texture matrix animations would come here.
                stream.AppendLine(string.Format("    Tex{0} = vec3({1});", pass, m_kCoordSrc[mat.TexGenInfos[pass].Source]));
            }

            stream.AppendLine("}");
            stream.AppendLine();

            // Compile the Vertex Shader and return whether it compiled sucesfully or not.
            System.IO.File.WriteAllText(mat.Name + "_vert_output", stream.ToString());
            return shader.CompileSource(stream.ToString(), OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
        }
    }
}

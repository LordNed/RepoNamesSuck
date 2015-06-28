using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WEditor.Common.Nintendo.J3D;

namespace WEditor.Rendering
{
    public class TEVShaderGenerator
    {
        public static Shader GenerateShader(Material fromMat)
        {
            Shader shader = new Shader(fromMat.Name);
            bool success = GenerateVertexShader(shader, fromMat);
            if (success)
                success = GenerateFragmentShader(shader, fromMat);

            if (!success)
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

            for (int tevStage = 0; tevStage < mat.NumTevStages; tevStage++)
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
            //if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0)) // These two technically should be optional, but ChanCtrlers can specify Color out not from Vert attrib.
                stream.AppendLine("out vec4 Color0;");
            //if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
                stream.AppendLine("out vec4 Color1;");

            for (int texGen = 0; texGen < mat.NumTexGens; texGen++)
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
                "uniform int NumLights;\n" +
                "uniform vec4 ambLightColor;\n");

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

            stream.AppendLine();
            stream.AppendLine("    // Ambient Colors & Material Colors");
            // Add the Ambient Colors for the Material
            for (int a = 0; a < mat.AmbientColors.Length; a++)
            {
                stream.AppendLine(string.Format("    vec4 ambColor{0} = vec4({1}, {2}, {3}, {4});", a, mat.AmbientColors[a].R, mat.AmbientColors[a].G, mat.AmbientColors[a].B, mat.AmbientColors[a].A));
            }

            // Add in the Material Colors
            for (int m = 0; m < mat.MaterialColors.Length; m++)
            {
                stream.AppendLine(string.Format("    vec4 matColor{0} = vec4({1}, {2}, {3}, {4});", m, mat.MaterialColors[m].R, mat.MaterialColors[m].G, mat.MaterialColors[m].B, mat.MaterialColors[m].A));
            }

            stream.AppendLine();
            stream.AppendLine(string.Format("    // ChanCtrl's - {0} count", mat.NumChannelControls));

            // Channel Controllers
            // A vertex can have up to two color channels (RGBA each) which gives us four possible channels:
            // color0, color1, alpha0, alpha1
            // Each channel has an associated ambient color/alpha and a material color/alpha. These can come
            // from vertex colors or existing amb/mat registers.
            for (int chanSel = 0; chanSel < mat.NumChannelControls; chanSel++)
            {
                ChanCtrl chanInfo = mat.ChannelControls[chanSel];
                string chanTarget, vtxColor, ambColor, matColor, ambLight, diffLight;
                string swizzle, chan;
                bool alpha;

                // Todo: Is this really a fixed order?
                switch (chanSel)
                {
                    case /* Color0 */ 0: chan = "0"; swizzle = ".rgb"; alpha = false; break;
                    case /* Alpha0 */ 1: chan = "0"; swizzle = ".a"; alpha = true; break;
                    case /* Color1 */ 2: chan = "1"; swizzle = ".rgb"; alpha = false; break;
                    case /* Alpha1 */ 3: chan = "1"; swizzle = ".a"; alpha = true; break;
                    default:
                        Console.WriteLine("[TEVShaderGen] Unknown vertex output color channel {0}, skipping.", chanSel);
                        continue;
                }

                chanTarget = string.Format("Color{0}{1}", chan, swizzle);
                ambColor = (chanInfo.AmbientSrc == GXColorSrc.Vertex ? "RawColor" : "ambColor") + chan + swizzle;
                matColor = (chanInfo.MaterialSrc == GXColorSrc.Vertex ? "RawColor" : "matColor") + chan + swizzle;
                ambLight = "ambLightColor" + swizzle;
                diffLight = GetLightCalcString(chanInfo, alpha);

                //Color{0}.rgb = ambient * ambLightColor * light
                stream.AppendLine(string.Format("    Color{0} = vec4(1, 1, 1, 1);", chan));
                if (chanInfo.Enable)
                    stream.AppendLine(string.Format("    {0} = {1} * {2} + {3} * {4};", chanTarget, ambColor, ambLight, matColor, diffLight));
                else
                    stream.AppendLine(string.Format("    {0} = {1};", chanTarget, matColor));

                stream.AppendLine();
                stream.AppendLine();
            }


            // Texture Coordinate Generation
            stream.AppendLine(string.Format("    // TexGen - {0} count", mat.NumTexGens));
            for (int i = 0; i < mat.NumTexGens; i++)
            {
                if (mat.TexGenInfos[i] == null)
                    continue;

                TexCoordGen texGen = mat.TexGenInfos[i];
                string texGenSrc, texGenFn, matrix;

                switch (texGen.Source)
                {
                    case GXTexGenSrc.Position: texGenSrc = "RawPosition"; break;
                    case GXTexGenSrc.Normal: texGenSrc = "RawNormal"; break;
                    case GXTexGenSrc.Color0: texGenSrc = "Color0"; break;
                    case GXTexGenSrc.Color1: texGenSrc = "Color1"; break;
                    case GXTexGenSrc.Tex0: texGenSrc = "RawTex0"; break; // Should Tex0 be TEXTURE 0? Or is it TEX0 = Input TEX0, while TEXCOORD0 = Output TEX0?
                    case GXTexGenSrc.Tex1: texGenSrc = "RawTex1"; break;
                    case GXTexGenSrc.Tex2: texGenSrc = "RawTex2"; break;
                    case GXTexGenSrc.Tex3: texGenSrc = "RawTex3"; break;
                    case GXTexGenSrc.Tex4: texGenSrc = "RawTex4"; break;
                    case GXTexGenSrc.Tex5: texGenSrc = "RawTex5"; break;
                    case GXTexGenSrc.Tex6: texGenSrc = "RawTex6"; break;
                    case GXTexGenSrc.Tex7: texGenSrc = "RawTex7"; break;
                    case GXTexGenSrc.TexCoord0: texGenSrc = "Tex0"; break;
                    case GXTexGenSrc.TexCoord1: texGenSrc = "Tex1"; break;
                    case GXTexGenSrc.TexCoord2: texGenSrc = "Tex2"; break;
                    case GXTexGenSrc.TexCoord3: texGenSrc = "Tex3"; break;
                    case GXTexGenSrc.TexCoord4: texGenSrc = "Tex4"; break;
                    case GXTexGenSrc.TexCoord5: texGenSrc = "Tex5"; break;
                    case GXTexGenSrc.TexCoord6: texGenSrc = "Tex6"; break;
                   
                    case GXTexGenSrc.Tangent: 
                    case GXTexGenSrc.Binormal:
                    default:
                        Console.WriteLine("[TEVShaderGen] Unsupported TexGenSrc: {0}, defaulting to TEXCOORD0.", texGen.Source);
                        texGenSrc = "Tex0";
                        break;
                }

                if(texGen.TexMatrixSource == GXTexMatrix.Identity)
                {
                    switch (texGen.Type)
                    {
                        case GXTexGenType.Matrix2x4: 
                            stream.AppendLine(string.Format("    Tex{0} = vec3({1}.xy, 0);", i, texGenSrc)); 
                            break;
                        case GXTexGenType.Matrix3x4:
                            stream.AppendLine(string.Format("    float3 uvw = {0}.xyz;", texGenSrc));
                            stream.AppendLine(string.Format("    Tex{0} = vec3((uvw / uvw.z).xy,0);", i));
                            break;
                        case GXTexGenType.SRTG:
                            stream.AppendLine(string.Format("    Tex{0} = vec3({1}.rg, 0);", i, texGenSrc));
                            break;

                        case GXTexGenType.Bump0:
                        case GXTexGenType.Bump1:
                        case GXTexGenType.Bump2:
                        case GXTexGenType.Bump3:
                        case GXTexGenType.Bump4:
                        case GXTexGenType.Bump5:
                        case GXTexGenType.Bump6:
                        case GXTexGenType.Bump7:
                        default:
                            Console.WriteLine("[TEVShaderGen] Unsupported TexMatrixSource: {0}, Defaulting to Matrix2x4", texGen.TexMatrixSource);
                            stream.AppendLine(string.Format("    Tex{0} = vec3({1}.xy, 0);", i, texGenSrc));
                            break;
                    }
                }
                else
                {
                    // Convert to TexMtx0 to TexMtx9
                    int matIndex = ((int)texGen.TexMatrixSource - 30) / 3;
                    switch (texGen.Type)
                    {
                        default:
                            Console.WriteLine("[TEVShaderGen] Unsupported TexMatrixSource");
                            break;
                    }
                }
            }

            stream.AppendLine("}");
            stream.AppendLine();

            // Compile the Vertex Shader and return whether it compiled sucesfully or not.
            System.IO.File.WriteAllText("ShaderDump/" + mat.Name + "_vert_output", stream.ToString());
            return shader.CompileSource(stream.ToString(), OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
        }

        private static string GetLightCalcString(ChanCtrl chanInfo, bool alpha)
        {
            // If the lighting channel is disabled, the material color for that channel will be passed through unmodified.
            if (!chanInfo.Enable)
                return "1f";

            //string matColorSrc = (chanInfo.MaterialSrc == GXColorSrc.Vertex) ? "RawColor" : "matColor";

            // Up to 8 possible lights.
            for (int i = 0; i < 8; i++)
            {
                if (((int)chanInfo.LitMask & (int)(GXLightId)i) != 0)
                {
                    // I don't think this is right
                }
            }

            if (chanInfo.LitMask == 0)
                return "0.0f";
            else
                return "0.5f";

            // Pre-lighting - hardware-computed local diffuse lighting with baked vertex lighting.
            // lit_color = pre_lit_color * (ambient_scale + diffuse_scale * other_attenuation * diffuse_lit_color)
            // ambient_scale + diffuse_scale = 1.0;
            // When no diffuse light is present, the color is equal to the ambient pre-lit color (pre_lit_color * ambient_scale).
            // When a light is shining on an object, the percentage of pre_lit_color is increased until, at brightest, the full value of pre_lit_color is used.
            // This requires some calculation based on the locaton in the level to nearby point lights.

            // Specular lighting - if if chanInfo.AttenuationFunction is set to GXAttenuationFn.Spec.
            // See GX.pdf - pg 62

            /*switch (chanInfo.DiffuseFunction)
            {
                case GXDiffuseFn.None:
                    break;
                case GXDiffuseFn.Signed:
                    break;
                case GXDiffuseFn.Clamp:
                    break;
                default:
                    break;
            }

            switch (chanInfo.AttenuationFunction)
            {
                case GXAttenuationFn.None:
                    break;
                case GXAttenuationFn.Spec:
                    break;
                case GXAttenuationFn.Spot:
                    break;
                default:
                    break;
            }*/
        }
    }
}

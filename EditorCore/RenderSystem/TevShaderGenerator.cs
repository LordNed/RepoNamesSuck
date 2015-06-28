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
                WLog.Warning(LogCategory.ShaderCompiler, shader, "Failed to generate shader for material {0}", fromMat.Name);
                // ToDo: Generate stub-shader here that expects Pos/UV and single texture.
                return shader;
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

            for (int i = 0; i < mat.NumChannelControls; i++)
                stream.AppendLine(string.Format("in vec4 Color{0};", i));

            for (int texGen = 0; texGen < mat.NumTexGens; texGen++)
                stream.AppendLine(string.Format("in vec3 Tex{0};", texGen));

            stream.AppendLine();

            // Final Output
            stream.AppendLine("// Final Output");
            stream.AppendLine("out vec4 PixelColor;");

            // Texture Inputs
            for (int i = 0; i < 8; i++)
            {
                if (mat.Textures[i] == null)
                    continue;

                stream.AppendLine(string.Format("uniform sampler2D Texture{0}", i));
            }

            // Main Function
            stream.AppendLine("void main()");
            stream.AppendLine("{");

            // Default initial values of the TEV registers.
            // ToDo: Does this need swizzling? themikelester has it marked as mat.registerColor[i==0?3:i-1]]
            stream.AppendLine("    // Initial TEV Register Values");
            stream.AppendLine(string.Format("    vec4 {0} = vec4({1}, {2}, {3}, {4});", "prev", mat.TevColor[0].R, mat.TevColor[0].G, mat.TevColor[0].B, mat.TevColor[0].A));
            stream.AppendLine(string.Format("    vec4 {0} = vec4({1}, {2}, {3}, {4});", "color0", mat.TevColor[1].R, mat.TevColor[1].G, mat.TevColor[1].B, mat.TevColor[1].A));
            stream.AppendLine(string.Format("    vec4 {0} = vec4({1}, {2}, {3}, {4});", "color1", mat.TevColor[2].R, mat.TevColor[2].G, mat.TevColor[2].B, mat.TevColor[2].A));
            stream.AppendLine(string.Format("    vec4 {0} = vec4({1}, {2}, {3}, {4});", "color2", mat.TevColor[3].R, mat.TevColor[3].G, mat.TevColor[3].B, mat.TevColor[3].A));
            stream.AppendLine();

            // Constant Color Registers
            stream.AppendLine("    // Konst TEV Colors");
            for (int i = 0; i < 4; i++)
            {
                stream.AppendLine(string.Format("    vec4 konst{0} = vec4({0}, {1}, {2}, {3});", i, mat.TevKonstColors[i].R, mat.TevKonstColors[i].G, mat.TevKonstColors[i].B, mat.TevKonstColors[i].A));
            }
            stream.AppendLine();

            // Texture Samples
            bool[] oldCombos = new bool[256];
            for (int i = 0, j = 0; i < mat.NumTevStages; i++)
            {
                TevOrder order = mat.TevOrderInfos[i];
                int tex = order.TexMap;
                GXTexCoordSlot coord = order.TexCoordId;

                // This TEV probably doesn't use textures.
                if (tex == 0xFF || coord == GXTexCoordSlot.Null)
                    continue;

                if (IsNewTexCombo(tex, (int)coord, oldCombos))
                {
                    string swizzle = ""; // Uhh I don't know if we need to swizzle since everyone's been converted into ARGB
                    stream.AppendLine(string.Format("    vec4 texCol{0} = texture(Texture{0}, {1}){2};", tex, coord, swizzle));
                }
            }
            stream.AppendLine();

            // ToDo: Implement indirect texturing.
            stream.AppendLine("    // TEV Stages");
            stream.AppendLine();
            stream.AppendLine();

            for (int i = 0; i < mat.NumTevStages; i++)
            {
                stream.AppendLine(string.Format("    // TEV Stage {0}", i));
                TevOrder order = mat.TevOrderInfos[i];
                TevStage stage = mat.TevStageInfos[i];

                TevSwapMode swap = mat.TevSwapModes[i];
                TevSwapModeTable rasTable = mat.TevSwapModeTables[swap.RasSel];
                TevSwapModeTable texTable = mat.TevSwapModeTables[swap.TexSel];

                // There's swapping involved in the ras table.
                stream.AppendLine(string.Format("    // Rasterization Swap Table: {0}", rasTable));
                if (!(rasTable.R == 0 && rasTable.G == 1 && rasTable.B == 2 && rasTable.A == 3))
                {
                    stream.AppendLine(string.Format("    {0} = {1}{2};", GetVertColorString(order), GetVertColorString(order), GetSwapModeSwizzleString(rasTable)));
                }
                stream.AppendLine();


                // There's swapping involved in the texture table.
                stream.AppendLine(string.Format("    // Texture Swap Table: {0}", texTable));
                if (!(texTable.R == 0 && texTable.G == 1 && texTable.B == 2 && texTable.A == 3))
                {
                    stream.AppendLine(string.Format("    {0} = {1}{2};", GetTexTapString(order), GetTexTapString(order), GetSwapModeSwizzleString(rasTable)));
                }
                stream.AppendLine();

                /*string[] colorInputs = new string[4];
                colorInputs[0] = GetColorInString(mat.TevColor[0], mat.KonstColorSels[i], order);
                colorInputs[1] = GetColorInString(stage.ColorOp, mat.KonstColorSels[i], order);
                colorInputs[2] = GetColorInString(stage.ColorBias, mat.KonstColorSels[i], order);
                colorInputs[3] = GetColorInString(stage.ColorScale, mat.KonstColorSels[i], order);

                stream.AppendLine("    // Color and Alpha Operations");
                stream.AppendLine(string.Format("    {0}", GetColorOpString(stage.ColorOp, stage.ColorBias, stage.ColorScale, stage.ColorClamp, colorInputs)));

                string[] alphaInputs = new string[4];
                alphaInputs[0] = GetAlphaInString(stage.AlphaIn[0], mat.KonstAlphaSels[i], order);
                alphaInputs[1] = GetAlphaInString(stage.AlphaIn[0], mat.KonstAlphaSels[i], order);
                alphaInputs[2] = GetAlphaInString(stage.AlphaIn[0], mat.KonstAlphaSels[i], order);
                alphaInputs[3] = GetAlphaInString(stage.AlphaIn[0], mat.KonstAlphaSels[i], order);

                stream.AppendLine(string.Format("    {0}", GetAlphaOpString(stage.AlphaOp, stage.AlphaBias, stage.AlphaScale, stage.AlphaClamp, alphaInputs)));
                stream.AppendLine();*/
            }
            stream.AppendLine();

            // Alpha Compare
            stream.AppendLine("    // Alpa Compare Op");
            AlphaCompare alphaCompare = mat.AlphaCompare;
            string alphaOp;
            switch (alphaCompare.Operation)
            {
                case GXAlphaOp.And: alphaOp = "&&"; break;
                case GXAlphaOp.Or: alphaOp = "||"; break;
                case GXAlphaOp.XOR: alphaOp = "^"; break; // Not really tested, unsupported in some examples but I don't see why.
                case GXAlphaOp.XNOR: alphaOp = "=="; break;  // Not really tested. ^
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unsupported alpha compare operation: {0}", alphaCompare.Operation);
                    alphaOp = "||";
                    break;
            }

            // clip(result.a < 0.5 && result a > 0.2 ? -1 : 1)
            string ifContents = string.Format("({0} {1} {2})",
                GetCompareString(alphaCompare.Comp0, "result.a", alphaCompare.Reference0),
                alphaOp,
                GetCompareString(alphaCompare.Comp1, "result.a", alphaCompare.Reference1));

            // clip equivelent
            stream.AppendLine("    // Clip");
            stream.AppendLine("    if{0}\n\t\tdiscard");

            stream.AppendLine("    PixelColor = result;");

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

            for (int i = 0; i < mat.NumChannelControls; i++)
                stream.AppendLine(string.Format("out vec4 Color{0};", i));

            for (int texGen = 0; texGen < mat.NumTexGens; texGen++)
            {
                if (mat.TexGenInfos[texGen] != null)
                    stream.AppendLine(string.Format("out vec3 Tex{0};", texGen));
            }

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
            //if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color0))
            //  stream.AppendLine("    Color0 = RawColor0;");
            //if (mat.VtxDesc.AttributeIsEnabled(ShaderAttributeIds.Color1))
            //stream.AppendLine("    Color1 = RawColor1;");

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
                        WLog.Warning(LogCategory.TEVShaderGenerator, shader, "Unknown vertex output color channel {0}, skipping.", chanSel);
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
                        WLog.Warning(LogCategory.TEVShaderGenerator, shader, "Unsupported TexGenSrc: {0}, defaulting to TEXCOORD0.", texGen.Source);
                        texGenSrc = "Tex0";
                        break;
                }

                if (texGen.TexMatrixSource == GXTexMatrix.Identity)
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
                            WLog.Warning(LogCategory.TEVShaderGenerator, shader, "Unsupported TexMatrixSource: {0}, Defaulting to Matrix2x4", texGen.TexMatrixSource);
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
                            WLog.Warning(LogCategory.TEVShaderGenerator, shader, "Unsupported TexMatrixSource");
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

        private static bool IsNewTexCombo(int texMap, int texCoordId, bool[] oldCombos)
        {
            int index = (texMap << 4 | texCoordId);
            if (oldCombos[index])
                return false;

            oldCombos[index] = true;
            return true;
        }

        private static string GetVertColorString(TevOrder orderInfo)
        {
            switch (orderInfo.ChannelId)
            {
                case GXColorChannelId.Color0: return "Color0.rgb";
                case GXColorChannelId.Color1: return "Color1.rgb";
                case GXColorChannelId.Alpha0: return "Color0.aaaa";
                case GXColorChannelId.Alpha1: return "Color1.aaaa";
                case GXColorChannelId.Color0A0: return "Color0.rgba";
                case GXColorChannelId.Color1A1: return "Color1.rgba";
                case GXColorChannelId.ColorZero: return "0.rrrr";
                case GXColorChannelId.AlphaBump:
                case GXColorChannelId.AlphaBumpN:
                case GXColorChannelId.ColorNull:
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unsupported ChannelId: {0}", orderInfo.ChannelId);
                    return "vec4(0.0, 1.0, 0.0, 1.0)";
            }
        }

        private static string GetSwapModeSwizzleString(TevSwapModeTable table)
        {
            return string.Format(".{0}{1}{2}{3}", table.R, table.G, table.B, table.A);
        }

        private static string GetTexTapString(TevOrder info)
        {
            return string.Format("texCol{0}{1}", info.TexMap, (int)info.TexCoordId);
        }

        private static string GetCompareString(GXCompareType compare, string a, byte refVal)
        {
            string outStr = "";
            float fRef = refVal / 255f;

            if (compare != GXCompareType.Always)
                WLog.Warning(LogCategory.TEVShaderGenerator, null, "Untested alpha-test functionality: {0}", compare);

            switch (compare)
            {
                case GXCompareType.Never: outStr = "false"; break;
                case GXCompareType.Less: outStr = "<"; break;
                case GXCompareType.Equal: outStr = "=="; break;
                case GXCompareType.LEqual: outStr = "<="; break;
                case GXCompareType.Greater: outStr = ">"; break;
                case GXCompareType.NEqual: outStr = "!="; break;
                case GXCompareType.GEqual: outStr = ">="; break;
                case GXCompareType.Always: outStr = "true"; break;
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Invalid comparison function, defaulting to always.");
                    outStr = "true";
                    break;
            }

            if (string.Compare(outStr, "false") == 0 || string.Compare(outStr, "true") == 0)
                return outStr;

            return string.Format("{0} {1} {2}", a, outStr, fRef);
        }

        private static string GetColorInString(GXCombineColorInput inputType, GXKonstColorSel konst, TevOrder texMapping)
        {
            switch (inputType)
            {
                case GXCombineColorInput.ColorPrev: return "result.rgb";
                case GXCombineColorInput.AlphaPrev: return "result.aaa";
                case GXCombineColorInput.C0: return "color0.rgb";
                case GXCombineColorInput.A0: return "color0.aaa";
                case GXCombineColorInput.C1: return "color1.rgb";
                case GXCombineColorInput.A1: return "color1.aaa";
                case GXCombineColorInput.C2: return "color2.rgb";
                case GXCombineColorInput.A2: return "color2.aaa";
                case GXCombineColorInput.TexColor: return GetTexTapString(texMapping) + ".rgb";
                case GXCombineColorInput.TexAlpha: return GetTexTapString(texMapping) + ".aaa";
                case GXCombineColorInput.RasColor: return GetVertColorString(texMapping) + ".rgb";
                case GXCombineColorInput.RasAlpha: return GetVertColorString(texMapping) + ".aaa";
                case GXCombineColorInput.One: return "1.0f.rrr";
                case GXCombineColorInput.Half: return "0.5f.rrr";
                case GXCombineColorInput.Konst: return GetKonstColorString(konst) + ".rgb";
                case GXCombineColorInput.Zero: return "0.0f.rrr";
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unknown Color Input type: {0}", inputType);
                    return "0.0f.rrr";
            }
        }

        private static string GetAlphaInString(GXCombineAlphaInput inputType, GXKonstAlphaSel konst, TevOrder texMapping)
        {
            switch (inputType)
            {
                case GXCombineAlphaInput.AlphaPrev: return "result.a";
                case GXCombineAlphaInput.A0: return "color0.a";
                case GXCombineAlphaInput.A1: return "color1.a";
                case GXCombineAlphaInput.A2: return "color2.a";
                case GXCombineAlphaInput.TexAlpha: return GetTexTapString(texMapping) + ".a";
                case GXCombineAlphaInput.RasAlpha: return GetVertColorString(texMapping) + ".a";
                case GXCombineAlphaInput.Konst: return GetKonstAlphaString(konst) + ".a";
                case GXCombineAlphaInput.Zero: return "0.0f";
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unknown Alpha Input type: {0}", inputType);
                    return "0.0f";
            }
        }

        private static string GetKonstAlphaString(GXKonstAlphaSel konst)
        {
            switch (konst)
            {
                case GXKonstAlphaSel.KASel_1: return "1.0";
                case GXKonstAlphaSel.KASel_7_8: return "0.875";
                case GXKonstAlphaSel.KASel_3_4: return "0.75";
                case GXKonstAlphaSel.KASel_5_8: return "0.625";
                case GXKonstAlphaSel.KASel_1_2: return "0.5";
                case GXKonstAlphaSel.KASel_3_8: return "0.375";
                case GXKonstAlphaSel.KASel_1_4: return "0.25";
                case GXKonstAlphaSel.KASel_1_8: return "0.125";
                case GXKonstAlphaSel.KASel_K0_R: return "konst0.r";
                case GXKonstAlphaSel.KASel_K1_R: return "konst1.r";
                case GXKonstAlphaSel.KASel_K2_R: return "konst2.r";
                case GXKonstAlphaSel.KASel_K3_R: return "konst3.r";
                case GXKonstAlphaSel.KASel_K0_G: return "konst0.g";
                case GXKonstAlphaSel.KASel_K1_G: return "konst1.g";
                case GXKonstAlphaSel.KASel_K2_G: return "konst2.g";
                case GXKonstAlphaSel.KASel_K3_G: return "konst3.g";
                case GXKonstAlphaSel.KASel_K0_B: return "konst0.b";
                case GXKonstAlphaSel.KASel_K1_B: return "konst1.b";
                case GXKonstAlphaSel.KASel_K2_B: return "konst2.b";
                case GXKonstAlphaSel.KASel_K3_B: return "konst3.b";
                case GXKonstAlphaSel.KASel_K0_A: return "konst0.a";
                case GXKonstAlphaSel.KASel_K1_A: return "konst1.a";
                case GXKonstAlphaSel.KASel_K2_A: return "konst2.a";
                case GXKonstAlphaSel.KASel_K3_A: return "konst3.a";
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unsupported GXKonstAlphaSel: {0}, returning 1.0", konst);
                    return "1.0";
            }
        }

        private static string GetKonstColorString(GXKonstColorSel konst)
        {
            switch (konst)
            {
                case GXKonstColorSel.KCSel_1: return "1.0.rrrr";
                case GXKonstColorSel.KCSel_7_8: return "0.875.rrrr";
                case GXKonstColorSel.KCSel_3_4: return "0.75.rrrr";
                case GXKonstColorSel.KCSel_5_8: return "0.625.rrrr";
                case GXKonstColorSel.KCSel_1_2: return "0.5.rrrr";
                case GXKonstColorSel.KCSel_3_8: return "0.375.rrrr";
                case GXKonstColorSel.KCSel_1_4: return "0.25.rrrr";
                case GXKonstColorSel.KCSel_1_8: return "0.125.rrrr";
                case GXKonstColorSel.KCSel_K0: return "konst0.rgba";
                case GXKonstColorSel.KCSel_K1: return "konst1.rgba";
                case GXKonstColorSel.KCSel_K2: return "konst2.rgba";
                case GXKonstColorSel.KCSel_K3: return "konst3.rgba";
                case GXKonstColorSel.KCSel_K0_R: return "konst0.rrrr";
                case GXKonstColorSel.KCSel_K1_R: return "konst1.rrrr";
                case GXKonstColorSel.KCSel_K2_R: return "konst2.rrrr";
                case GXKonstColorSel.KCSel_K3_R: return "konst3.rrrr";
                case GXKonstColorSel.KCSel_K0_G: return "konst0.gggg";
                case GXKonstColorSel.KCSel_K1_G: return "konst1.gggg";
                case GXKonstColorSel.KCSel_K2_G: return "konst2.gggg";
                case GXKonstColorSel.KCSel_K3_G: return "konst3.gggg";
                case GXKonstColorSel.KCSel_K0_B: return "konst0.bbbb";
                case GXKonstColorSel.KCSel_K1_B: return "konst1.bbbb";
                case GXKonstColorSel.KCSel_K2_B: return "konst2.bbbb";
                case GXKonstColorSel.KCSel_K3_B: return "konst3.bbbb";
                case GXKonstColorSel.KCSel_K0_A: return "konst0.aaaa";
                case GXKonstColorSel.KCSel_K1_A: return "konst1.aaaa";
                case GXKonstColorSel.KCSel_K2_A: return "konst2.aaaa";
                case GXKonstColorSel.KCSel_K3_A: return "konst3.aaaa";
                default:
                    WLog.Warning(LogCategory.TEVShaderGenerator, null, "Unsupported GXKonstColorSel: {0}, returning 1.0", konst);
                    return "1.0";
            }
        }
    }
}

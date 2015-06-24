using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders.J3D
{
    public static class MAT3Loader
    {
        /// <summary> Delegate defines a function that decodes one instance of type T.</summary>
        /// <param name="stream">The stream to decode the instance from</param>
        private delegate T LoadFromStream<T>(EndianBinaryReader stream);

        private static List<T> Collect<T>(EndianBinaryReader stream, LoadFromStream<T> function, int count)
        {
            List<T> values = new List<T>();
            for (int i = 0; i < count; i++)
            {
                values.Add(function(stream));
            }

            return values;
        }

        private static List<T> ReadSection<T>(EndianBinaryReader stream, long offset, long nextOffset, LoadFromStream<T> function, int itemSize)
        {
            stream.BaseStream.Position = offset;
            return Collect<T>(stream, function, (int)(nextOffset - offset) / itemSize);
        }

        #region Stream Decoding Functions
        private static Color ReadColor32(EndianBinaryReader stream)
        {
            return new Color(stream.ReadByte() / 255f, stream.ReadByte() / 255f, stream.ReadByte() / 255f, stream.ReadByte() / 255f);
        }

        private static Color ReadColorShort(EndianBinaryReader stream)
        {
            // ToDo: Are these actually just divided by 255f? Wouldn't they be divided by short.MaxValue?
            return new Color(stream.ReadInt16() / 255f, stream.ReadInt16() / 255f, stream.ReadInt16() / 255f, stream.ReadInt16() / 255f);
        }

        private static ZMode ReadZMode(EndianBinaryReader stream)
        {
            var retVal = new ZMode
            {
                Enable = stream.ReadBoolean(),
                Function = (GXCompareType) stream.ReadByte(),
                UpdateEnable = stream.ReadBoolean(),
            };

            stream.ReadByte(); // Padding
            return retVal;
        }

        private static AlphaCompare ReadAlphaCompare(EndianBinaryReader stream)
        {
            var retVal = new AlphaCompare
            {
                Comp0 = (GXCompareType)stream.ReadByte(),
                Reference0 = stream.ReadByte(),
                Operation = (GXAlphaOp) stream.ReadByte(),
                Comp1 = (GXCompareType)stream.ReadByte(),
                Reference1 = stream.ReadByte()
            };

            stream.ReadBytes(3); // Padding
            return retVal;
        }

        private static BlendMode ReadBlendMode(EndianBinaryReader stream)
        {
            return new BlendMode
            {
                Type = (GXBlendMode) stream.ReadByte(),
                SourceFact = (GXBlendModeControl) stream.ReadByte(),
                DestinationFact = (GXBlendModeControl) stream.ReadByte(),
                Operation = stream.ReadByte()
            };
        }

        private static ChannelControl ReadChannelControl(EndianBinaryReader stream)
        {
            var retVal = new ChannelControl
            {
                Enable = stream.ReadBoolean(),
                MaterialSrc = stream.ReadByte(),
                LitMask = stream.ReadByte(),
                DiffuseFunction = stream.ReadByte(),
                AttenuationFunction = stream.ReadByte(),
                AmbientSource = stream.ReadByte()
            };

            stream.ReadBytes(2); // Padding
            return retVal;
        }

        private static TexCoordGen ReadTexCoordGen(EndianBinaryReader stream)
        {
            var retVal = new TexCoordGen
            {
                Type = stream.ReadByte(),
                Source = stream.ReadByte(),
                TexMatrix = stream.ReadByte()
            };

            stream.ReadByte(); // Padding
            return retVal;
        }

        private static TexMatrix ReadTexMatrix(EndianBinaryReader stream)
        {
            var retVal = new TexMatrix();
            retVal.Projection = stream.ReadByte();
            retVal.Type = stream.ReadByte();
            stream.ReadUInt16(); // Padding
            retVal.CenterS = stream.ReadSingle();
            retVal.CenterT = stream.ReadSingle();
            retVal.Unknown0 = stream.ReadSingle();
            retVal.ScaleS = stream.ReadSingle();
            retVal.ScaleT = stream.ReadSingle();
            retVal.Rotation = stream.ReadInt16() * (180 / 32768f);
            stream.ReadUInt16(); // Padding
            retVal.TranslateS = stream.ReadSingle();
            retVal.TranslateT = stream.ReadSingle();
            retVal.PreMatrix = new float[4, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    retVal.PreMatrix[x, y] = stream.ReadSingle();
                }
            }

            return retVal;
        }

        private static TevIn ReadTevIn(EndianBinaryReader stream)
        {
            return new TevIn { A = stream.ReadByte(), B = stream.ReadByte(), C = stream.ReadByte(), D = stream.ReadByte() };
        }

        private static TevOp ReadTevOp(EndianBinaryReader stream)
        {
            return new TevOp
            {
                Operation = stream.ReadByte(),
                Bias = stream.ReadByte(),
                Scale = stream.ReadByte(),
                Clamp = stream.ReadByte(),
                Out = stream.ReadByte()
            };
        }

        private static TevOrder ReadTevOrder(EndianBinaryReader stream)
        {
            var retVal = new TevOrder
            {
                TexCoordId = stream.ReadByte(),
                TexMap = stream.ReadByte(),
                ChannelId = stream.ReadByte()
            };

            stream.ReadByte(); // Padding
            return retVal;
        }

        private static TevCombinerStage ReadTevCombinerStage(EndianBinaryReader stream)
        {
            var retVal = new TevCombinerStage
            {
                Unknown0 = stream.ReadByte(),
                ColorIn = stream.ReadByte(),
                ColorOp = stream.ReadByte(),
                ColorBias = stream.ReadByte(),
                ColorScale = stream.ReadByte(),
                ColorClamp = stream.ReadByte(),
                ColorRegId = stream.ReadByte(),
                AlphaIn = stream.ReadByte(),
                AlphaOp = stream.ReadByte(),
                AlphaBias = stream.ReadByte(),
                AlphaScale = stream.ReadByte(),
                AlphaClamp = stream.ReadByte(),
                AlphaRegId = stream.ReadByte(),
                Unknown1 = stream.ReadByte()
            };

            return retVal;
        }

        private static TevSwapMode ReadTevSwapMode(EndianBinaryReader stream)
        {
            var retVal = new TevSwapMode
            {
                RasSel = stream.ReadByte(),
                TexSel = stream.ReadByte()
            };

            stream.ReadBytes(2); // Padding
            return retVal;
        }

        private static TevSwapModeTable ReadTevSwapModeTable(EndianBinaryReader stream)
        {
            return new TevSwapModeTable
                {
                    R = stream.ReadByte(),
                    G = stream.ReadByte(),
                    B = stream.ReadByte(),
                    A = stream.ReadByte()
                };
        }

        private static FogInfo ReadFogInfo(EndianBinaryReader stream)
        {
            var retVal = new FogInfo();

            retVal.Type = stream.ReadByte();
            retVal.Enable = stream.ReadBoolean();
            retVal.Center = stream.ReadUInt16();
            retVal.StartZ = stream.ReadSingle();
            retVal.EndZ = stream.ReadSingle();
            retVal.NearZ = stream.ReadSingle();
            retVal.FarZ = stream.ReadSingle();
            retVal.Color = ReadColor32(stream);
            retVal.Table = new ushort[10];
            for (int i = 0; i < retVal.Table.Length; i++)
                retVal.Table[i] = stream.ReadUInt16();

            return retVal;
        }

        private static int ReadInt32(EndianBinaryReader stream)
        {
            return stream.ReadInt32();
        }

        private static byte ReadByte(EndianBinaryReader stream)
        {
            return stream.ReadByte();
        }

        private static short ReadShort(EndianBinaryReader stream)
        {
            return stream.ReadInt16();
        }
        #endregion

        public static List<WEditor.Common.Nintendo.J3D.Material> LoadMAT3SectionFromStream(EndianBinaryReader reader, long chunkStart, List<ushort> indexToMaterialIndex)
        {
            short materialCount = reader.ReadInt16();
            short padding = reader.ReadInt16();
            int materialsOffset = reader.ReadInt32();
            int materialIndexOffset = reader.ReadInt32();
            int stringTableOffset = reader.ReadInt32(); // Name Offset
            int indirectTexturingOffset = reader.ReadInt32();
            int gxCullModeOffset = reader.ReadInt32();
            int materialColorOffset = reader.ReadInt32(); // gxColorMaterial Color
            int numColorChanOffset = reader.ReadInt32();
            int colorChanInfoOffset = reader.ReadInt32();
            int ambientColorOffset = reader.ReadInt32(); // Ambient Color
            int lightInfoOffset = reader.ReadInt32(); //
            int texGenNumberOffset = reader.ReadInt32(); // numTexGens
            int texCoordInfoOffset = reader.ReadInt32(); // TexCoordGen Offset
            int texCoordInfo2Offset = reader.ReadInt32();
            int texMatrixInfoOffset = reader.ReadInt32();
            int texMatrixInfo2Offset = reader.ReadInt32();
            int texTableOffset = reader.ReadInt32(); // Texture Offset (?)
            int tevOrderInfoOffset = reader.ReadInt32();
            int tevColorOffset = reader.ReadInt32(); // gxColorS10
            int tevKColorOffset = reader.ReadInt32(); // gxColor
            int tevStageNumInfoOffset = reader.ReadInt32();
            int tevStageInfoOffset = reader.ReadInt32(); // Tev Combiner
            int tevSwapModeInfoOffset = reader.ReadInt32();
            int tevSwapModeTableInfoOffset = reader.ReadInt32();
            int fogInfoOffset = reader.ReadInt32();
            int alphaCompareInfoOffset = reader.ReadInt32();
            int blendInfoOffset = reader.ReadInt32();
            int zModeInfoOffset = reader.ReadInt32();
            int ditherInfoOffset = reader.ReadInt32();
            int nbtScaleInfoOffset = reader.ReadInt32();


            List<Common.Nintendo.J3D.Material> materialList = new List<Material>();

            // Read the materialIndexOffset section into our outRemapIndexes since there's a weird extra level of redirection here.
            reader.BaseStream.Position = chunkStart + materialIndexOffset;
            for (int i = 0; i < materialCount; i++)
            {
                indexToMaterialIndex.Add(reader.ReadUInt16());
            }

            /* STRING TABLE */
            reader.BaseStream.Position = chunkStart + stringTableOffset;
            StringTable nameTable = StringTable.FromStream(reader);

            /* INDIRECT TEXTURING */
            // ???????

            /* CULL MODE */
            List<int> cullModes = ReadSection<int>(reader, chunkStart + gxCullModeOffset, chunkStart + materialColorOffset, ReadInt32, 4);

            /* MATERIAL COLOR */
            List<Color> materialColors = ReadSection<Color>(reader, chunkStart + materialColorOffset, chunkStart + numColorChanOffset, ReadColor32, 4);

            /* NUM COLOR CHAN */
            // THIS IS A GUESS AT DATA TYPE
            List<byte> numChannelControls = ReadSection<byte>(reader, chunkStart + numColorChanOffset, chunkStart + colorChanInfoOffset, ReadByte, 1);

            /* COLOR CHAN INFO */
            List<ChannelControl> colorChannelInfos = ReadSection<ChannelControl>(reader, chunkStart + colorChanInfoOffset, chunkStart + ambientColorOffset, ReadChannelControl, 8);

            /* AMBIENT COLOR */
            List<Color> ambientColors = ReadSection<Color>(reader, chunkStart + ambientColorOffset, chunkStart + lightInfoOffset, ReadColor32, 4);

            /* LIGHT INFO */
            // THIS IS A GUESS AT DATA TYPE
            var lightingColors = ReadSection<Color>(reader, chunkStart + lightInfoOffset, chunkStart + texGenNumberOffset, ReadColorShort, 8);

            /* TEX GEN NUMBER */
            // THIS IS A GUESS AT DATA TYPE
            var numTexGens = ReadSection<byte>(reader, chunkStart + texGenNumberOffset, chunkStart + texCoordInfoOffset, ReadByte, 1);

            /* TEX GEN INFO */
            var texGenInfos = ReadSection<TexCoordGen>(reader, chunkStart + texCoordInfoOffset, chunkStart + texCoordInfo2Offset, ReadTexCoordGen, 4);

            /* TEX GEN 2 INFO */
            var texGen2Infos = ReadSection<TexCoordGen>(reader, chunkStart + texCoordInfo2Offset, chunkStart + texMatrixInfoOffset, ReadTexCoordGen, 4);

            /* TEX MATRIX INFO */
            var texMatrixInfo = ReadSection<TexMatrix>(reader, chunkStart + texMatrixInfoOffset, chunkStart + texMatrixInfo2Offset, ReadTexMatrix, 100);

            /* POST TRANSFORM MATRIX INFO */
            var texMatrix2Info = ReadSection<TexMatrix>(reader, chunkStart + texMatrixInfo2Offset, chunkStart + texTableOffset, ReadTexMatrix, 100);

            /* TEXURE INDEX */
            var texIndexes = ReadSection<short>(reader, chunkStart + texTableOffset, chunkStart + tevOrderInfoOffset, ReadShort, 2);

            /* TEV ORDER INFO */
            var tevOrderInfos = ReadSection<TevOrder>(reader, chunkStart + tevOrderInfoOffset, chunkStart + tevColorOffset, ReadTevOrder, 4);

            /* TEV COLORS */
            var tevColors = ReadSection<Color>(reader, chunkStart + tevColorOffset, chunkStart + tevKColorOffset, ReadColorShort, 8);

            /* TEV KONST COLORS */
            var tevKonstColors = ReadSection<Color>(reader, chunkStart + tevKColorOffset, chunkStart + tevStageNumInfoOffset, ReadColor32, 4);

            /* NUM TEV STAGES */
            // THIS IS A GUESS AT DATA TYPE
            var numTevStages = ReadSection<byte>(reader, chunkStart + tevStageNumInfoOffset, chunkStart + tevStageInfoOffset, ReadByte, 1);

            /* TEV STAGE INFO */
            var tevStageInfos = ReadSection<TevCombinerStage>(reader, chunkStart + tevStageInfoOffset, chunkStart + tevSwapModeInfoOffset, ReadTevCombinerStage, 20);

            /* TEV SWAP MODE INFO */
            var tevSwapModeInfos = ReadSection<TevSwapMode>(reader, chunkStart + tevSwapModeInfoOffset, chunkStart + tevSwapModeTableInfoOffset, ReadTevSwapMode, 4);

            /* TEV SWAP MODE TABLE INFO */
            var tevSwapModeTables = ReadSection<TevSwapModeTable>(reader, chunkStart + tevSwapModeTableInfoOffset, chunkStart + fogInfoOffset, ReadTevSwapModeTable, 4);

            /* FOG INFO */ 
            // THIS IS A GUESS AT DATA TYPE
            var fogInfos = ReadSection<FogInfo>(reader, chunkStart + fogInfoOffset, chunkStart + alphaCompareInfoOffset, ReadFogInfo, 44);

            /* ALPHA COMPARE INFO */
            var alphaCompares = ReadSection<AlphaCompare>(reader, chunkStart + alphaCompareInfoOffset, chunkStart + blendInfoOffset, ReadAlphaCompare, 4);
            
            /* BLEND INFO */
            List<BlendMode> blendModeInfos = ReadSection<BlendMode>(reader, chunkStart + blendInfoOffset, chunkStart + zModeInfoOffset, ReadBlendMode, 4);

            /* ZMODE INFO */
            List<ZMode> zModeInfos = ReadSection<ZMode>(reader, chunkStart + zModeInfoOffset, chunkStart + ditherInfoOffset, ReadZMode, 4);

            /* DITHER INFO */
            // ????

            /* NBT SCALE INFO */
            // ????


            for (int m = 0; m < materialCount; m++)
            {
                // A Material entry is 0x14c long.
                reader.BaseStream.Position = chunkStart + materialsOffset + (m * 0x14c);

                // Now that we've read the contents of the material section, we can load their values
                // into a material class which keeps it nice and tidy and full of class references
                // and not indexes.
                WEditor.Common.Nintendo.J3D.Material material = new WEditor.Common.Nintendo.J3D.Material();
                materialList.Add(material);

                material.Flag = reader.ReadByte();
                material.CullMode = (GXCullMode)cullModes[reader.ReadByte()];
                material.NumChannelControls = numChannelControls[reader.ReadByte()];
                material.NumTexGens = numTexGens[reader.ReadByte()];
                material.NumTevStages = numTevStages[reader.ReadByte()];
                material.ZCompareLocIndex = reader.ReadByte();
                material.ZMode = zModeInfos[reader.ReadByte()];
                material.DitherIndex = reader.ReadByte();

                // Not sure what these materials are used for. gxColorMaterial is the function that reads them.
                material.MaterialColors = new Color[2];
                for (int i = 0; i < material.MaterialColors.Length; i++)
                    material.MaterialColors[i] = materialColors[reader.ReadInt16()];

                material.ChannelControls = new ChannelControl[4];
                for (int i = 0; i < material.ChannelControls.Length; i++)
                    material.ChannelControls[i] = colorChannelInfos[reader.ReadInt16()];

                material.AmbientColors = new Color[2];
                for (int i = 0; i < material.AmbientColors.Length; i++)
                    material.AmbientColors[i] = ambientColors[reader.ReadInt16()];

                material.LightingColors = new Color[8];
                for (int i = 0; i < material.LightingColors.Length; i++)
                {
                    // Index will be -1 if there's no Lighting Colors on this material.
                    short index = reader.ReadInt16();
                    if(index >= 0)
                        material.LightingColors[i] = lightingColors[index]; 
                }

                material.TexGenInfos = new TexCoordGen[8];
                for (int i = 0; i < material.TexGenInfos.Length; i++)
                {
                    // Index will be -1 if there's no Tex Gens on this material.
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TexGenInfos[i] = texGenInfos[index];
                }

                material.TexGen2Infos = new TexCoordGen[8];
                for (int i = 0; i < material.TexGen2Infos.Length; i++)
                    material.TexGen2Infos[i] = texGenInfos[reader.ReadInt16()];

                material.TexMatrices = new TexMatrix[10];
                for (int i = 0; i < material.TexMatrices.Length; i++)
                    material.TexMatrices[i] = texMatrixInfo[reader.ReadInt16()];

                material.DttMatrices = new TexMatrix[20];
                for (int i = 0; i < material.DttMatrices.Length; i++)
                    material.DttMatrices[i] = texMatrix2Info[reader.ReadInt16()];

                material.Textures = new short[8];
                for (int i = 0; i < material.Textures.Length; i++)
                    material.Textures[i] = texIndexes[reader.ReadInt16()];

                material.TevKonstColors = new Color[4];
                for (int i = 0; i < material.TevKonstColors.Length; i++)
                    material.TevKonstColors[i] = tevKonstColors[reader.ReadInt16()];

                // Guessing that this one doesn't index anything else as it's just an enum value and there doesn't seem to be an offset for it in the header.
                material.KonstColorSels = new GXKonstColorSel[16];
                for (int i = 0; i < material.KonstColorSels.Length; i++)
                    material.KonstColorSels[i] = (GXKonstColorSel)reader.ReadInt16();

                // Guessing that this one doesn't index anything else as it's just an enum value and there doesn't seem to be an offset for it in the header.
                material.KonstAlphaSels = new GXKonstAlphaSel[16];
                for (int i = 0; i < material.KonstAlphaSels.Length; i++)
                    material.KonstAlphaSels[i] = (GXKonstAlphaSel) reader.ReadInt16();

                material.TevOrderInfos = new TevOrder[16];
                for (int i = 0; i < material.TevOrderInfos.Length; i++)
                    material.TevOrderInfos[i] = tevOrderInfos[reader.ReadInt16()];

                material.TevColor = new Color[4];
                for (int i = 0; i < material.TevColor.Length; i++)
                    material.TevColor[i] = tevColors[reader.ReadInt16()];

                material.TevStageInfos = new TevCombinerStage[16];
                for (int i = 0; i < material.TevStageInfos.Length; i++)
                    material.TevStageInfos[i] = tevStageInfos[reader.ReadInt16()];

                material.TevSwapModes = new TevSwapMode[16];
                for (int i = 0; i < material.TevSwapModes.Length; i++)
                    material.TevSwapModes[i] = tevSwapModeInfos[reader.ReadInt16()];

                material.TevSwapModeTables = new TevSwapModeTable[16];
                for (int i = 0; i < material.TevSwapModeTables.Length; i++)
                    material.TevSwapModeTables[i] = tevSwapModeTables[reader.ReadInt16()];

                material.UnknownIndexes = new short[12];
                for (int l = 0; l < material.UnknownIndexes.Length; l++)
                    material.UnknownIndexes[l] = reader.ReadInt16();

                material.FogIndex = reader.ReadInt16();
                material.AlphaCompare = alphaCompares[reader.ReadInt16()];
                material.BlendMode = blendModeInfos[reader.ReadInt16()];
                material.UnknownIndex2 = reader.ReadInt16();
            }

            return materialList;
        }
    }
}

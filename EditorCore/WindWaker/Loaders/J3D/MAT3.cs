using GameFormatReader.Common;
using System.Collections.Generic;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        /// <summary> Delegate defines a function that decodes one instance of type T.</summary>
        /// <param name="stream">The stream to decode the instance from</param>
        private delegate T LoadFromStream<T>(EndianBinaryReader stream);

        /* MAT 3 HEADER INFO
         * The variable names are far more descriptive than using an int[] offset list. However, Nintendo
         * appears to store unused indexes as zeros, which means that we have to write a function that
         * searches for the next non-zero offset. We thus have to convert it from specifically named
         * variables to an int[] so we can iterate over it. The original variable names have been replicated
         * below for clarity/reference.
         * 
         * 
         * 
            00 - int materialsOffset = reader.ReadInt32();
            01 - int materialIndexOffset = reader.ReadInt32();
            02 - int stringTableOffset = reader.ReadInt32(); // Name Offset
            03 - int indirectTexturingOffset = reader.ReadInt32();
            04 - int gxCullModeOffset = reader.ReadInt32();
            05 - int materialColorOffset = reader.ReadInt32(); // gxColorMaterial Color
            06 - int numColorChanOffset = reader.ReadInt32();
            07 - int colorChanInfoOffset = reader.ReadInt32();
            08 - int ambientColorOffset = reader.ReadInt32(); // Ambient Color
            09 - int lightInfoOffset = reader.ReadInt32(); //
            10 - int texGenNumberOffset = reader.ReadInt32(); // numTexGens
            11 - int texCoordInfoOffset = reader.ReadInt32(); // TexCoordGen Offset
            12 - int texCoordInfo2Offset = reader.ReadInt32();
            13 - int texMatrixInfoOffset = reader.ReadInt32();
            14 - int texMatrixInfo2Offset = reader.ReadInt32();
            15 - int texTableOffset = reader.ReadInt32(); // Texture Offset (?)
            16 - int tevOrderInfoOffset = reader.ReadInt32();
            17 - int tevColorOffset = reader.ReadInt32(); // gxColorS10
            18 - int tevKColorOffset = reader.ReadInt32(); // gxColor
            19 - int tevStageNumInfoOffset = reader.ReadInt32();
            20 - int tevStageInfoOffset = reader.ReadInt32(); // Tev Combiner
            21 - int tevSwapModeInfoOffset = reader.ReadInt32();
            22 - int tevSwapModeTableInfoOffset = reader.ReadInt32();
            23 - int fogInfoOffset = reader.ReadInt32();
            24 - int alphaCompareInfoOffset = reader.ReadInt32();
            25 - int blendInfoOffset = reader.ReadInt32();
            26 - int zModeInfoOffset = reader.ReadInt32();
            27 - int zCompLocOffset 
            28 - int ditherInfoOffset = reader.ReadInt32();
            29 - int nbtScaleInfoOffset = reader.ReadInt32();
         * 
         */

        private static List<T> Collect<T>(EndianBinaryReader stream, LoadFromStream<T> function, int count)
        {
            List<T> values = new List<T>();
            for (int i = 0; i < count; i++)
            {
                values.Add(function(stream));
            }

            return values;
        }

        private static List<T> ReadSection<T>(EndianBinaryReader stream, long chunkStart, int chunkSize, int[] offsets, int offset, LoadFromStream<T> function, int itemSize)
        {
            stream.BaseStream.Position = chunkStart + offsets[offset];
            return Collect<T>(stream, function, GetOffsetLength(offsets, offset, chunkSize) / itemSize);
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
                Function = (GXCompareType)stream.ReadByte(),
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
                Operation = (GXAlphaOp)stream.ReadByte(),
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
                Type = (GXBlendMode)stream.ReadByte(),
                SourceFact = (GXBlendModeControl)stream.ReadByte(),
                DestinationFact = (GXBlendModeControl)stream.ReadByte(),
                Operation = (GXLogicOp)stream.ReadByte()
            };
        }

        private static ChanCtrl ReadChannelControl(EndianBinaryReader stream)
        {
            var retVal = new ChanCtrl
            {
                Enable = stream.ReadBoolean(),
                MaterialSrc = (GXColorSrc) stream.ReadByte(),
                LitMask = (GXLightId) stream.ReadByte(),
                DiffuseFunction = (GXDiffuseFn) stream.ReadByte(),
                AttenuationFunction = (GXAttenuationFn) stream.ReadByte(),
                AmbientSrc = (GXColorSrc) stream.ReadByte()
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

        private static bool ReadBool(EndianBinaryReader stream)
        {
            return stream.ReadBoolean();
        }
        #endregion

        public static List<WEditor.Common.Nintendo.J3D.Material> LoadMAT3SectionFromStream(EndianBinaryReader reader, long chunkStart, int chunkSize, List<ushort> indexToMaterialIndex)
        {
            short materialCount = reader.ReadInt16();
            short padding = reader.ReadInt16();

            // Nintendo sets unused offsets to zero, so we can't just use the next variable name in the list. Instead we have to search
            // until we find a non-zero one and calculate the difference that way. Thus, we convert all of the offsets into an int[] for
            // array operations.
            int[] offsets = new int[30];
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = reader.ReadInt32();

            List<Common.Nintendo.J3D.Material> materialList = new List<Material>();

            // Read the materialIndexOffset section into our outRemapIndexes since there's a weird extra level of redirection here.
            reader.BaseStream.Position = chunkStart + offsets[1];
            for (int i = 0; i < materialCount; i++)
            {
                indexToMaterialIndex.Add(reader.ReadUInt16());
            }

            /* STRING TABLE */
            reader.BaseStream.Position = chunkStart + offsets[2];
            StringTable nameTable = StringTable.FromStream(reader);

            /* INDIRECT TEXTURING */
            // ???????

            /* CULL MODE */
            var cullModes = ReadSection<int>(reader, chunkStart, chunkSize, offsets, 4, ReadInt32, 4);

            /* MATERIAL COLOR */
            var materialColors = ReadSection<Color>(reader, chunkStart, chunkSize, offsets, 5, ReadColor32, 4);

            /* NUM COLOR CHAN */
            // THIS IS A GUESS AT DATA TYPE
            var numChannelControls = ReadSection<byte>(reader, chunkStart, chunkSize, offsets, 6, ReadByte, 1);

            /* COLOR CHAN INFO */
            var colorChannelInfos = ReadSection<ChanCtrl>(reader, chunkStart, chunkSize, offsets, 7, ReadChannelControl, 8);

            /* AMBIENT COLOR */
            var ambientColors = ReadSection<Color>(reader, chunkStart, chunkSize, offsets, 8, ReadColor32, 4);

            /* LIGHT INFO */
            // THIS IS A GUESS AT DATA TYPE
            var lightingColors = ReadSection<Color>(reader, chunkStart, chunkSize, offsets, 9, ReadColorShort, 8);

            /* TEX GEN NUMBER */
            // THIS IS A GUESS AT DATA TYPE
            var numTexGens = ReadSection<byte>(reader, chunkStart, chunkSize, offsets, 10, ReadByte, 1);

            /* TEX GEN INFO */
            var texGenInfos = ReadSection<TexCoordGen>(reader, chunkStart, chunkSize, offsets, 11, ReadTexCoordGen, 4);

            /* TEX GEN 2 INFO */
            var texGen2Infos = ReadSection<TexCoordGen>(reader, chunkStart, chunkSize, offsets, 12, ReadTexCoordGen, 4);

            /* TEX MATRIX INFO */
            var texMatrixInfo = ReadSection<TexMatrix>(reader, chunkStart, chunkSize, offsets, 13, ReadTexMatrix, 100);

            /* POST TRANSFORM MATRIX INFO */
            var texMatrix2Info = ReadSection<TexMatrix>(reader, chunkStart, chunkSize, offsets, 14, ReadTexMatrix, 100);

            /* TEXURE INDEX */
            var texIndexes = ReadSection<short>(reader, chunkStart, chunkSize, offsets, 15, ReadShort, 2);

            /* TEV ORDER INFO */
            var tevOrderInfos = ReadSection<TevOrder>(reader, chunkStart, chunkSize, offsets, 16, ReadTevOrder, 4);

            /* TEV COLORS */
            var tevColors = ReadSection<Color>(reader, chunkStart, chunkSize, offsets, 17, ReadColorShort, 8);

            /* TEV KONST COLORS */
            var tevKonstColors = ReadSection<Color>(reader, chunkStart, chunkSize, offsets, 18, ReadColor32, 4);

            /* NUM TEV STAGES */
            // THIS IS A GUESS AT DATA TYPE
            var numTevStages = ReadSection<byte>(reader, chunkStart, chunkSize, offsets, 19, ReadByte, 1);

            /* TEV STAGE INFO */
            var tevStageInfos = ReadSection<TevCombinerStage>(reader, chunkStart, chunkSize, offsets, 20, ReadTevCombinerStage, 20);

            /* TEV SWAP MODE INFO */
            var tevSwapModeInfos = ReadSection<TevSwapMode>(reader, chunkStart, chunkSize, offsets, 21, ReadTevSwapMode, 4);

            /* TEV SWAP MODE TABLE INFO */
            var tevSwapModeTables = ReadSection<TevSwapModeTable>(reader, chunkStart, chunkSize, offsets, 22, ReadTevSwapModeTable, 4);

            /* FOG INFO */
            var fogInfos = ReadSection<FogInfo>(reader, chunkStart, chunkSize, offsets, 23, ReadFogInfo, 44);

            /* ALPHA COMPARE INFO */
            var alphaCompares = ReadSection<AlphaCompare>(reader, chunkStart, chunkSize, offsets, 24, ReadAlphaCompare, 8);

            /* BLEND INFO */
            var blendModeInfos = ReadSection<BlendMode>(reader, chunkStart, chunkSize, offsets, 25, ReadBlendMode, 4);

            /* ZMODE INFO */
            var zModeInfos = ReadSection<ZMode>(reader, chunkStart, chunkSize, offsets, 26, ReadZMode, 4);

            /* ZCOMP LOC INFO */
            // THIS IS A GUESS AT DATA TYPE
            var zCompLocInfos = ReadSection<bool>(reader, chunkStart, chunkSize, offsets, 27, ReadBool, 1);

            /* DITHER INFO */
            // THIS IS A GUESS AT DATA TYPE
            var ditherInfos = ReadSection<bool>(reader, chunkStart, chunkSize, offsets, 28, ReadBool, 1);

            /* NBT SCALE INFO */
            // ????


            for (int m = 0; m < materialCount; m++)
            {
                // A Material entry is 0x14c long.
                reader.BaseStream.Position = chunkStart + offsets[0] + (m * 0x14c);

                // The first byte of a material is some form of flag. Values found so far are 1, 4 and 0. 1 is the most common.
                // bmdview2 documentation says that means "draw on way down" while 4 means "draw on way up" (of INF1 heirarchy)
                // However, none of the documentation seems to mention type 0 - if the value is 0, it seems to be some junk/EOF
                // marker for the material section. On some files (not all) there will be say, 12 materials, but the highest index
                // in the material remap table only goes up to 10 (so the 11th material) and the 12th will never be referenced. However
                // if we read it like we do here with a for loop, we'll hit that one and try to parse all the indexes and it'll just all
                // around kind of explode.
                //
                // To resolve this, we'll check if the flag value is zero - if so, skip creating a material for it.

                byte flag = reader.ReadByte();
                if (flag == 0)
                    continue;

                // Now that we've read the contents of the material section, we can load their values into a material 
                // class which keeps it nice and tidy and full of class references and not indexes.
                WEditor.Common.Nintendo.J3D.Material material = new WEditor.Common.Nintendo.J3D.Material();
                materialList.Add(material);

                material.Name = nameTable[m];
                material.Flag = flag;
                material.CullMode = (GXCullMode)cullModes[reader.ReadByte()];
                material.NumChannelControls = numChannelControls[reader.ReadByte()];
                material.NumTexGens = numTexGens[reader.ReadByte()];
                material.NumTevStages = numTevStages[reader.ReadByte()];
                material.ZCompLoc = zCompLocInfos[reader.ReadByte()];
                material.ZMode = zModeInfos[reader.ReadByte()];
                material.Dither = ditherInfos[reader.ReadByte()];

                // Not sure what these materials are used for. gxColorMaterial is the function that reads them.
                material.MaterialColors = new Color[2];
                for (int i = 0; i < material.MaterialColors.Length; i++)
                {
                    short index = reader.ReadInt16();
                    //if (index >= 0)
                    material.MaterialColors[i] = materialColors[index];
                }

                material.ChannelControls = new ChanCtrl[4];
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
                    if (index >= 0)
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
                {
                    // Index will be -1 if there's no Tex Gen 2s on this material.
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TexGen2Infos[i] = texGenInfos[index];
                }

                material.TexMatrices = new TexMatrix[10];
                for (int i = 0; i < material.TexMatrices.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TexMatrices[i] = texMatrixInfo[index];
                }

                material.DttMatrices = new TexMatrix[20];
                for (int i = 0; i < material.DttMatrices.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.DttMatrices[i] = texMatrix2Info[index];
                }

                material.TextureIndexes = new short[8];
                for (int i = 0; i < material.TextureIndexes.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TextureIndexes[i] = texIndexes[index];
                }

                material.TevKonstColors = new Color[4];
                for (int i = 0; i < material.TevKonstColors.Length; i++)
                    material.TevKonstColors[i] = tevKonstColors[reader.ReadInt16()];

                // Guessing that this one doesn't index anything else as it's just an enum value and there doesn't seem to be an offset for it in the header.
                material.KonstColorSels = new GXKonstColorSel[16];
                for (int i = 0; i < material.KonstColorSels.Length; i++)
                    material.KonstColorSels[i] = (GXKonstColorSel)reader.ReadByte();

                // Guessing that this one doesn't index anything else as it's just an enum value and there doesn't seem to be an offset for it in the header.
                material.KonstAlphaSels = new GXKonstAlphaSel[16];
                for (int i = 0; i < material.KonstAlphaSels.Length; i++)
                    material.KonstAlphaSels[i] = (GXKonstAlphaSel)reader.ReadByte();

                material.TevOrderInfos = new TevOrder[16];
                for (int i = 0; i < material.TevOrderInfos.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TevOrderInfos[i] = tevOrderInfos[index];
                }

                material.TevColor = new Color[4];
                for (int i = 0; i < material.TevColor.Length; i++)
                    material.TevColor[i] = tevColors[reader.ReadInt16()];

                material.TevStageInfos = new TevCombinerStage[16];
                for (int i = 0; i < material.TevStageInfos.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TevStageInfos[i] = tevStageInfos[index];
                }

                material.TevSwapModes = new TevSwapMode[16];
                for (int i = 0; i < material.TevSwapModes.Length; i++)
                {
                    short index = reader.ReadInt16();
                    if (index >= 0)
                        material.TevSwapModes[i] = tevSwapModeInfos[index];
                }

                material.TevSwapModeTables = new TevSwapModeTable[4];
                for (int i = 0; i < material.TevSwapModeTables.Length; i++)
                    material.TevSwapModeTables[i] = tevSwapModeTables[reader.ReadInt16()];

                material.UnknownIndexes = new short[12];
                for (int l = 0; l < material.UnknownIndexes.Length; l++)
                    material.UnknownIndexes[l] = reader.ReadInt16();

                short fogIndex = reader.ReadInt16();
                material.Fog = fogInfos[fogIndex];

                short alphaCompareIndex = reader.ReadInt16();
                material.AlphaCompare = alphaCompares[alphaCompareIndex];
                material.BlendMode = blendModeInfos[reader.ReadInt16()];
                material.UnknownIndex2 = reader.ReadInt16();
            }

            return materialList;
        }

        private static int GetOffsetLength(int[] dataOffsets, int currentIndex, int endChunkOffset)
        {
            int currentOffset = dataOffsets[currentIndex];

            // Find the next available offset in the array, and subtract the two offsets to get the length of the data.
            for (int i = currentIndex + 1; i < dataOffsets.Length; i++)
            {
                if (dataOffsets[i] != 0)
                {
                    return dataOffsets[i] - currentOffset;
                }
            }

            // If we didn't find a dataOffset that was valid, then we go to the end of the chunk.
            return endChunkOffset - currentOffset;
        }
    }
}

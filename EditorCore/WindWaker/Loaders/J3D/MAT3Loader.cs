using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Nintendo.J3D;

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
            for(int i = 0; i < count; i++)
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
                Function = stream.ReadByte(),
                UpdateEnable = stream.ReadBoolean(),
            };

            stream.ReadByte(); // Padding
            return retVal;
        }

        private static AlphaCompare ReadAlphaCompare(EndianBinaryReader stream)
        {
            var retVal = new AlphaCompare
            {
                Compare0 = stream.ReadByte(),
                Reference0 = stream.ReadByte(),
                Operation = stream.ReadByte(),
                Compare1 = stream.ReadByte(),
                Reference1 = stream.ReadByte()
            };

            stream.ReadBytes(3); // Padding
            return retVal;
        }

        private static BlendMode ReadBlendMode(EndianBinaryReader stream)
        {
            return new BlendMode
            {
                Type = stream.ReadByte(),
                SourceFact = stream.ReadByte(),
                DestinationFact = stream.ReadByte(),
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
            retVal.PreMatrix = new float[4,4];
            for(int y = 0; y < 4; y++)
            {
                for(int x = 0; x < 4; x++)
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

        public static void LoadMAT3SectionFromStream(EndianBinaryReader reader, long chunkStart, List<J3DFileResource.MaterialEntry> outMaterials, List<ushort> indexToMaterialIndex, List<short> texStageIndexToTextureIndex)
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


            // Read the materialIndexOffset section into our outRemapIndexes since there's a weird extra level of redirection here.
            reader.BaseStream.Position = chunkStart + materialIndexOffset;
            for (int i = 0; i < materialCount; i++)
            {
                indexToMaterialIndex.Add(reader.ReadUInt16());
            }

            /* STRING TABLE */
            reader.BaseStream.Position = chunkStart + stringTableOffset;
            StringTable nameTable = StringTable.FromStream(reader);

            /* COLOR 1 (MATERIAL COLOR) */
            List<Color> materialColors = ReadSection<Color>(reader, chunkStart + materialColorOffset, chunkStart + numColorChanOffset, ReadColor32, 4);
            List<Color> ambientColors = ReadSection<Color>(reader, chunkStart + ambientColorOffset, chunkStart + lightInfoOffset, ReadColor32, 4);
            List<Color> tevColors = ReadSection<Color>(reader, chunkStart + tevColorOffset, chunkStart + tevKColorOffset, ReadColorShort, 8);
            List<Color> tevKonstColors = ReadSection<Color>(reader, chunkStart + tevKColorOffset, chunkStart + tevStageNumInfoOffset, ReadColor32, 4);
            List<int> cullModes = ReadSection<int>(reader, chunkStart + gxCullModeOffset, chunkStart + materialColorOffset, ReadInt32, 4);
            List<ChannelControl> colorChannelInfo = ReadSection<ChannelControl>(reader, chunkStart + colorChanInfoOffset, chunkStart + ambientColorOffset, ReadChannelControl, 8);
            List<byte> texGenCounts = ReadSection<byte>(reader, chunkStart + texGenNumberOffset, chunkStart + texCoordInfoOffset, ReadByte, 1);
            List<TexCoordGen> texGenInfo = ReadSection<TexCoordGen>(reader, chunkStart + texCoordInfoOffset, chunkStart + texCoordInfo2Offset, ReadTexCoordGen, 4);
            List<TexMatrix> texMatrixInfo = ReadSection<TexMatrix>(reader, chunkStart + texMatrixInfoOffset, chunkStart + texMatrixInfo2Offset, ReadTexMatrix, 100);
            texStageIndexToTextureIndex.AddRange(ReadSection<short>(reader, chunkStart + texTableOffset, chunkStart + tevOrderInfoOffset, ReadShort, 2));
            List<TevOrder> tevOrderInfo = ReadSection<TevOrder>(reader, chunkStart + tevOrderInfoOffset, chunkStart + tevColorOffset, ReadTevOrder, 4);
            List<byte> tevCounts = ReadSection<byte>(reader, chunkStart + tevStageNumInfoOffset, chunkStart + tevStageInfoOffset, ReadByte, 1);
            List<TevCombinerStage> tevStageInfos = ReadSection<TevCombinerStage>(reader, chunkStart + tevStageInfoOffset, chunkStart + tevSwapModeInfoOffset, ReadTevCombinerStage, 20);
            List<TevSwapMode> tevSwapModeInfos = ReadSection<TevSwapMode>(reader, chunkStart + tevSwapModeInfoOffset, chunkStart + tevSwapModeTableInfoOffset, ReadTevSwapMode, 4);
            List<TevSwapModeTable> tevSwapModeTables = ReadSection<TevSwapModeTable>(reader, chunkStart + tevSwapModeTableInfoOffset, chunkStart + fogInfoOffset, ReadTevSwapModeTable, 4);
            List<AlphaCompare> alphaCompares = ReadSection<AlphaCompare>(reader, chunkStart + alphaCompareInfoOffset, chunkStart + blendInfoOffset, ReadAlphaCompare, 4);
            List<BlendMode> blendModeInfos = ReadSection<BlendMode>(reader, chunkStart + blendInfoOffset, chunkStart + zModeInfoOffset, ReadBlendMode, 4);
            List<ZMode> zModeInfos = ReadSection<ZMode>(reader, chunkStart + zModeInfoOffset, chunkStart + ditherInfoOffset, ReadZMode, 4);


            for (int m = 0; m < materialCount; m++)
            {
                // A Material entry is 0x14c long.
                reader.BaseStream.Position = chunkStart + materialsOffset + (m * 0x14c);

                // Start reading each material.
                J3DFileResource.MaterialEntry material = new J3DFileResource.MaterialEntry();
                outMaterials.Add(material);

                material.Unknown1Index = reader.ReadByte();
                material.CullModeIndex = reader.ReadByte();
                material.NumChannelsIndex = reader.ReadByte();
                material.NumTexGensIndex = reader.ReadByte();
                material.NumTevStagesIndex = reader.ReadByte();
                material.ZCompareLocIndex = reader.ReadByte();
                material.ZModeIndex = reader.ReadByte();
                material.DitherIndex = reader.ReadByte();

                material.MaterialColorIndex = new short[2];
                for (int l = 0; l < 2; l++)
                    material.MaterialColorIndex[l] = reader.ReadInt16();

                material.ChannelControlIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.ChannelControlIndex[l] = reader.ReadInt16();

                material.AmbientColorIndex = new short[2];
                for (int l = 0; l < 2; l++)
                    material.AmbientColorIndex[l] = reader.ReadInt16();

                material.LightingIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.LightingIndex[l] = reader.ReadInt16();

                material.TexCoordIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.TexCoordIndex[l] = reader.ReadInt16();

                material.TexCoord2Index = new short[8];
                for (int l = 0; l < 8; l++)
                    material.TexCoord2Index[l] = reader.ReadInt16();

                material.TexMatrixIndex = new short[10];
                for (int l = 0; l < 10; l++)
                    material.TexMatrixIndex[l] = reader.ReadInt16();

                material.TexMatrix2Index = new short[20];
                for (int l = 0; l < 20; l++)
                    material.TexMatrix2Index[l] = reader.ReadInt16();

                material.texIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.texIndex[l] = reader.ReadInt16();

                material.tevConstantColorIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.tevConstantColorIndex[l] = reader.ReadInt16();

                material.constColorSel = new byte[16];
                for (int l = 0; l < 16; l++)
                    material.constColorSel[l] = reader.ReadByte();

                material.constAlphaSel = new byte[16];
                for (int l = 0; l < 16; l++)
                    material.constAlphaSel[l] = reader.ReadByte();

                material.tevOrderIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevOrderIndex[l] = reader.ReadInt16();

                material.tevColorIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.tevColorIndex[l] = reader.ReadInt16();

                material.tevStageInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevStageInfoIndex[l] = reader.ReadInt16();

                material.tevSwapModeInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevSwapModeInfoIndex[l] = reader.ReadInt16();

                material.tevSwapModeTableInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevSwapModeTableInfoIndex[l] = reader.ReadInt16();

                material.unknownIndices = new short[12];
                for (int l = 0; l < 12; l++)
                    material.unknownIndices[l] = reader.ReadInt16();

                material.FogIndex = reader.ReadInt16();
                material.AlphaCompareIndex = reader.ReadInt16();
                material.BlendModeIndex = reader.ReadInt16();
                material.Unknown2Index = reader.ReadInt16();
            }
        }
    }
}

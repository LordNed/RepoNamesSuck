﻿using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTools.yaz0
{
    public static class Yaz0Encoder
    {
        public static EndianBinaryWriter Encode(MemoryStream input)
        {
            // Write 'Yaz0' Header
            EndianBinaryWriter output = new EndianBinaryWriter(new MemoryStream(), Endian.Big);
            output.Write(0x59617A30);

            // Write uncompressed data size.
            output.Write((int)input.Length);

            // Write 8 bytes padding.
            output.Write((long)0);

            EncodeYaz0(input, output);

            return output;
        }

        private static void EncodeYaz0(MemoryStream input, EndianBinaryWriter output)
        {
            byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum per code.
            int dstSize = 0;
            int percent = -1;

            int validBitCount = 0;
            byte curCodeByte = 0;
            int srcPos = 0;
            int dstPos = 0;

            byte[] src = input.ToArray();

            while(srcPos < input.Length)
            {
                int numBytes, matchPos, srcPosBak;
                NintendoYaz0Encode(src, srcPos, out numBytes, out matchPos);

                if(numBytes < 3)
                {
                    // Straight Copy
                    dst[dstPos] = src[srcPos];
                    srcPos++;

                    // Set flag for straight copy
                    curCodeByte |= (byte)(0x80 >> validBitCount);
                }
                else
                {
                    // RLE part
                    int dist = srcPos - matchPos - 1;
                    byte byte1, byte2, byte3;

                    // Requires a 3 byte encoding
                    if(numBytes >= 0x12)
                    {
                        byte1 = (byte)(0 | (dist >> 8));
                        byte2 = (byte)(dist & 0xFF);
                        dst[dstPos++] = byte1;
                        dst[dstPos++] = byte2;

                        // Maximum run length for 3 byte encoding.
                        if (numBytes > 0xFF + 0x12)
                            numBytes = 0xFF + 0x12;
                        byte3 = (byte)(numBytes - 0x12);
                        dst[dstPos++] = byte3;
                    }
                    // 2 byte encoding
                    else
                    {
                        byte1 = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        byte2 = (byte)(dist & 0xFF);
                        dst[dstPos++] = byte1;
                        dst[dstPos++] = byte2;
                    }
                    srcPos += numBytes;
                }

                validBitCount++;
                
                // Write 8 codes if we've filled a block
                if(validBitCount == 8)
                {
                    // Write the code byte 
                    output.Write(curCodeByte);

                    // And then any bytes in the dst buffer.
                    for(int i = 0; i < dstPos; i++)
                        output.Write(dst[i]);
                    
                    dstSize += dstPos + 1;
                    srcPosBak = srcPos;
                    curCodeByte = 0;
                    validBitCount = 0;
                    dstPos = 0;
                }

                if((srcPos + 1) * 100/input.Length != percent)
                {
                    percent = (int)((srcPos + 1) * 100 / input.Length);
                }
            }

            // If we didn't finish off on a whole byte, add the last code byte.
            if (validBitCount > 0)
            {
                // Write the code byte 
                output.Write(curCodeByte);

                // And then any bytes in the dst buffer.
                for (int i = 0; i < dstPos; i++)
                    output.Write(dst[i]);

                dstSize += dstPos + 1;
                curCodeByte = 0;
                validBitCount = 0;
                dstPos = 0;
            }
        }

        static int sNumBytes1, sMatchPos;
        static bool sPrevFlag = false;

        /// <summary>
        /// A look ahead encoding scheme for NGC Yaz0
        /// </summary>
        /// <param name="input"></param>
        /// <param name="srcPos"></param>
        /// <param name="numBytes"></param>
        /// <param name="matchPos"></param>
        private static void NintendoYaz0Encode(byte[] src, int srcPos, out int numBytes, out int matchPos)
        {
            //int startPos = srcPos - 0x1000;
            numBytes = 1;

            // If prevFlag is set, it means that the previous position was determined by the look-ahead try so use
            // that. This is not the best optimization, but apparently Nintendo's choice for speed.
            if(sPrevFlag)
            {
                matchPos = sMatchPos;
                sPrevFlag = false;
                numBytes = sNumBytes1;
                return;
            }

            sPrevFlag = false;
            SimpleRLEEncode(src, srcPos + 1, out numBytes, out sMatchPos);
            matchPos = sMatchPos;

            // If this position is RLE encoded, then compare to copying 1 byte and next pos (srcPos + 1) encoding.
            if(numBytes >= 3)
            {
                SimpleRLEEncode(src, srcPos + 1, out sNumBytes1, out sMatchPos);

                // If the next position encoding is +2 longer than current position, choose it.
                // This does not gurantee the best optimization, but fairly good optimization with speed.
                if(sNumBytes1 >= numBytes+2)
                {
                    numBytes = 1;
                    sPrevFlag = true;
                }
            }
        }

        private static void SimpleRLEEncode(byte[] src, int srcPos, out int numBytes, out int matchPos)
        {
            int startPos = srcPos - 0x1000;
            numBytes = 1;
            matchPos = 0;

            if (startPos < 0)
                startPos = 0;

            // Search backwards through the stream for an already encoded bit.
            for(int i = 0; i < srcPos; i++)
            {
                int j = 0;
                for(; j < src.Length-srcPos; j++)
                {
                    if (src[i + j] != src[j + srcPos])
                        break;
                }

                if(j > numBytes)
                {
                    numBytes = j;
                    matchPos = i;
                }
            }

            if (numBytes == 2)
                numBytes = 1;
        }
    }
}

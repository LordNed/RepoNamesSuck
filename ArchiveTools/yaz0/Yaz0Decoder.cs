using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace ArchiveTools.yaz0
{
    public class Yaz0Decoder
    {
        public EndianBinaryWriter Decode(EndianBinaryReader stream)
        {
            // 16 byte Header
            if (stream.ReadUInt32() != 0x59617A30) // "Yaz0" Magic
            {
                throw new InvalidDataException("Invalid Magic, not a Yaz0 File");
            }

            int uncompressedSize = stream.ReadInt32();
            stream.ReadBytes(8); // Padding


            byte[] output = new byte[uncompressedSize];
            int destPos = 0;

            byte curCodeByte = 0;
            uint validBitCount = 0;

            while (destPos < uncompressedSize)
            {
                // Read the new "code" byte if the current one has been used.
                if(validBitCount == 0)
                {
                    curCodeByte = stream.ReadByte();
                    validBitCount = 8;
                }

                if((curCodeByte & 0x80) != 0)
                {
                    // Straight Copy
                    output[destPos] = stream.ReadByte();
                    destPos++;
                }
                else
                {
                    // RLE Part
                    byte byte1 = stream.ReadByte();
                    byte byte2 = stream.ReadByte();

                    int dist = ((byte1 & 0xF) << 8) | byte2;
                    int copySource = destPos - (dist + 1);

                    int numBytes = byte1 >> 4;
                    if(numBytes == 0)
                    {
                        numBytes = stream.ReadByte() + 0x12;
                    }
                    else
                    {
                        numBytes += 2;
                    }

                    // Copy Run
                    for(int i = 0; i < numBytes; i++)
                    {
                        output[destPos] = output[copySource];
                        copySource++;
                        destPos++;
                    }
                }

                // Use the next bit from the code byte
                curCodeByte <<= 1;
                validBitCount -= 1;

                // The codeByte specifies what to do for the next 8 steps.
                //BitArray codeByte = new BitArray(new byte[] { stream.ReadByte() });

                //for (int i = 0; i < 8; i++)
                //{
                //    bool isSet = codeByte.Get(i);

                //    if (isSet)
                //    {
                //        // If the bit is set then there is no compression, just write the data to the output.
                //        output.Write(stream.ReadByte());
                //    }
                //    else
                //    {
                //        // If the bit is not set, then the data needs to be decompressed. The next two bytes tells the data location and size.
                //        // The decompressed data has already been written to the output stream, so we go and retrieve it.
                //        byte byte1 = stream.ReadByte();
                //        byte byte2 = stream.ReadByte();

                //        int streamMoveDist = ((byte1 & 0xF) << 8) | byte2;
                //        long copySource = output.BaseStream.Position - (streamMoveDist + 1);

                //        int numBytes = byte1 >> 4;
                //        if (numBytes == 0)
                //        {
                //            // Read the third byte which tells you how much data to read.
                //            byte byte3 = stream.ReadByte();
                //            numBytes = byte3 + 0x12;
                //        }
                //        else
                //        {
                //            numBytes += 2;
                //        }

                //        // Copy Run
                //        for (int k = 0; k < numBytes; k++)
                //        {
                //            long outputPos = output.BaseStream.Position;
                //            output.BaseStream.Position = copySource;
                //            byte data = (byte)output.BaseStream.ReadByte();

                //            // Restore the position
                //            output.BaseStream.Position = outputPos;
                //            output.Write(data);
                //            copySource++;
                //        }

                //        //long currentOutputOffset = stream.BaseStream.Position;
                //        //long moveTo = currentOutputOffset - (streamMoveDist + 1);






                //        //// Jump to the position that has the data that we copy from. 
                //        //output.BaseStream.Seek(moveTo, SeekOrigin.Current);
                //        //byte[] dataToCopy = new byte[numBytes];
                //        //for (int k = 0; k < numBytes; k++)
                //        //    dataToCopy[k] = (byte)output.BaseStream.ReadByte();


                //        //todo: fix me.
                //        //    if(dataToCopy.Length < numBytes)
                //        //    {
                //        //        // The data we have read is less than what should have been read, so the read data is repeated until the bytecount is reached.
                //        //        // newCopy = [toCopy]
                //        //        // diff = byteCount - len(toCopy)

                //        //        // Append full copy of the current string to our new copy
                //        //        // for i in range(diff // len(tocopy))
                //        //            // newCopy.append(toCopy)

                //        //        // Append the rest of the copy to the new copy
                //        //        //newCopy.append(dataToCopy[:(diff )])
                //        //    }

                //        //    // Jump the output stream back to where we were originally writing data.
                //        //    output.BaseStream.Position = currentOutputOffset;


                //    }
                //}
            }

            return new EndianBinaryWriter(new MemoryStream(output), Endian.Big);
        }
    }
}

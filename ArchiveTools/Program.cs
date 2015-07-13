using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveTools
{
    class Program
    {
        static void Main(string[] args)
        {
            using(EndianBinaryReader reader = new EndianBinaryReader(File.Open(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage.arc", FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
            {
                yaz0.Yaz0Decoder decoder = new yaz0.Yaz0Decoder();
                var outputStream = decoder.Decode(reader);

                var fileStream = File.Create(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage_decompressed.arc");
                outputStream.BaseStream.Seek(0, SeekOrigin.Begin);
                outputStream.BaseStream.CopyTo(fileStream);
                fileStream.Close();
            }
        }
    }
}

using ArchiveTools.yaz0;
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
            Console.WriteLine("Decompressing stage.arc");
            using(EndianBinaryReader reader = new EndianBinaryReader(File.Open(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage_compressed.arc", FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
            {
                var outputStream = Yaz0Decoder.Decode(reader);

                using(var decompressedArchive = File.Create(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage_decompressed.arc"))
                {
                outputStream.Seek(0, SeekOrigin.Begin);
                outputStream.CopyTo(decompressedArchive);
                decompressedArchive.Close();
                }

            }

            //Console.WriteLine("Compressing stage_decompressed.arc");
            //byte[] data = File.ReadAllBytes(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage_decompressed.arc");
            //var compressedArc = Yaz0Encoder.Encode(new MemoryStream(data));

            //using(var compressedARchive = File.Create(@"E:\New_Data_Drive\WindwakerModding\root\res\Stage\Abesso\stage_compressed.arc"))
            //{
            //    compressedArc.Seek(0, SeekOrigin.Begin);
            //    compressedArc.BaseStream.CopyTo(compressedARchive);
            //    compressedARchive.Close();
            //}
        }
    }
}

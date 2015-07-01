using GameFormatReader.Common;
using System;
using System.ComponentModel;
using System.IO;
namespace WEditor.WindWaker
{
    //public enum FileResourceType
    //{
    //    Collision,
    //    RoomEntityData,
    //    StageEntityData,
    //    BMD,
    //    BDL,
    //    ExternalTexture,
    //    BoneAnimation,
    //    TevRegisterAnimation,
    //    TextureAnimation,
    //    AlternateMaterial,
    //    Unknown
    //}

    public abstract class BaseFileResource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Name of the file (with extension)
        /// </summary>
        public string FileName
        {
            get { return m_fileName; }
            set
            {
                m_fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Archive that this file belongs to.
        /// </summary>
        public ZArchive ParentArchive { get; private set; }

        private string m_fileName;

        public BaseFileResource(string fileName, ZArchive parentArchive)
        {
            FileName = fileName;
            ParentArchive = parentArchive;
        }

        public override string ToString()
        {
            return FileName;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UnsupportedFileResource : BaseFileResource
    {
        public byte[] Data;

        public UnsupportedFileResource(string fileName, ZArchive parentArchive) : base(fileName, parentArchive)
        {

        }

        public override string ToString()
        {
            return string.Format("[UnsupportedFileResource] {0}", base.ToString());
        }
    }

    public static class UnsupportedFileResourceLoader
    {
        public static void Load(UnsupportedFileResource resource, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found ensure");

            // Simply load the contents of the data into the Data array and preserve it for now.
            using(EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open), Endian.Big))
            {
                resource.Data = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
    }
}

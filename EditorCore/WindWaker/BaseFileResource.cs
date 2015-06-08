using System.ComponentModel;
namespace EditorCore.WindWaker
{
    public enum FileResourceType
    {
        Collision,
        RoomEntityData,
        StageEntityData,
        BMD,
        BDL,
        ExternalTexture,
        BoneAnimation,
        TevRegisterAnimation,
        TextureAnimation,
        AlternateMaterial,
        Unknown
    }

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
        /// Name of the folder that this file came from. Used to put the file back into
        /// the folder it came from, regardless of the FileResourceType. Used to preserve
        /// loading / unloading archives.
        /// </summary>
        public string FolderName { get; private set; }

        /// <summary>
        /// Archive that this file belongs to.
        /// </summary>
        public ZArchive ParentArchive { get; private set; }

        private string m_fileName;

        public BaseFileResource(string fileName, string folderName, ZArchive parentArchive)
        {
            FileName = fileName;
            FolderName = folderName;
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

        public UnsupportedFileResource(string fileName, string folderName, ZArchive parentArchive) : base(fileName, folderName, parentArchive)
        {

        }
    }
}

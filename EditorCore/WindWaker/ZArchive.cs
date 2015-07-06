using System.ComponentModel;
using WEditor.FileSystem;
namespace WEditor.WindWaker
{
    public enum ArchiveType 
    {
        Stage, 
        Room 
    }

    /// <summary>
    /// A Zelda Archive (ZArchive for short) describes one (eventual) exported .arc file. It holds a list of the 
    /// the files in the archive.
    /// </summary>
    public class ZArchive : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Name of this archive, generally the folder name on disk it was when archived.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Is this a Room or a Stage
        /// </summary>
        public ArchiveType Type
        {
            get { return m_type; }
            private set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// A virtual filesystem which can contain multiple subfolders/files (recursive).
        /// </summary>
        public VirtualFilesystemDirectory Contents
        {
            get { return m_contents; }
            set
            {
                m_contents = value;
                OnPropertyChanged("Contents");
            }
        }

        private string m_name;
        private ArchiveType m_type;
        private VirtualFilesystemDirectory m_contents;

        public ZArchive(string name, ArchiveType type)
        {
            Name = name;
            Type = type;
            Contents = new VirtualFilesystemDirectory("root", this);
        }

        public override string ToString()
        {
            return Name;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

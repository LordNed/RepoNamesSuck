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

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        public ArchiveType Type
        {
            get { return m_type; }
            private set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        public VirtualFilesystemDirectory Files { get; private set; }

        private string m_name;
        private ArchiveType m_type;

        public ZArchive(string name, ArchiveType type)
        {
            Name = name;
            Type = type;
            Files = new VirtualFilesystemDirectory(Name, this);
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

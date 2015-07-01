using System.Collections.Generic;
using System.ComponentModel;
using WEditor.WindWaker;

namespace WEditor.FileSystem
{
    public enum NodeType
    {
        None,
        Directory,
        File
    }

    public abstract class VirtualFilesystemNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NodeType Type
        {
            get { return m_type; }
            protected set { m_type = value; }
        }
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        private NodeType m_type;
        private string m_name;

        public VirtualFilesystemNode()
        {
            Type = NodeType.None;
            Name = "Unnamed";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VirtualFilesystemDirectory : VirtualFilesystemNode
    {
        public BindingList<VirtualFilesystemNode> Children { get; private set;}

        public VirtualFilesystemDirectory(string name)
        {
            Children = new BindingList<VirtualFilesystemNode>();
            Name = name;
            Type = NodeType.Directory;
        }
    }

    public class VirtualFilesystemFile : VirtualFilesystemNode
    {
        public BaseFileResource File
        {
            get { return m_file; }
            private set
            {
                m_file = value;
                OnPropertyChanged("File");
            }
        }

        public string Extension
        {
            get { return m_extension; }
            private set
            {
                m_extension = value;
                OnPropertyChanged("Extension");
            }
        }

        private BaseFileResource m_file;
        private string m_extension;

        public VirtualFilesystemFile(string name, string extension, BaseFileResource file)
        {
            Name = name;
            Extension = extension;
            File = file;
            Type = NodeType.File;
        }
    }
}

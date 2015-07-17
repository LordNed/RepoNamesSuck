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

        public ZArchive ParentArchive
        {
            get { return m_parentArchive; }
        }

        private NodeType m_type;
        private string m_name;
        private ZArchive m_parentArchive;

        protected VirtualFilesystemNode(ZArchive parentArc)
        {
            Type = NodeType.None;
            Name = "Unnamed";
            m_parentArchive = parentArc;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VirtualFilesystemDirectory : VirtualFilesystemNode
    {
        public BindingList<VirtualFilesystemNode> Children { get; private set; }

        public VirtualFilesystemDirectory(string name, ZArchive parentArc)
            : base(parentArc)
        {
            Children = new BindingList<VirtualFilesystemNode>();
            Name = name;
            Type = NodeType.Directory;
        }

        public List<VirtualFilesystemFile> FindByExtension(string extension)
        {
            return FindByExtension(new string[] { extension });
        }

        public List<VirtualFilesystemFile> FindByExtension(string[] extensions)
        {
            return FindByExtensionRecursive(extensions);
        }

        private List<VirtualFilesystemFile> FindByExtensionRecursive(string[] extensions)
        {
            List<VirtualFilesystemFile> validFiles = new List<VirtualFilesystemFile>();

            foreach(var child in Children)
            {
                if(child.Type == NodeType.File)
                {
                    VirtualFilesystemFile file = (VirtualFilesystemFile)child;
                    foreach(var extension in extensions)
                    {
                        if (string.Compare(file.Extension, extension, System.StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            validFiles.Add(file);
                            break;
                        }
                    }
                }
                else if(child.Type == NodeType.Directory)
                {
                    VirtualFilesystemDirectory dir = (VirtualFilesystemDirectory)child;
                    validFiles.AddRange(dir.FindByExtensionRecursive(extensions));
                }
            }

            return validFiles;
        }
    }

    public class VirtualFilesystemFile : VirtualFilesystemNode
    {
        public VirtualFileContents File;

        public string Extension
        {
            get { return m_extension; }
            private set
            {
                m_extension = value;
                OnPropertyChanged("Extension");
            }
        }

        private string m_extension;

        public VirtualFilesystemFile(string name, string extension, VirtualFileContents file, ZArchive parentArc) : base(parentArc)
        {
            Type = NodeType.File;
            Name = name;
            Extension = extension;
            File = file;
        }
    }

    public class VirtualFileContents
    {
        private byte[] m_data;

        public VirtualFileContents(byte[] data)
        {
            m_data = data;    
        }

        public byte[] GetData()
        {
            return m_data;
        }
    }
}

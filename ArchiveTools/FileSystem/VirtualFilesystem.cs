﻿using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

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
            Name = string.Empty;
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

        public VirtualFilesystemDirectory(string name)
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

        public override string ToString()
        {
            return string.Format("[Dir] {0}", Name);
        }

        public void ExportToDisk(string folder)
        {
            ExportToDiskRecursive(folder, this);
        }

        private void ExportToDiskRecursive(string folder, VirtualFilesystemDirectory dir)
        {
            // Create the directory that this node represents.
            // If it's a directory, append the directory name to the folder and onwards!
            folder = string.Format("{0}{1}/", folder, dir.Name);
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception while trying to create folder {0}: {1}", folder, ex.ToString());
            }

            foreach (var node in dir.Children)
            {
                VirtualFilesystemDirectory vfDir = node as VirtualFilesystemDirectory;
                VirtualFilesystemFile vfFile = node as VirtualFilesystemFile;

                if (vfDir != null)
                {
                    ExportToDiskRecursive(folder, vfDir);
                }
                else if (vfFile != null)
                {
                    // However, if it's a file we're going to write it to disk.
                    string filePath = string.Format("{0}{1}{2}", folder, vfFile.Name, vfFile.Extension);
                    try
                    {
                        using (EndianBinaryWriter writer = new EndianBinaryWriter(File.Create(filePath), Endian.Big))
                        {
                            writer.Write(vfFile.File.GetData());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception while trying to write file {0}: {1}", filePath, ex.ToString());
                    }
                }
            }
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

        public VirtualFilesystemFile(string name, string extension, VirtualFileContents file)
        {
            Type = NodeType.File;
            Name = name;
            Extension = extension;
            File = file;
        }

        public override string ToString()
        {
            return string.Format("[File] 0}", Name);
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
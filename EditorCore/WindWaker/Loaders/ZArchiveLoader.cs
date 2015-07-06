using WEditor.WindWaker.MapEntities;
using System;
using System.IO;
using WEditor.FileSystem;

namespace WEditor.WindWaker.Loaders
{
    public class ZArchiveLoader
    {
        public void LoadFromDirectory(ZArchive intoArchive, string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("folderPath is null or empty!");

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException("folderPath not found, ensure the directory exists first!");

            FillArchiveRecursively(intoArchive.Contents, folderPath);
        }

        private void FillArchiveRecursively(VirtualFilesystemDirectory virDirectory, string folderPath)
        {
            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            foreach(DirectoryInfo folder in folderInfo.GetDirectories())
            {
                var virtualDirectory = new VirtualFilesystemDirectory(folder.Name, virDirectory.ParentArchive);
                virDirectory.Children.Add(virtualDirectory);

                foreach(FileInfo file in folder.GetFiles())
                {
                    // This tries to non-agressively read the file contents (ie: not requiring a lock on it) so hopefully it'll play nicer with having a HexEditor
                    // with the file already open inside of it.
                    using(FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (BinaryReader streamReader = new BinaryReader(fileStream))
                        {
                            byte[] fileContents = streamReader.ReadBytes((int)streamReader.BaseStream.Length);
                           

                            VirtualFileContents vFileContents = new VirtualFileContents(fileContents);
                            string fileName = Path.GetFileNameWithoutExtension(file.Name);
                            var virtualFile = new VirtualFilesystemFile(fileName, file.Extension, vFileContents, virDirectory.ParentArchive);
                            virtualDirectory.Children.Add(virtualFile);
                        }
                    }
                }

                foreach(DirectoryInfo dir in folder.GetDirectories())
                {
                    FillArchiveRecursively(virtualDirectory, dir.FullName);
                }
            }
        }

    }
}

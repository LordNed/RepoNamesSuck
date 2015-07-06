using WEditor.WindWaker.MapEntities;
using System;
using System.IO;
using WEditor.FileSystem;

namespace WEditor.WindWaker.Loaders
{
    public class ZArchiveLoader
    {
        /// <summary>
        /// This looks for appropriate sub-folders (bdl, btk, dzb, dzr, dzs, dat, tex, etc.) and loads each file within it as appropriate.
        /// </summary>
        /// <param name="archive">ZArchive to put the loaded files in.</param>
        /// <param name="folderDirectory">Absolute path to folder on disk which contains sub-folders of the names above.</param>
        [Obsolete]
        public static void Load(Map map, ZArchive archive, string folderDirectory)
        {
            if (string.IsNullOrEmpty(folderDirectory))
                throw new ArgumentException("folderDirectory null or empty");
            if (!Directory.Exists(folderDirectory))
                throw new DirectoryNotFoundException("folderDirectory not found ensure you create the directory on disk first!");

            DirectoryInfo rootFolderInfo = new DirectoryInfo(folderDirectory);
            foreach (DirectoryInfo folder in rootFolderInfo.GetDirectories())
            {
                string folderName = folder.Name;
                var virtualDirectory = new VirtualFilesystemDirectory(folderName, archive);
                archive.Contents.Children.Add(virtualDirectory);

                // Get all of the files within this folder.
                foreach(FileInfo file in folder.GetFiles())
                {
                    // Name of the file with extension
                    string fileName = file.Name;
                    string fileExtension = file.Extension;

                    BaseFileResource resource = null;

                    switch (fileExtension.ToLower())
                    {
                        /* Map Collision Format */
                        case ".dzb":
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loading DZB (Map Collision) {0}...", fileName);
                            resource = new UnsupportedFileResource(fileName, archive);
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loaded {0}.", fileName);
                            break;

                        /* Room and Stage Entity Data */
                        case ".dzr":
                        case ".dzs":
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loading DZR/DZS (Map Entity Data) {0}...", fileName);
                            resource = new MapEntityResource(fileName, archive);
                            MapEntityLoader.Load((MapEntityResource) resource, map, file.FullName);
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loaded {0}.", fileName);
                            break;

                        /* 3D Model Formats */
                        case ".bmd":
                        case ".bdl":
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loading J3D (3D Model) {0}...", fileName);
                            resource = new J3DFileResource(fileName, archive);
                            J3DLoader.Load((J3DFileResource)resource, file.FullName);
                            WLog.Info(LogCategory.ArchiveLoading, archive, "Loaded {0}.", fileName);
                            break;

                        /* Event List */
                        case ".dat":
                        /* External Textures (skybox?) */
                        case ".bti":
                        /* Bone Animation */
                        case ".bck":
                        /* TEV Register Animation */
                        case ".brk":
                        /* Texture Animation */
                        case ".btk":
                        /* Alternate Materials (MAT3 Chunk from BMD/BDL) */
                        case ".bmt":
                            resource = new UnsupportedFileResource(fileName, archive);
                            break;

                        default:
                            WLog.Warning(LogCategory.ArchiveLoading, archive, "Unknown folder type \"{0}\" found ({1}).", folderName, fileName);
                            resource = new UnsupportedFileResource(fileName, archive);
                            UnsupportedFileResourceLoader.Load((UnsupportedFileResource)resource, file.FullName);
                            break;
                    }

                    var virtualFile = new VirtualFilesystemFile(fileName, fileExtension, resource, archive);
                    virtualDirectory.Children.Add(virtualFile);
                }
            }
        }

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
                            var virtualFile = new VirtualFilesystemFile(file.Name, file.Extension, vFileContents, virDirectory.ParentArchive);
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

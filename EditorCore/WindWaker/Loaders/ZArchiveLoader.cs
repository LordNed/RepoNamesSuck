using System;
using System.IO;
namespace EditorCore.WindWaker.Loaders
{
    public static class ZArchiveLoader
    {
        /// <summary>
        /// This looks for appropriate sub-folders (bdl, btk, dzb, dzr, dzs, dat, tex, etc.) and loads each file within it as appropriate.
        /// </summary>
        /// <param name="archive">ZArchive to put the loaded files in.</param>
        /// <param name="folderDirectory">Absolute path to folder on disk which contains sub-folders of the names above.</param>
        public static void Load(ZArchive archive, string folderDirectory)
        {
            if (string.IsNullOrEmpty(folderDirectory))
                throw new ArgumentException("folderDirectory null or empty");
            if (!Directory.Exists(folderDirectory))
                throw new DirectoryNotFoundException("folderDirectory not found ensure you create the directory on disk first!");

            DirectoryInfo rootFolderInfo = new DirectoryInfo(folderDirectory);
            foreach (DirectoryInfo folder in rootFolderInfo.GetDirectories())
            {
                string folderName = folder.Name;

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
                            resource = new UnsupportedFileResource(fileName, folderName, archive);
                            break;

                        /* Room and Stage Entity Data */
                        case ".dzr":
                        case ".dzs":
                            resource = new UnsupportedFileResource(fileName, folderName, archive);
                            break;

                        /* 3D Model Formats */
                        case ".bmd":
                        case ".bdl":
                            resource = new UnsupportedFileResource(fileName, folderName, archive);
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
                            resource = new UnsupportedFileResource(fileName, folderName, archive);
                            break;

                        default:
                            Console.WriteLine("Unknown folder type {0} found ({1}).", folderName, fileName);
                            resource = new UnsupportedFileResource(fileName, folderName, archive);
                            UnsupportedFileResourceLoader.Load((UnsupportedFileResource)resource, file.FullName);
                            break;
                    }

                    archive.Files.Add(resource);
                }
            }
        }
    }
}

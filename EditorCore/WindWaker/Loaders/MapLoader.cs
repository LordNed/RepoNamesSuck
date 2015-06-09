using System;
using System.Collections.Generic;
using System.IO;

namespace EditorCore.WindWaker.Loaders
{
    public static class MapLoader
    {
        /// <summary>
        /// This will create a new Map from an existing directory of rooms and stages on disk.
        /// </summary>
        /// <param name="folderDirectory">Absolute path to folder on disk including the Map name (ie: contents should be Stage.arc and Room*.arc)</param>
        /// <returns></returns>
        public static Map Load(string folderDirectory)
        {
            if (string.IsNullOrEmpty(folderDirectory))
                throw new ArgumentException("folderDirectory null or empty");
            if(!Directory.Exists(folderDirectory))
                throw new DirectoryNotFoundException("folderDirectory not found ensure you create the directory on disk first!");

            // Calculate the Map Name
            DirectoryInfo rootFolderInfo = new DirectoryInfo(folderDirectory);
            string mapName = rootFolderInfo.Name;

            Map newMap = new Map();
            newMap.Name = mapName;
            newMap.ProjectFilePath = Path.GetDirectoryName(folderDirectory);

            var archiveFolderMap = new Dictionary<ZArchive,string>();

            // Scan the folders in this directory and construct ZArchives out of each one. Determine the ZArchive type based on the folder name.
            foreach (var dirInfo in rootFolderInfo.GetDirectories())
            {
                ZArchive archive = null;

                string folderName = dirInfo.Name;
                if(folderName.ToLower().StartsWith("stage"))
                {
                    archive = new ZArchive(ArchiveType.Stage);

                    if (newMap.Stage != null)
                        throw new FileLoadException("folderDirectory contains more than one stage archive!");

                    newMap.Stage = archive;
                }
                else if (folderName.ToLower().StartsWith("room"))
                {
                    archive = new ZArchive(ArchiveType.Room);
                    newMap.Rooms.Add(archive);
                }

                archiveFolderMap[archive] = dirInfo.FullName;
            }

            // Now that we've loaded and created ZArchives for all stage/rooms, we can load the Stage entities, then Room entities.
            if(newMap.Stage != null)
            {
                ZArchiveLoader.Load(newMap, newMap.Stage, archiveFolderMap[newMap.Stage]);
            }

            foreach(var room in newMap.Rooms)
            {
                ZArchiveLoader.Load(newMap, room, archiveFolderMap[room]);
            }

            return newMap;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace WEditor.WindWaker.Loaders
{
    public class MapLoader
    {
        /// <summary>
        /// This will create a new Map from an existing directory of rooms and stages on disk.
        /// </summary>
        /// <param name="folderDirectory">Absolute path to folder on disk including the Map name (ie: contents should be Stage.arc and Room*.arc)</param>
        /// <returns></returns>
        [Obsolete]
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
                    archive = new ZArchive(folderName, ArchiveType.Stage);

                    if (newMap.Stage != null)
                        throw new FileLoadException("folderDirectory contains more than one stage archive!");

                    newMap.Stage = archive;
                }
                else if (folderName.ToLower().StartsWith("room"))
                {
                    archive = new ZArchive(folderName, ArchiveType.Room);
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

        public Map CreateFromDirectory(WWorld world, string folderPath)
        {
            if (world == null)
                throw new ArgumentNullException("No world to load map into specified.");

            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("folderPath is null or empty!");

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException("folderPath not found, ensure the directory exists first!");

            // Calculate the Map Name from the folderPath - it should be the last segment of the folder path.s
            DirectoryInfo rootFolderInfo = new DirectoryInfo(folderPath);
            string mapName = rootFolderInfo.Name;

            ZArchiveLoader archiveLoader = new ZArchiveLoader();


            // Maps are stored in two distinct parts. A Stage which encompasses global data for all rooms, and then
            // one or more rooms. We're going to load both the room and stage into ZArchives and then load the data
            // stored in them into different data.
            var archiveFolderMap = new Dictionary<string, ZArchive>();
            foreach (var dirInfo in rootFolderInfo.GetDirectories())
            {
                ZArchive archive = null;

                string folderName = dirInfo.Name;
                if (folderName.ToLower().StartsWith("stage"))
                {
                    archive = new ZArchive(folderName, ArchiveType.Stage);

                    if(archiveFolderMap.ContainsKey("stage"))
                    {
                        WLog.Warning(LogCategory.EditorCore, null, "{0} contains more than one stage archive, ignoring second...", folderPath);
                        continue;
                    }
                }
                else if (folderName.ToLower().StartsWith("room"))
                {
                    archive = new ZArchive(folderName, ArchiveType.Room);
                }

                // Fill the archives with their contents.
                archiveLoader.LoadFromDirectory(archive, dirInfo.FullName);
                archiveFolderMap[folderName.ToLower()] = archive;
            }

            Map newMap = new Map();
            newMap.Name = mapName;
            newMap.ProjectFilePath = Path.GetDirectoryName(folderPath);

            StageLoader stageLoader = new StageLoader();
            RoomLoader roomLoader = new RoomLoader();

            if(archiveFolderMap.ContainsKey("stage"))
            {
                // Load data from Stage archive into Stage. Stage is loaded first as some of the map entities need to reference
                // things in the stage.
                ZArchive stageArchive = archiveFolderMap["stage"];
                Stage stage = stageLoader.LoadFromArchive(world, stageArchive);

                // newMap.Stage = stage;
            }
            
            foreach(var kvp in archiveFolderMap)
            {
                if(kvp.Key.StartsWith("room"))
                {
                    // Now load data from each Room archive into a Room.
                    //RoomLoader newRoom = roomLoader.LoadFromArchive(world, kvp.Value);

                    // newMap.Rooms.Add(newRoom);
                }
            }


            return newMap;
        }
    }
}

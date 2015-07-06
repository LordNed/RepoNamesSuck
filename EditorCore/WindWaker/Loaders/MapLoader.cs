using System;
using System.Collections.Generic;
using System.IO;

namespace WEditor.WindWaker.Loaders
{
    public class MapLoader
    {
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

            SceneLoader sceneLoader = new SceneLoader();

            // Oof. So Stages can have references to rooms (via MULT chunk) and rooms can have references to other rooms (via PLYR)
            // and it wouldn't surprise me if somewhere, a room references something in a stage. Because of this, we're going to load
            // maps in two passes. The first pass does not resolve the reference (and instead leaves them as their indexes) and then
            // once the stage and all rooms have been loaded, we can then do a second pass and turn the indexes into object references.
            foreach(var kvp in archiveFolderMap)
            {
                if(kvp.Key.StartsWith("room"))
                {
                    Room room = sceneLoader.LoadFromArchive<Room>(world, kvp.Value);
                    newMap.NewRooms.Add(room);
                }
                else if (kvp.Key.StartsWith("stage"))
                {
                    newMap.NewStage = sceneLoader.LoadFromArchive<Stage>(world, kvp.Value);
                }
            }

            // Fix up object-references on map entity data.
            sceneLoader.PostProcessEntityData(newMap);

            // ToDo: Split off some of the data into things associated with each Room vs. Scene vs. in game.

            return newMap;
        }
    }
}

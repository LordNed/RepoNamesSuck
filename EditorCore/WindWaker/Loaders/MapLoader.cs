using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WEditor.Common.Maps;

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
                    newMap.Rooms.Add(room);
                }
                else if (kvp.Key.StartsWith("stage"))
                {
                    newMap.Stage = sceneLoader.LoadFromArchive<Stage>(world, kvp.Value);
                }
            }

            // Fix up object-references on map entity data.
            sceneLoader.PostProcessEntityData(newMap);

            // ToDo: Split off some of the data into things associated with each Room vs. Scene vs. in game.
            if(newMap.Stage != null)
            {
                PostProcessScene(newMap.Stage, world);
            }

            foreach(var room in newMap.Rooms)
            {
                PostProcessScene(room, world);
            }


            return newMap;
        }

        private void PostProcessScene(Scene scene, WWorld world)
        {
            PostProcessPointLights(scene, world);
            PostProcessArrows(scene, world);
            PostProcessSoundSources(scene, world);
            PostProcessShipSpawns(scene, world);
        }

        private void PostProcessShipSpawns(Scene scene, WWorld world)
        {
            var spawnList = FindAllByType("SHIP", scene.Entities);
            for(int i = 0; i < spawnList.Count; i++)
            {
                ShipSpawn shipSpawn = new ShipSpawn
                {
                    Position = (Vector3)spawnList[i]["Position"].Value,
                    YRotation = (short)spawnList[i]["Rotation"].Value,
                    Unknown1 = (short)spawnList[i]["Unknown 1"].Value
                };

                scene.ShipSpawns.Add(shipSpawn);
                world.RegisterObject(shipSpawn);
            }
        }

        private void PostProcessSoundSources(Scene scene, WWorld world)
        {
            var sondList = FindAllByType("SOND", scene.Entities);
            for(int i = 0; i < sondList.Count; i++)
            {
                SoundSource sndSrc = new SoundSource
                {
                    Name = (string)sondList[i]["Name"].Value,
                    Position = (Vector3)sondList[i]["Position"].Value,
                    Unknown1 = (byte)sondList[i]["Unknown 1"].Value,
                    Unknown2 = (byte)sondList[i]["Unknown 2"].Value,
                    Unknown3 = (byte)sondList[i]["Unknown 3"].Value,
                    SoundId = (byte)sondList[i]["Sound ID"].Value,
                    SoundRadius = (byte)sondList[i]["Sound Radius"].Value,
                    Padding1 = (byte)sondList[i]["Padding 1"].Value,
                    Padding2 = (byte)sondList[i]["Padding 2"].Value,
                    Padding3 = (byte)sondList[i]["Padding 3"].Value
                };

                scene.Sounds.Add(sndSrc);
                world.RegisterObject(sndSrc);
            }
        }

        private void PostProcessArrows(Scene scene, WWorld world)
        {
            var arobList = FindAllByType("AROB", scene.Entities);
            for(int i = 0; i < arobList.Count; i++)
            {
                Arrow arrow = new Arrow
                {
                    Position = (Vector3)arobList[i]["Position"].Value,
                    Rotation = (XYZRotation)arobList[i]["Rotation"].Value,
                    Padding = (short)arobList[i]["Padding"].Value,
                };

                scene.AROB.Add(arrow);
                world.RegisterObject(arrow);
            }

            var raroList = FindAllByType("RARO", scene.Entities);
            for (int i = 0; i < raroList.Count; i++)
            {
                Arrow arrow = new Arrow
                {
                    Position = (Vector3)raroList[i]["Position"].Value,
                    Rotation = (XYZRotation)raroList[i]["Rotation"].Value,
                    Padding = (short)raroList[i]["Padding"].Value,
                };

                scene.RARO.Add(arrow);
                world.RegisterObject(arrow);
            }
        }

        private void PostProcessPointLights(Scene scene, WWorld world)
        {
            var lghtList = FindAllByType("LGHT", scene.Entities);
            for(int i = 0; i < lghtList.Count; i++)
            {
                PointLight pointLight = new PointLight
                {
                    Position = (Vector3)lghtList[i]["Position"].Value,
                    Radius = (Vector3)lghtList[i]["Radius"].Value,
                    Color = (Color32)lghtList[i]["Color"].Value
                };

                scene.LGHT.Add(pointLight);
                world.RegisterObject(pointLight);
            }

            var lgtvList = FindAllByType("LGTV", scene.Entities);
            for (int i = 0; i < lgtvList.Count; i++)
            {
                PointLight pointLight = new PointLight
                {
                    Position = (Vector3)lgtvList[i]["Position"].Value,
                    Radius = (Vector3)lgtvList[i]["Radius"].Value,
                    Color = (Color32)lgtvList[i]["Color"].Value
                };

                scene.LGTV.Add(pointLight);
                world.RegisterObject(pointLight);
            }
        }
        
        private List<MapEntityData> FindAllByType(string fourCC, BindingList<MapEntityData> fromList)
        {
            List<MapEntityData> results = new List<MapEntityData>();
            foreach (var item in fromList)
            {
                if (string.Compare(item.FourCC, fourCC, StringComparison.InvariantCultureIgnoreCase) == 0)
                    results.Add(item);
            }

            return results;
        }
    }
}

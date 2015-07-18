using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WEditor.Common.Maps;
using WEditor.FileSystem;
using WEditor.Maps;

namespace WEditor.WindWaker.Loaders
{
    public class MapLoader
    {
        public Map CreateFromDirectory(WWorld world, string folderPath)
        {
            if (world == null)
                throw new ArgumentNullException("world", "No world to load map into specified.");

            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("folderPath is null or empty!");

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException("folderPath not found, ensure the directory exists first!");

            // Calculate the Map Name from the folderPath - it should be the last segment of the folder path.s
            DirectoryInfo rootFolderInfo = new DirectoryInfo(folderPath);
            string mapName = rootFolderInfo.Name;


            // Sort the directories in rootFolderInfo into natural order, instead of alphabetical order which solves issues
            // where room indexes were getting remapped to the wrong one.
            IEnumerable<DirectoryInfo> subFolders = rootFolderInfo.GetDirectories().OrderByNatural(x => x.Name);
            IEnumerable<FileInfo> subFiles = rootFolderInfo.GetFiles().OrderByNatural(x => x.Name);

            // Maps are stored in two distinct parts. A Stage which encompasses global data for all rooms, and then
            // one or more rooms. We're going to load both the room and stage into ZArchives and then load the data
            // stored in them into different data.
            var archiveFolderMap = new Dictionary<string, VirtualFilesystemDirectory>();
            foreach (var dirInfo in subFolders)
            {
                VirtualFilesystemDirectory archive = null;

                string folderName = dirInfo.Name;
                if (folderName.ToLower().StartsWith("stage"))
                {
                    archive = new VirtualFilesystemDirectory(folderName);

                    if (archiveFolderMap.ContainsKey("stage"))
                    {
                        WLog.Warning(LogCategory.EditorCore, null, "{0} contains more than one stage archive, ignoring second...", folderPath);
                        continue;
                    }
                }
                else if (folderName.ToLower().StartsWith("room"))
                {
                    archive = new VirtualFilesystemDirectory(folderName);
                }

                // sea has LOD folders which don't have the right sub-folder setup, boo. This skips them for now,
                // maybe later we can add an ArchiveType.LOD.
                if (archive == null)
                    continue;

                // Fill the archives with their contents.
                archive.ImportFromDisk(dirInfo.FullName);
                archiveFolderMap[folderName.ToLower()] = archive;
            }

            // We're also going to try and process the files inside the folder to see if they're archives.
            foreach (var fileInfo in subFiles)
            {
                VirtualFilesystemDirectory archive = WArchiveTools.ArcUtilities.LoadArchive(fileInfo.FullName);

                // File wasn't a valid RARC archive.
                if (archive == null)
                    continue;

                if (archive.Name.ToLower().StartsWith("stage"))
                {
                    if (archiveFolderMap.ContainsKey("stage"))
                    {
                        WLog.Warning(LogCategory.EditorCore, null, "{0} contains more than one stage archive, ignoring second...", folderPath);
                        continue;
                    }
                }

                string arcName = Path.GetFileNameWithoutExtension(fileInfo.FullName).ToLower();
                archiveFolderMap[arcName] = archive;
            }

            Map newMap = new Map();
            newMap.Name = mapName;
            newMap.ProjectFilePath = Path.GetDirectoryName(folderPath);

            SceneLoader sceneLoader = new SceneLoader();

            // Oof. So Stages can have references to rooms (via MULT chunk) and rooms can have references to other rooms (via PLYR)
            // and it wouldn't surprise me if somewhere, a room references something in a stage. Because of this, we're going to load
            // maps in two passes. The first pass does not resolve the reference (and instead leaves them as their indexes) and then
            // once the stage and all rooms have been loaded, we can then do a second pass and turn the indexes into object references.
            foreach (var kvp in archiveFolderMap)
            {
                if (kvp.Key.StartsWith("room"))
                {
                    Room room = sceneLoader.LoadFromArchive<Room>(world, kvp.Value);
                    room.Name = kvp.Key;
                    newMap.Rooms.Add(room);
                }
                else if (kvp.Key.StartsWith("stage"))
                {
                    newMap.Stage = sceneLoader.LoadFromArchive<Stage>(world, kvp.Value);
                    newMap.Stage.Name = "Stage";
                }
            }

            // Fix up object-references on map entity data.
            sceneLoader.PostProcessEntityData(newMap);

            // ToDo: Split off some of the data into things associated with each Room vs. Scene vs. in game.
            if (newMap.Stage != null)
            {
                PostProcessStage(newMap.Stage, world);
            }

            foreach (var room in newMap.Rooms)
            {
                PostProcessRoom(room, world);
            }


            return newMap;
        }

        private static void PostProcessScene(Scene scene, WWorld world)
        {
            PostProcessPointLights(scene, world);
            PostProcessArrows(scene, world);
            PostProcessSoundSources(scene, world);
            PostProcessShipSpawns(scene, world);
        }

        private static void PostProcessRoom(Room room, WWorld world)
        {
            PostProcessScene(room, world);
        }

        private static void PostProcessStage(Stage stage, WWorld world)
        {
            // We're going to extract the information from the MULT chunk and
            // apply it to the rooms so they have the correct offset.
            var multList = FindAllByType("MULT", stage.Entities);
            foreach (var entry in multList)
            {
                Vector2 translation = entry.GetProperty<Vector2>("Translation");
                float yRotation = (float)(entry.GetProperty<short>("Rotation")) / 32768f * 180;
                byte unknown1 = entry.GetProperty<byte>("Unknown 1");

                Room room = entry.GetProperty<Room>("Room");

                if (room != null)
                {
                    room.Translation = new Vector3(translation.X, 0, translation.Y);
                    room.YRotation = yRotation;
                    room.MULT_Unknown1 = unknown1;
                }
            }

            PostProcessScene(stage, world);
        }

        private static void PostProcessShipSpawns(Scene scene, WWorld world)
        {
            var spawnList = FindAllByType("SHIP", scene.Entities);
            for (int i = 0; i < spawnList.Count; i++)
            {
                ShipSpawn shipSpawn = new ShipSpawn();
                shipSpawn.Transform.Position = spawnList[i].GetProperty<Vector3>("Position");

                float yRotation = spawnList[i].GetProperty<short>("Rotation") / 32768f * 180;
                shipSpawn.Transform.Rotation = new Quaternion(0f, yRotation, 0f, 0f);

                shipSpawn.ShipId = spawnList[i].GetProperty<byte>("Ship Id");
                shipSpawn.Unknown1 = spawnList[i].GetProperty<byte>("Unknown 1");

                scene.ShipSpawns.Add(shipSpawn);
                world.RegisterObject(shipSpawn);
            }
        }

        private static void PostProcessSoundSources(Scene scene, WWorld world)
        {
            var sondList = FindAllByType("SOND", scene.Entities);
            for (int i = 0; i < sondList.Count; i++)
            {
                SoundSource sndSrc = new SoundSource();

                sndSrc.Name = sondList[i].GetProperty<string>("Name");
                sndSrc.Transform.Position = sondList[i].GetProperty<Vector3>("Position");
                sndSrc.Unknown1 = sondList[i].GetProperty<byte>("Unknown 1");
                sndSrc.Unknown2 = sondList[i].GetProperty<byte>("Unknown 2");
                sndSrc.Unknown3 = sondList[i].GetProperty<byte>("Unknown 3");
                sndSrc.SoundId = sondList[i].GetProperty<byte>("Sound ID");
                sndSrc.SoundRadius = sondList[i].GetProperty<byte>("Sound Radius");
                sndSrc.Padding1 = sondList[i].GetProperty<byte>("Padding 1");
                sndSrc.Padding2 = sondList[i].GetProperty<byte>("Padding 2");
                sndSrc.Padding3 = sondList[i].GetProperty<byte>("Padding 3");

                scene.Sounds.Add(sndSrc);
                world.RegisterObject(sndSrc);
            }
        }

        private static void PostProcessArrows(Scene scene, WWorld world)
        {
            var arobList = FindAllByType("AROB", scene.Entities);
            for (int i = 0; i < arobList.Count; i++)
            {
                Arrow arrow = new Arrow();

                arrow.Transform.Position = arobList[i].GetProperty<Vector3>("Position");
                XYZRotation fullRot = arobList[i].GetProperty<XYZRotation>("Rotation");
                arrow.Transform.Rotation = new Quaternion(fullRot.X, fullRot.Y, fullRot.Z, 0f);
                arrow.Padding = arobList[i].GetProperty<short>("Padding");

                scene.AROB.Add(arrow);
                world.RegisterObject(arrow);
            }

            var raroList = FindAllByType("RARO", scene.Entities);
            for (int i = 0; i < raroList.Count; i++)
            {
                Arrow arrow = new Arrow();

                arrow.Transform.Position = raroList[i].GetProperty<Vector3>("Position");
                XYZRotation fullRot = raroList[i].GetProperty<XYZRotation>("Rotation");
                arrow.Transform.Rotation = new Quaternion(fullRot.X, fullRot.Y, fullRot.Z, 0f);
                arrow.Padding = raroList[i].GetProperty<short>("Padding");


                scene.RARO.Add(arrow);
                world.RegisterObject(arrow);
            }
        }

        private static void PostProcessPointLights(Scene scene, WWorld world)
        {
            var lghtList = FindAllByType("LGHT", scene.Entities);
            for (int i = 0; i < lghtList.Count; i++)
            {
                PointLight pointLight = new PointLight();
                pointLight.Transform.Position = lghtList[i].GetProperty<Vector3>("Position");
                pointLight.Radius = lghtList[i].GetProperty<Vector3>("Radius");
                pointLight.Color = lghtList[i].GetProperty<Color32>("Color");

                scene.LGHT.Add(pointLight);
                world.RegisterObject(pointLight);
            }

            var lgtvList = FindAllByType("LGTV", scene.Entities);
            for (int i = 0; i < lgtvList.Count; i++)
            {
                PointLight pointLight = new PointLight();
                pointLight.Transform.Position = lgtvList[i].GetProperty<Vector3>("Position");
                pointLight.Radius = lgtvList[i].GetProperty<Vector3>("Radius");
                pointLight.Color = lgtvList[i].GetProperty<Color32>("Color");

                scene.LGTV.Add(pointLight);
                world.RegisterObject(pointLight);
            }
        }

        private static List<MapEntity> FindAllByType(string fourCC, BindingList<MapEntity> fromList)
        {
            List<MapEntity> results = new List<MapEntity>();
            foreach (var item in fromList)
            {
                if (string.Compare(item.FourCC, fourCC, StringComparison.InvariantCultureIgnoreCase) == 0)
                    results.Add(item);
            }

            return results;
        }
    }
}

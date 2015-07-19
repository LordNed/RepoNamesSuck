using GameFormatReader.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (!System.IO.Directory.Exists(folderPath))
                throw new System.IO.DirectoryNotFoundException("folderPath not found, ensure the directory exists first!");

            // Calculate the Map Name from the folderPath - it should be the last segment of the folder path.s
            System.IO.DirectoryInfo rootFolderInfo = new System.IO.DirectoryInfo(folderPath);
            string mapName = rootFolderInfo.Name;


            // Sort the directories in rootFolderInfo into natural order, instead of alphabetical order which solves issues
            // where room indexes were getting remapped to the wrong one.
            IEnumerable<System.IO.DirectoryInfo> subFolders = rootFolderInfo.GetDirectories().OrderByNatural(x => x.Name);
            IEnumerable<System.IO.FileInfo> subFiles = rootFolderInfo.GetFiles().OrderByNatural(x => x.Name);

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

                string arcName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.FullName).ToLower();
                archiveFolderMap[arcName] = archive;
            }

            Map newMap = new Map();
            newMap.Name = mapName;
            newMap.ProjectFilePath = System.IO.Path.GetDirectoryName(folderPath);

            LoadEntities(newMap, archiveFolderMap, world);

            return newMap;
        }

        private void LoadEntities(Map newMap, Dictionary<string, VirtualFilesystemDirectory> archiveFolderMap, WWorld world)
        {
            MapEntityLoader entityLoader = new MapEntityLoader(newMap);

            // For each room/scene, find the associated dzr/dzs file and load its
            // contents into the entityLoader.
            foreach (var kvp in archiveFolderMap)
            {
                Scene sceneData = null;

                if (kvp.Key.StartsWith("room"))
                {
                    sceneData = new Room();
                    newMap.Rooms.Add((Room)sceneData);
                }
                else if (kvp.Key.StartsWith("stage"))
                {
                    sceneData = new Stage();
                    newMap.Stage = (Stage)sceneData;
                }
                else
                {
                    // Skip any errant thing in the folder that's not a room or a stage.
                    continue;
                }

                sceneData.Name = kvp.Key;
                // Check to see if this Archive has stage/room entity data.
                var roomEntData = kvp.Value.FindByExtension(".dzr");
                var stageEntData = kvp.Value.FindByExtension(".dzs");

                VirtualFilesystemFile vfsFile = null;
                if (roomEntData.Count > 0)
                    vfsFile = roomEntData[0];
                else if (stageEntData.Count > 0)
                    vfsFile = stageEntData[0];
                else
                    continue;

                using (EndianBinaryReader reader = new EndianBinaryReader(new System.IO.MemoryStream(vfsFile.File.GetData()), Endian.Big))
                {
                    entityLoader.LoadFromStream(sceneData, reader);
                }
            }

            // Once we've loaded all of the entities from the stream, we're going to
            // post-process them to resolve object references.
            entityLoader.PostProcessEntities();

            // Finally, we can actually convert these into map objects
            foreach (var roomOrStageData in entityLoader.GetData())
            {
                Stage stage = roomOrStageData.Key as Stage;
                Room room = roomOrStageData.Key as Room;

                if (stage != null)
                {
                    PostProcessStage(stage, roomOrStageData.Value);
                    foreach (var entity in stage.Entities)
                        entity.World = world;
                }
                if (room != null)
                {
                    PostProcessRoom(room, roomOrStageData.Value);
                    foreach (var entity in room.Entities)
                        entity.World = world;
                }                
            }
        }

        private static void PostProcessStage(Stage stage, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            // Process objects which belong to both Stage and Rooms.
            PostProcessSharedEntities(stage, rawEntityData);

            // Create Arrows out of AROB tags
            PostProcessArrows(stage, "AROB", rawEntityData);

            // Create Paths out of RPAT tags
            PostProcessPaths(stage, "PATH", "PPNT", rawEntityData);
            // Non Physical Items
            // CAMR
            // DMAP
            // EnvR
            // Colo
            // Pale
            // Virt
            // EVNT
            // MECO
            // MEMA
            // MULT
            // PATH
            // RTBL
            // STAG
        }

        private static void PostProcessRoom(Room room, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            // Process objects which belong to both Stage and Rooms.
            PostProcessSharedEntities(room, rawEntityData);

            // Create Doors out of DOOR tags
            PostProcessDoors(room, rawEntityData);

            // Create Light Vectors out of LGTV tags
            PostProcessLightVectors(room, rawEntityData);

            // Create TGOBs out of TGOB tags.
            PostProcessTGOB(room, rawEntityData);

            // Create TGSC out of TGSC tags.
            PostProcessTGSC(room, rawEntityData);

            // Non Physical Items
            // FILI
        }

        private static void PostProcessSharedEntities(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            // Create Actors out of ACTR tags.
            PostProcessActors(scene, rawEntityData);

            // Create scaleable objects out of SCOB tags
            PostProcessScaleableObjects(scene, rawEntityData);

            // Create Player Spawns out of PLYR tags
            PostProcessPlayerSpawns(scene, rawEntityData);

            // Create Arrows out of RARO tags
            PostProcessArrows(scene, "RARO", rawEntityData);

            // Create Paths out of RPAT tags
            PostProcessPaths(scene, "RPAT", "RPPN", rawEntityData);

            // Create Sound Sources out of SOND tags.
            PostProcessSoundSources(scene, rawEntityData);

            // Create TGDR objects out of TGDR tags.
            PostProcessTGDR(scene, rawEntityData);

            // Create Treasure Chests out of TRES tags.
            PostProcessTreasureChests(scene, rawEntityData);

            // Non Physical Items
            // 2DMA (Both)
            // FLOR (Both)
            // RCAM (Both)
            // RPAT (Both)
            // SCLS (Both)
        }

        private static void PostProcessActors(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var actorEntity in FindAllByType("ACTR", rawEntityData))
            {
                Actor actor = new Actor();
                actor.Name = actorEntity.Fields.GetProperty<string>("Name");
                actor.Fields = actorEntity.Fields;

                ProcessTransform(actor);
                actor.Fields.RemoveProperty("Name");

                scene.Entities.Add(actor);
            }
        }

        private static void PostProcessScaleableObjects(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach(var scobEntity in FindAllByType("SCOB", rawEntityData))
            {
                ScaleableObject scob = new ScaleableObject();
                scob.Name = scobEntity.Fields.GetProperty<string>("Name");
                scob.Fields = scobEntity.Fields;

                ProcessTransform(scob);
                scob.Fields.RemoveProperty("Name");

                scene.Entities.Add(scob);
            }
        }

        private static void PostProcessTreasureChests(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach(var chestEntity in FindAllByType("TRES", rawEntityData))
            {
                TreasureChest tres = new TreasureChest();
                tres.Name = chestEntity.Fields.GetProperty<string>("Name");
                tres.Fields = chestEntity.Fields;

                ProcessTransform(tres);
                tres.Fields.RemoveProperty("Name");

                scene.Entities.Add(tres);
            }
        }

        private static void PostProcessTGDR(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach(var tgdrEntity in FindAllByType("TGDR", rawEntityData))
            {
                Door door = new Door();
                door.Name = tgdrEntity.Fields.GetProperty<string>("Name");
                door.Fields = tgdrEntity.Fields;

                ProcessTransform(door);
                door.Fields.RemoveProperty("Name");

                scene.Entities.Add(door);
            }
        }

        private static void PostProcessSoundSources(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach(var soundEntity in FindAllByType("SOND", rawEntityData))
            {
                SoundSource sndSource = new SoundSource();
                sndSource.Name = soundEntity.Fields.GetProperty<string>("Name");
                sndSource.Fields = soundEntity.Fields;

                ProcessTransform(sndSource);
                sndSource.Fields.RemoveProperty("Name");

                scene.Entities.Add(sndSource);
            }
        }

        private static void PostProcessPaths(Scene scene, string pathFourCC, string pointFourCC, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            // Build a list of PathPoints out of the point list.
            List<PathPoint> pathPointsList = new List<PathPoint>();
            foreach (var point in FindAllByType(pointFourCC, rawEntityData))
            {
                PathPoint newPoint = new PathPoint
                {
                    Unknown1 = point.Fields.GetProperty<int>("Unknown 1"),
                    Postion = point.Fields.GetProperty<Vector3>("Position")
                };

                pathPointsList.Add(newPoint);
            }

            // Now create a list of MapPaths and assign the loaded PathPoints to it.
            foreach (var path in FindAllByType(pathFourCC, rawEntityData))
            {
                short numPoints = path.Fields.GetProperty<short>("Number of Points");
                int firstPointOffset = path.Fields.GetProperty<int>("First Entry Offset");

                // Paths store their number of points, and the offset (in bytes) to the index of the first path in the list.
                // We divide by the length of a PPNT/RPPN to turn the offset into an index.
                int pointStartIndex = firstPointOffset / 0x10;

                Path newPath = new Path();
                newPath.Fields = path.Fields;
                newPath.Fields.RemoveProperty("Number of Points");
                newPath.Fields.RemoveProperty("First Entry Offset");
                
                for (int i = pointStartIndex; i < pointStartIndex + numPoints; i++)
                {
                    newPath.Points.Add(pathPointsList[i]);
                }

                scene.Entities.Add(newPath);
            }
        }

        private static void PostProcessArrows(Scene scene, string arrowFourCC, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var arrowEntity in FindAllByType(arrowFourCC, rawEntityData))
            {
                Arrow arrow = new Arrow();

                ProcessTransform(arrow);

                scene.Entities.Add(arrow);

                Trace.Assert((ushort)arrowEntity.Fields.GetProperty<short>("Padding") == 0xFFFF);
            }
        }

        private static void PostProcessPlayerSpawns(Scene scene, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach(var spawnEntity in FindAllByType("PLYR", rawEntityData))
            {
                PlayerSpawn spawn = new PlayerSpawn();
                spawn.Name = spawnEntity.Fields.GetProperty<string>("Name");
                spawn.Fields = spawnEntity.Fields;
                spawn.Fields.RemoveProperty("Name");

                ProcessTransform(spawn);

                scene.Entities.Add(spawn);
            }
        }

        private static void PostProcessTGSC(Room room, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var tgscEntity in FindAllByType("TGSC", rawEntityData))
            {
                ScaleableObject tgsc = new ScaleableObject();
                tgsc.Name = tgscEntity.Fields.GetProperty<string>("Name");
                tgsc.Fields = tgscEntity.Fields;

                ProcessTransform(tgsc);
                tgsc.Fields.RemoveProperty("Name");

                room.Entities.Add(tgsc);
            }
        }

        private static void PostProcessTGOB(Room room, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var tgobEntity in FindAllByType("TGOB", rawEntityData))
            {
                Actor tgob = new Actor();
                tgob.Name = tgobEntity.Fields.GetProperty<string>("Name");
                tgob.Fields = tgobEntity.Fields;

                ProcessTransform(tgob);
                tgob.Fields.RemoveProperty("Name");

                room.Entities.Add(tgob);
            }
        }

        private static void PostProcessLightVectors(Room room, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var lgtvEntity in FindAllByType("LGTV", rawEntityData))
            {
                LightVector tgob = new LightVector();
                tgob.Fields = lgtvEntity.Fields;

                ProcessTransform(tgob);

                room.Entities.Add(tgob);
            }
        }

        private static void PostProcessDoors(Room room, List<MapEntityLoader.RawMapEntity> rawEntityData)
        {
            foreach (var doorEntity in FindAllByType("DOOR", rawEntityData))
            {
                Door door = new Door();
                door.Name = doorEntity.Fields.GetProperty<string>("Name");
                door.Fields = doorEntity.Fields;

                ProcessTransform(door);
                door.Fields.RemoveProperty("Name");

                room.Entities.Add(door);
            }
        }

        private static List<MapEntityLoader.RawMapEntity> FindAllByType(string fourCC, List<MapEntityLoader.RawMapEntity> fromList)
        {
            var results = new List<MapEntityLoader.RawMapEntity>();
            foreach (var item in fromList)
            {
                if (string.Compare(item.FourCC, fourCC, StringComparison.InvariantCultureIgnoreCase) == 0)
                    results.Add(item);
            }

            return results;
        }

        private static void ProcessTransform(SceneComponent forObject)
        {
            // Remove the Position field as the Actor has a Transform which gives it a position.
            if (forObject.Fields.HasProperty("Position"))
            {
                forObject.Transform.Position = forObject.Fields.GetProperty<Vector3>("Position");
                forObject.Fields.RemoveProperty("Position");
            }

            // Remove the Rotation field.
            if (forObject.Fields.HasProperty("Rotation"))
            {
                Quaternion rotation = forObject.Fields.GetProperty<Quaternion>("Rotation");
                forObject.Transform.Rotation = rotation;
                forObject.Fields.RemoveProperty("Rotation");
            }

            // Remove the Scale field.
            if (forObject.Fields.HasProperty("Scale"))
            {
                forObject.Transform.Scale = forObject.Fields.GetProperty<Vector3>("Scale");
                forObject.Fields.RemoveProperty("Scale");
            }
        }
    }
}

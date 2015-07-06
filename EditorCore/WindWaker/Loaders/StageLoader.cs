using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Maps;
using WEditor.FileSystem;

namespace WEditor.WindWaker.Loaders
{
    public class StageLoader
    {
        public Stage LoadFromArchive(WWorld world, ZArchive stageArchive)
        {
            if (world == null)
                throw new ArgumentNullException("world must not be null!");
            if (stageArchive == null)
                throw new ArgumentNullException("stageArchive must not be null!");

            Stage stage = new Stage();

            // Wind Waker only supports one-level deep of folders (ie: root/<folders>/<files>) so
            // we don't load this recursively.
            foreach (var vFolder in stageArchive.Contents.Children)
            {
                // Skip loose files alongside the top level folders inside the root.
                if (vFolder.Type != NodeType.Directory)
                    continue;

                VirtualFilesystemDirectory folder = (VirtualFilesystemDirectory)vFolder;
                foreach (var vFile in folder.Children)
                {
                    // Skip folder inside of these folders, the game doesn't use them and it's probably a mis-placed/broken file.
                    if (vFile.Type != NodeType.File)
                        continue;

                    VirtualFilesystemFile file = (VirtualFilesystemFile)vFile;

                    switch (folder.Name.ToLower())
                    {
                        /* Room and Stage Entity Data */
                        case ".dzr":
                        case ".dzs":
                            LoadStageEntityData(file, stage, world);
                            break;
                        /* 3D Model Formats */
                        case ".bmd":
                        case ".bdl":
                            //LoadStageModelData(file, stage, world);
                            break;
                        /* Map Collision Format */
                        case ".dzb":
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
                            break;
                        default:
                            WLog.Warning(LogCategory.ArchiveLoading, stageArchive, "Unknown folder type \"{0}\" found in Archive \"{1}\"", folder.Name, stageArchive.Name);
                            break;
                    }
                }

            }

            return stage;
        }

        private void LoadStageEntityData(VirtualFilesystemFile file, Stage stage, WWorld world)
        {
            WLog.Info(LogCategory.ArchiveLoading, null, "Loading DZS (Stage Entity Data) {0}.{1}...", file.Name, file.Extension);

            // We're going to load this DZS file into a generic list of all things contained by the DZS/DZR file, and then we'll manually pick through
            // chunks who's info we care about. Some objects will get stored on the Stage and discarded, while others will get converted into intermediate
            // formats.
            MapEntityLoader entityLoader = new MapEntityLoader();
            using(EndianBinaryReader reader = new EndianBinaryReader(file.Data.GetData(), Endian.Big))
            {
                List<MapEntityData> stageEntityData = entityLoader.LoadFromStream(reader);
            }

        
            WLog.Info(LogCategory.ArchiveLoading, null, "Loaded {0}.{1}.", file.Name, file.Extension);
        }
    }
}

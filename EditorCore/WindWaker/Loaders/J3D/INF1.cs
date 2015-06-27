using GameFormatReader.Common;
using System;
using System.Collections.Generic;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        private static SceneNode LoadINF1FromFile(SceneNode rootNode, EndianBinaryReader reader, long chunkStart)
        {
            ushort unknown1 = reader.ReadUInt16(); // A lot of Link's models have it but no idea what it means. Alt. doc says: "0 for BDL, 01 for BMD"
            ushort padding = reader.ReadUInt16();
            uint packetCount = reader.ReadUInt32(); // Total number of Packets across all Batches in file.
            uint vertexCount = reader.ReadUInt32(); // Total number of vertexes across all batches within the file.
            uint hierarchyDataOffset = reader.ReadUInt32();

            // The Hierarchy defines how Joints, Materials and Shapes are laid out. This allows them to bind a material
            // and draw multiple shapes (batches) with the material. It also complicates drawing things, but whatever.
            reader.BaseStream.Position = chunkStart + hierarchyDataOffset;

            List<J3DFileResource.InfoNode> infoNodes = new List<J3DFileResource.InfoNode>();
            J3DFileResource.InfoNode curNode = null;

            do
            {
                curNode = new J3DFileResource.InfoNode();
                curNode.Type = (J3DFileResource.HierarchyDataTypes)reader.ReadUInt16();
                curNode.Value = reader.ReadUInt16(); // "Index into Joint, Material, or Shape table.

                infoNodes.Add(curNode);
            }
            while (curNode.Type != J3DFileResource.HierarchyDataTypes.Finish);

            ConvertInfoHiearchyToSceneGraph(ref rootNode, infoNodes, 0);
            return rootNode;
        }

        /// <summary>
        /// This function is used to convert from a flat-file of J3DFileResource.InfoNodes into a treeview-type version of SceneNode.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="allNodes"></param>
        /// <param name="currentListIndex"></param>
        /// <returns></returns>
        private static int ConvertInfoHiearchyToSceneGraph(ref SceneNode parent, List<J3DFileResource.InfoNode> allNodes, int currentListIndex)
        {
            for (int i = currentListIndex; i < allNodes.Count; i++)
            {
                J3DFileResource.InfoNode curNode = allNodes[i];
                SceneNode newNode = new SceneNode();

                switch (curNode.Type)
                {
                    case J3DFileResource.HierarchyDataTypes.NewNode:
                        // Increase the depth of the hierarchy by creating a new node and then processing all of the next nodes as its children.
                        // This function is recursive and will return the integer value of how many nodes it processed, this allows us to skip
                        // the list forward that many now that they've been handled.
                        newNode.Type = J3DFileResource.HierarchyDataTypes.NewNode;
                        newNode.Value = curNode.Value;

                        i += ConvertInfoHiearchyToSceneGraph(ref newNode, allNodes, i + 1);
                        parent.Children.Add(newNode);
                        break;

                    case J3DFileResource.HierarchyDataTypes.EndNode:
                        // Alternatively, if it's a EndNode, that's our signal to go up a level. We return the number of nodes that were processed between the last NewNode
                        // and this EndNode at this depth in the hierarchy.
                        return i - currentListIndex + 1;

                    case J3DFileResource.HierarchyDataTypes.Material:
                    case J3DFileResource.HierarchyDataTypes.Joint:
                    case J3DFileResource.HierarchyDataTypes.Batch:
                    case J3DFileResource.HierarchyDataTypes.Finish:

                        // If it's any of the above we simply create a node for them. We create and pull from a different InfoNode because
                        // Hitting a NewNode can modify the value of i so curNode is now no longer valid.
                        J3DFileResource.InfoNode thisNode = allNodes[i];
                        newNode.Type = thisNode.Type;
                        newNode.Value = thisNode.Value;
                        parent.Children.Add(newNode);
                        break;

                    default:
                        Console.WriteLine("[J3DLoader] Unsupported HierarchyDataType \"{0}\" in model!", curNode.Type);
                        break;
                }
            }

            return 0;
        }
    }
}

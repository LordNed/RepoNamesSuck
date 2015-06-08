using System.ComponentModel;
namespace EditorCore.WindWaker
{
    public enum ArchiveType 
    {
        Stage, 
        Room 
    }

    /// <summary>
    /// A Zelda Archive (ZArchive for short) describes one (eventual) exported .arc file. It holds a list of the 
    /// the files in the archive.
    /// </summary>
    public class ZArchive
    {
        public ArchiveType Type { get; private set; }
        public BindingList<BaseFileResource> Files { get; private set; }

        public ZArchive(ArchiveType type)
        {
            Type = type;
            Files = new BindingList<BaseFileResource>();
        }
    }
}

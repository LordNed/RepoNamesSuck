using System;
using System.Collections.Generic;

namespace EditorCore.WindWaker.MapEntities
{
    public class MapEntityObject : BaseFileResource
    {
        public MapEntityObject(string fileName, string folderName, ZArchive parentArchive) : base (fileName, folderName, parentArchive)
        {

        }

        public override string ToString()
        {
            return string.Format("[MapEntityObject] {0}", base.ToString());
        }
    }
}

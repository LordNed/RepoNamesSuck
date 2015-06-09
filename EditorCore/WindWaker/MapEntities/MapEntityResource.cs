using EditorCore.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EditorCore.WindWaker.MapEntities
{
    public class MapEntityResource : BaseFileResource
    {
        public BindingList<MapEntityObject> Objects;

        public MapEntityResource(string fileName, string folderName, ZArchive parentArchive) : base (fileName, folderName, parentArchive)
        {
            Objects = new BindingList<MapEntityObject>();
        }

        public override string ToString()
        {
            return string.Format("[MapEntityResource] {0}", base.ToString());
        }
    }

    
}

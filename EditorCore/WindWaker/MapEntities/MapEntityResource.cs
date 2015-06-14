using WEditor.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WEditor.WindWaker.MapEntities
{
    public class MapEntityResource : BaseFileResource
    {
        public BindingList<MapEntityObject> Objects { get; private set; }

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

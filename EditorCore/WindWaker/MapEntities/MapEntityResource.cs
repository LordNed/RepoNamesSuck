using WEditor.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WEditor.WindWaker.MapEntities
{
    public class MapEntityResource : BaseFileResource
    {
        public ObservableCollection<MapEntityObject> Objects { get; private set; }

        public MapEntityResource(string fileName, ZArchive parentArchive) : base (fileName, parentArchive)
        {
            Objects = new ObservableCollection<MapEntityObject>();
        }

        public override string ToString()
        {
            return string.Format("[MapEntityResource] {0}", base.ToString());
        }
    }

    
}

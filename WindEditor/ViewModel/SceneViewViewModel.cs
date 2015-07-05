using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor;
using WEditor.FileSystem;
using WEditor.WindWaker;
using WEditor.WindWaker.MapEntities;

namespace WindEditor.UI
{
    public class SceneViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<ZArchive> ArchiveList
        {
            get { return m_archiveList; }
            set
            {
                m_archiveList = value;
                OnPropertyChanged("ArchiveList");
            }
        }

        private BindingList<ZArchive> m_archiveList;
        private MainWindowViewModel m_mainView;

        public SceneViewViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
        }

        public void SetMap(Map map)
        {
            ArchiveList = new BindingList<ZArchive>();
            if (map == null)
                return;

            for (int i = 0; i < map.Rooms.Count; i++)
                ArchiveList.Add(map.Rooms[i]);
            ArchiveList.Add(map.Stage);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnSceneViewSelectObject(object newObject)
        {
            // newObject can be of a variety of different things - it could be a VirtualDirectory, VirtualFile, Archive, etc.
            // There's not really an easy way I know of, so we're just going to manually handle all the types and walk up
            // till we find their parent Archive and then walk down to find the DZS/DZR so we can bind that to our EntityOutliner.
            ZArchive rootArchive = null;
            if (newObject is ZArchive)
            {
                rootArchive = (ZArchive)newObject;
            }
            else if (newObject is VirtualFilesystemDirectory || newObject is VirtualFilesystemFile)
            {
                var node = (VirtualFilesystemNode)newObject;
                rootArchive = node.ParentArchive;
            }
            else
            {
                WLog.Warning(LogCategory.UI, newObject, "Unknown object type selected in SceneView: {0}", newObject);
                return;
            }

            if(rootArchive != null)
            {
                var vfFileList = rootArchive.Files.FindByExtension(new[] { ".dzs", ".dzr" });
                if (vfFileList.Count > 0)
                {
                    m_mainView.SetSelectedEntityFile((MapEntityResource) vfFileList[0].File);
                }
                else
                {
                    m_mainView.SetSelectedEntityFile(null);
                }
            }
        }
    }
}

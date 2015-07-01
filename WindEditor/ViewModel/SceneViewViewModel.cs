using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.WindWaker;

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

        public SceneViewViewModel()
        {
            Console.WriteLine("Constructed.");
        }

        public void SetMap(Map map)
        {
            ArchiveList = new BindingList<ZArchive>();
            for (int i = 0; i < map.Rooms.Count; i++)
                ArchiveList.Add(map.Rooms[i]);
            ArchiveList.Add(map.Stage);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

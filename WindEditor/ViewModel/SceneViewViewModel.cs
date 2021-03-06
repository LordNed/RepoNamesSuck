﻿using System.ComponentModel;
using WEditor.WindWaker;

namespace WindEditor.UI.ViewModel
{
    public class SceneViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<Scene> ArchiveList
        {
            get { return m_archiveList; }
            set
            {
                m_archiveList = value;
                OnPropertyChanged("ArchiveList");
            }
        }

        public Scene SelectedScene
        {
            get { return m_mainView.SelectedScene; }
            set
            {
                m_mainView.SelectedScene = value;
                OnPropertyChanged("SelectedScene");
            }
        }

        private BindingList<Scene> m_archiveList;
        private MainWindowViewModel m_mainView;

        public SceneViewViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
            ArchiveList = new BindingList<Scene>();
        }

        public void SetMap(Map map)
        {
            ArchiveList.Clear();
            if (map == null)
                return;

            for (int i = 0; i < map.Rooms.Count; i++)
                ArchiveList.Add(map.Rooms[i]);
            ArchiveList.Add(map.Stage);

            if (map.Rooms.Count > 0)
                SelectedScene = map.Rooms[0];
        }

        internal void OnSceneViewSelectObject(object newObject)
        {
            m_mainView.SetSelectedSceneFile((Scene)newObject);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor;

namespace WindEditor.UI
{
    public class EntityOutlinerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MapEntityObject> EntityList
        {
            get { return m_entityList; }
            set
            {
                m_entityList = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MainWindowViewModel m_mainView;
        private ObservableCollection<MapEntityObject> m_entityList;

        public EntityOutlinerViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
            m_entityList = new ObservableCollection<MapEntityObject>();
        }


        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnSelectedEntityChanged(object newObject)
        {
            m_mainView.SetSelectedEntity((MapEntityObject)newObject);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor;
using WEditor.Common.Maps;

namespace WindEditor.UI
{
    public class EntityOutlinerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<MapEntityData> EntityList
        {
            get { return m_entityList; }
            set
            {
                m_entityList = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MainWindowViewModel m_mainView;
        private BindingList<MapEntityData> m_entityList;

        public EntityOutlinerViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
            m_entityList = new BindingList<MapEntityData>();
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

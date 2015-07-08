using System.ComponentModel;
using WEditor.Maps;

namespace WindEditor.UI
{
    public class EntityOutlinerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<MapEntity> EntityList
        {
            get { return m_entityList; }
            set
            {
                m_entityList = value;
                OnPropertyChanged("EntityList");
            }
        }

        private MainWindowViewModel m_mainView;
        private BindingList<MapEntity> m_entityList;

        public EntityOutlinerViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
            m_entityList = new BindingList<MapEntity>();
        }


        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnSelectedEntityChanged(object newObject)
        {
            m_mainView.SetSelectedEntity((MapEntity)newObject);
        }
    }
}

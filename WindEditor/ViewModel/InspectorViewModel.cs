using System.ComponentModel;
using WEditor.Maps;

namespace WindEditor.UI.ViewModel
{
    public class InspectorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MapEntity SelectedEntity
        {
            get { return m_selectedEntity; }
            set
            {
                m_selectedEntity = value;
                OnPropertyChanged("SelectedEntity");
            }
        }


        private MapEntity m_selectedEntity;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor;

namespace WindEditor.UI
{
    public class InspectorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MapEntityObject SelectedEntity
        {
            get { return m_selectedEntity; }
            set
            {
                m_selectedEntity = value;
                OnPropertyChanged("SelectedEntity");
            }
        }


        private MapEntityObject m_selectedEntity;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

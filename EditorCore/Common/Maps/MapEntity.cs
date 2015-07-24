using OpenTK;
using System;
using System.ComponentModel;

namespace WEditor.Maps
{

    public class MapEntity : WObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FourCC { get; internal set; }

        public MapLayer Layer
        {
            get { return m_mapLayer; }
            set
            {
                m_mapLayer = value;
                OnPropertyChanged("MapLayer");
            }
        }

        public bool LayerCanChange { get; internal set; }
        public PropertyCollection Fields
        {
            get { return m_fields; }
            set
            {
                m_fields = value;
                OnPropertyChanged("Fields");
            }
        }

        private MapLayer m_mapLayer;
        private PropertyCollection m_fields;

        public MapEntity()
        {
            Layer = MapLayer.Default;
            FourCC = string.Empty;
            LayerCanChange = false;
            Fields = new PropertyCollection();
        }

        public virtual void OnDrawGizmos() { }
        public virtual void OnDrawGizmosSelected() { }

        public override string ToString()
        {
            return FourCC;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

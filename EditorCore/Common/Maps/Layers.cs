using System.ComponentModel;

namespace WEditor.Maps
{
    public enum MapLayer
    {
        Default, // Always Loaded 
        Layer0, // Additionally loaded ontop of the Default layer
        Layer1,
        Layer2,
        Layer3,
        Layer4,
        Layer5,
        Layer6,
        Layer7,
        Layer8,
        Layer9,
        LayerA,
        LayerB,
    }

    public class LayerVisibility : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MapLayer Layer { get; private set; }
        public bool Visible
        {
            get { return m_visible; }
            set
            {
                m_visible = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
            }
        }

        private bool m_visible;

        public LayerVisibility(MapLayer layer)
        {
            Layer = layer;
            Visible = true;
        }

        public override string ToString()
        {
            return Layer.ToString();
        }
    }
}

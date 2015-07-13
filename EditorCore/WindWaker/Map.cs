using System.Collections.ObjectModel;
using System.ComponentModel;
using WEditor.Maps;

namespace WEditor.WindWaker
{
    /// <summary>
    /// A map is a collection of rooms and an associated stage.
    /// </summary>
    public class Map : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This is the name of the map. Maximum of 8 characters.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                if (m_name.Length > 8)
                    m_name = m_name.Substring(0, 8);

                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// This is the location of the Map on disk. This is assigned when the map is loaded. Does not contain
        /// the final folder name (which is the name of the map)
        /// </summary>
        public string ProjectFilePath
        {
            get { return m_projectFilePath; }
            set
            {
                m_projectFilePath = value;
                OnPropertyChanged("ProjectFilePath");
            }
        }

        /// <summary>
        /// A list of currently loaded rooms in this map. Does not include the stage archive.
        /// </summary>
        public BindingList<Room> Rooms
        {
            get { return m_rooms; }
            set
            {
                m_rooms = value;
                OnPropertyChanged("Rooms");
            }
        }

        /// <summary>
        /// The stage associated with these rooms.
        /// </summary>
        public Stage Stage
        {
            get { return m_stage; }
            set
            { 
                m_stage = value;
                OnPropertyChanged("Stage");
            }
        }

        public BindingList<LayerVisibility> LayerVisibilities { get; private set; }

        private string m_name;
        private string m_projectFilePath;
        private Stage m_stage;
        private BindingList<Room> m_rooms;

        public Map()
        {
            Stage = null;
            Name = "Unnamed";

            Stage = null;
            Rooms = new BindingList<Room>();

            // Generate the 12 (+ default) layers Wind Waker supports
            LayerVisibilities = new BindingList<LayerVisibility>();
            for(int i = 0; i < 13; i++)
            {
                LayerVisibility newLayer = new LayerVisibility((MapLayer)i);
                LayerVisibilities.Add(newLayer);
            }
        }

        public bool LayerIsVisible(MapLayer layer)
        {
            for(int i = 0; i < LayerVisibilities.Count; i++)
            {
                if (LayerVisibilities[i].Layer == layer)
                    return LayerVisibilities[i].Visible;
            }

            return false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

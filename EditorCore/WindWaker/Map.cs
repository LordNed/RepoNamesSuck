﻿using System.ComponentModel;

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
        public BindingList<ZArchive> Rooms { get; private set; }

        /// <summary>
        /// The stage associated with these rooms.
        /// </summary>
        public ZArchive Stage
        {
            get { return m_stage; }
            set
            { 
                m_stage = value;
                OnPropertyChanged("Stage");
            }
        }

        public Stage NewStage;
        public BindingList<Room> NewRooms;

        private string m_name;
        private string m_projectFilePath;
        private ZArchive m_stage;

        public Map()
        {
            Stage = null;
            Rooms = new BindingList<ZArchive>();
            Name = "Unnamed";

            NewStage = null;
            NewRooms = new BindingList<Room>();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using WEditor.Common.Maps;
using WEditor.Maps;
using WEditor.Rendering;

namespace WEditor.WindWaker
{
    public abstract class Scene : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Is this scene currently visible? If so it will render in the editor, otherwise it (plus its contents) will not.
        /// </summary>
        public bool Visible
        {
            get { return m_visible; }
            set
            {
                m_visible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        /// <summary>
        /// What is the name of this? ie: Stage, Room0, etc.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        public BindingList<MapEntity> Entities { get; private set; }
        public List<Mesh> MeshList { get; private set; }

        private string m_name = string.Empty;
        private bool m_visible = true;

        protected Scene()
        {
            Entities = new BindingList<MapEntity>();
            MeshList = new List<Mesh>();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

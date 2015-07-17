using System.ComponentModel;
using WEditor.Common.Maps;
using WEditor.Maps;
using WEditor.Rendering;

namespace WEditor.WindWaker
{
    public abstract class Scene
    {
        /// <summary> List of LGHT entries in map. How is this different than LGTV? </summary>
        public BindingList<PointLight> LGHT { get; set; }

        /// <summary> List of LGTV entries in map. How is this different than LGHT? </summary>
        public BindingList<PointLight> LGTV { get; set; }

        /// <summary> I'm pretty sure these are Stage Arrows - only used in Stages. </summary>
        public BindingList<Arrow> AROB { get; set; }

        /// <summary> I'm pretty sure these are Room Arrows - only used in Rooms. </summary>
        public BindingList<Arrow> RARO { get; set; }
        
        /// <summary> Sources of ambient sound in the map. </summary>
        public BindingList<SoundSource> Sounds { get; set; }

        /// <summary> Ship Spawns  </summary>
        public BindingList<ShipSpawn> ShipSpawns { get; set; }


        public BindingList<MapEntity> Entities { get; set; }
        public BindingList<Mesh> Meshes { get; set; }
        public BindingList<SceneComponent> Objects { get; set; }


        public string Name { get; set; }

        protected Scene()
        {
            LGHT = new BindingList<PointLight>();
            LGTV = new BindingList<PointLight>();
            AROB = new BindingList<Arrow>();
            RARO = new BindingList<Arrow>();
            Sounds = new BindingList<SoundSource>();
            ShipSpawns = new BindingList<ShipSpawn>();

            Entities = new BindingList<MapEntity>();
            Meshes = new BindingList<Mesh>();
            Objects = new BindingList<SceneComponent>();
        }
    }
}

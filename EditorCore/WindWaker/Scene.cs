﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Common.Maps;
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
        


        public BindingList<MapEntityData> Entities { get; set; }
        public BindingList<Mesh> Meshes { get; set; }

        public Scene()
        {
            Entities = new BindingList<MapEntityData>();
            Meshes = new BindingList<Mesh>();
        }
    }
}

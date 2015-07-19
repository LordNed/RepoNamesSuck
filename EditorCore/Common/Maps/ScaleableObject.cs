﻿using OpenTK;

namespace WEditor.Maps
{
    public class ScaleableObject : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f);
        }
    }
}
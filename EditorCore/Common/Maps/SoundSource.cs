using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class SoundSource : SceneComponent
    {
        public string Name;
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public byte SoundId;
        public byte SoundRadius;
        public byte Padding1;
        public byte Padding2;
        public byte Padding3;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, new Vector3(SoundRadius * 10f, SoundRadius * 10f, SoundRadius * 10f));
        }
    }
}

using OpenTK;

namespace WEditor.Common.Maps
{
    public class SoundSource : WObject
    {
        public string Name;
        public Vector3 Position;
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
            World.Gizmos.DrawWireCube(Position, new Vector3(SoundRadius * 10f, SoundRadius * 10f, SoundRadius * 10f));
        }
    }
}

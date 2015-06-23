using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Common.Nintendo.J3D
{
    public class ZMode
    {
        public bool Enable;
        public byte Function;
        public bool UpdateEnable;
    }

    public class AlphaCompare
    {
        public byte Compare0;
        public byte Reference0;
        public byte Operation;
        public byte Compare1;
        public byte Reference1;
    }

    public class BlendMode
    {
        public byte Type;
        public byte SourceFact;
        public byte DestinationFact;
        public byte Operation;
    }

    public class ChannelControl
    {
        public bool Enable;
        public byte MaterialSrc;
        public byte LitMask;
        public byte DiffuseFunction;
        public byte AttenuationFunction;
        public byte AmbientSource;
    }

    public class TexCoordGen
    {
        public byte Type;
        public byte Source;
        public byte TexMatrix;
    }

    public class TexMatrix
    {
        public byte Projection;
        public byte Type;
        public float CenterS;
        public float CenterT;
        public float Unknown0;
        public float ScaleS;
        public float ScaleT;
        public float Rotation;
        public float TranslateS;
        public float TranslateT;
        public float[] PreMatrix; // 4 Long
    }

    public class TevIn
    {
        public byte A;
        public byte B;
        public byte C;
        public byte D;
    }

    public class TevOp
    {
        public byte Operation;
        public byte Bias;
        public byte Scale;
        public byte Clamp;
        public byte Out;
    }

    public class TevOrder
    {
        public byte TexCoord;
        public byte TexMap;
        public byte Color;
    }

    public class TevIndirect
    {
        public byte IndStage;
        public byte Format;
        public byte Bias;
        public byte IndMatrix;
        public byte WrapS;
        public byte WrapT;
        public byte AddPrev;
        public byte Utclod; // wtf?
        public byte Alpha;
    }

    public class TevSwapMode
    {
        public byte RasSel;
        public byte TexSel;
    }

    public class TevSwapModeTable
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    public class IndTexOrder
    {
        public byte TexCoord;
        public byte TexMap;
    }

    public class IndTexCoordScale
    {
        public byte ScaleS;
        public byte ScaleT;
    }

    public class IndTexMatrix
    {
        public float[] OffsetMatrix; // 2 of them
        public byte ScaleExponent;
    }

    public class TevCombiner
    {
        public byte Unknown0;
        public TevIn ColorIn;
        public TevOp ColorOp;
        public TevIn AlphaIn;
        public TevOp AlphaOp;
    }

    public class FogInfo
    {
        public byte Type;
        public bool Enable;
        public ushort Center;
        public float StartZ;
        public float EndZ;
        public float NearZ;
        public float FarZ;
        public Color32 Color;
        public ushort[] Table; // 10 of these.
    }
}

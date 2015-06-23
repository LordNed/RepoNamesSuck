using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Rendering;

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
        public float[,] PreMatrix; // 4x4
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
        public byte TexCoordId;
        public byte TexMap;
        public byte ChannelId;
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

    public class TevCombinerStage
    {
        public byte Unknown0;
        //public TevIn ColorIn;
        //public TevOp ColorOp;
        //public TevIn AlphaIn;
        //public TevOp AlphaOp;

        public byte ColorIn;
        public byte ColorOp;
        public byte ColorBias;
        public byte ColorScale;
        public byte ColorClamp;
        public byte ColorRegId;
        public byte AlphaIn;
        public byte AlphaOp;
        public byte AlphaBias;
        public byte AlphaScale;
        public byte AlphaClamp;
        public byte AlphaRegId;
        public byte Unknown1;
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

    public class Material
    {
        public byte Unknown0;
        public GXCullMode CullMode;
        public byte NumColorChannels; // "NumChannels"
        public byte NumTexGens;
        public byte NumTevStages;
        public ZMode ZMode; // Z Compare Mode - glDepthFunc, less, lequal, equal, greater, always, never
        public byte DitherIndex;
        public Color[] MaterialColors; // 2
        public ChannelControl[] ChannelControl; // 4
        public Color[] AmbientColors; // 2
        public Color[] LightingColors; // 8
        public TexCoordGen[] TexCoordGens; // 8
        public TexCoordGen[] TexCoordGens2; // 8
        public TexMatrix[] TexMatrixs; // 10
        public TexMatrix[] TexMatrixs2; // 20
        public Texture2D[] Textures; // ToDo: Remap this already?
        public Color[] TevKColor; //4
        public GXKonstColorSel[] ConstColorSel; // 16
        public GXKonstAlphaSel[] ConstAlphaSel; // 16
        public TevOrder[] TevOrderIndex; // 16
        public Color[] TevColor; // 4
        public TevCombinerStage[] TevStageInfo; // 16
        public TevSwapMode[] TevSwapMode; // 16
        public TevSwapModeTable[] TevSwapModeTable; // 16
        public short[] UnknownIndexes; // 12
        public short FogIndex;
        public AlphaCompare AlphaCompare;
        public BlendMode BlendMode;
        public short Unknown2;
    }
}

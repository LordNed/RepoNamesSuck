using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Rendering;

namespace WEditor.Common.Nintendo.J3D
{
    /// <summary>
    /// GX_SetZMode - Sets the Z-buffer compare mode. The result of the Z compare is used to conditionally write color values to the Embedded Frame Buffer.
    /// </summary>
    public class ZMode
    {
        /// <summary> If false, ZBuffering is disabled and the Z buffer is not updated. </summary>
        public bool Enable;
        /// <summary> Determines the comparison that is performed. The newely rasterized Z value is on the left while the value from the Z buffer is on the right. If the result of the comparison is false, the newly rasterized pixel is discarded. </summary>
        public GXCompareType Function;
        /// <summary> If true, the Z buffer is updated with the new Z value after a comparison is performed. 
        /// Example: Disabling this would prevent a write to the Z buffer, useful for UI elements or other things
        /// that shouldn't write to Z Buffer. See glDepthMask. </summary>
        public bool UpdateEnable;
    }

    /// <summary>
    /// GX_SetAlphaCompare - Sets the parameters for the alpha compare function which uses the alpha output from the last active TEV stage.
    /// The alpha compare operation is:
    ///     alpha_pass = (alpha_src(comp0)ref0) (op) (alpha_src(comp1)ref1)
    /// where alpha_src is the alpha from the last active TEV stage.
    /// </summary>
    public class AlphaCompare
    {
        /// <summary> subfunction 0 </summary>
        public GXCompareType Comp0;
        /// <summary> Reference value for subfunction 0. </summary>
        public byte Reference0;
        /// <summary> Alpha combine control for subfunctions 0 and 1. </summary>
        public GXAlphaOp Operation;
        /// <summary> subfunction 1 </summary>
        public GXCompareType Comp1;
        /// <summary> Reference value for subfunction 1. </summary>
        public byte Reference1;
    }

    /// <summary>
    /// GX_SetBlendMode - Determines how the source image is blended with the Embedded Frame Buffer.
    /// When <see cref="Type"/> is set to <see cref="GXBlendMode.None"/> the source data is written directly to the EFB. 
    /// When set to <see cref="GXBlendMode.Blend"/> source and EFB pixels are blended using the following equation:
    ///     dst_pix_clr = src_pix_clr * src_fact + dst_pix_clr * dst_fact
    /// </summary>
    public class BlendMode
    {
        /// <summary> Blending Type </summary>
        public GXBlendMode Type;
        /// <summary> Blending Control </summary>
        public GXBlendModeControl SourceFact;
        /// <summary> Blending Control </summary>
        public GXBlendModeControl DestinationFact;
        public byte Operation; // Seems to be logic operators such as clear, and, copy, equiv, inv, invand, etc.
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
        public Color Color;
        public ushort[] Table; // 10 of these.
    }

    public class Material
    {
        public byte Flag; // ToDo: Usage?
        public GXCullMode CullMode;
        public byte NumChannelControls; // "NumChans"
        public byte NumTexGens;
        public byte NumTevStages;

        /// <summary> Does the Z buffering happen before or after texturing. Normally it happens before, but when alpha compare is used, it must be done after. </summary>
        public byte ZCompareLocIndex; // This one is still an index I think, I don't think it's translated to its actual value.

        /// <summary> Z Buffer compare mode. </summary>
        public ZMode ZMode;

        public byte DitherIndex;


        public Color[] MaterialColors; // 2
        public ChannelControl[] ChannelControls; // 4
        public Color[] AmbientColors; // 2
        public Color[] LightingColors; // 8
        public TexCoordGen[] TexGenInfos; // 8
        public TexCoordGen[] TexGen2Infos; // 8
        public TexMatrix[] TexMatrices; // 10
        public TexMatrix[] DttMatrices; // 20 - Post-Transform Matrices
        public short[] Textures; // 8
        public Color[] TevKonstColors; //4
        public GXKonstColorSel[] KonstColorSels; // 16
        public GXKonstAlphaSel[] KonstAlphaSels; // 16
        public TevOrder[] TevOrderInfos; // 16

        /// <summary> Might be the colors the TEV registers are initialized with - prev, color0, color1 and color2. </summary>
        public Color[] TevColor; // 4 - er... which color? There's lots of colors!
        public TevCombinerStage[] TevStageInfos; // 16
        public TevSwapMode[] TevSwapModes; // 16
        public TevSwapModeTable[] TevSwapModeTables; // 16
        public short[] UnknownIndexes; // 12
        public short FogIndex;
        public AlphaCompare AlphaCompare;
        public BlendMode BlendMode;
        public short UnknownIndex2;
    }
}

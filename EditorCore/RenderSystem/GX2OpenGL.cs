using OpenTK.Graphics.OpenGL;
using WEditor.Common.Nintendo.J3D;

namespace WEditor.Rendering
{
    public static class GX2OpenGL
    {
        public static void SetDitherState(bool enabled)
        {
            if (enabled)
                GL.Enable(EnableCap.Dither);
            else
                GL.Disable(EnableCap.Dither);
        }

        public static void SetDepthState(ZMode zMode)
        {
            if (zMode.Enable)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(GetOpenGLDepthFunc(zMode.Function));
                GL.DepthMask(zMode.UpdateEnable);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public static DepthFunction GetOpenGLDepthFunc(GXCompareType gxCompare)
        {
            switch (gxCompare)
            {
                case GXCompareType.Never: return DepthFunction.Never;
                case GXCompareType.Less: return DepthFunction.Less;
                case GXCompareType.Equal: return DepthFunction.Equal;
                case GXCompareType.LEqual: return DepthFunction.Lequal;
                case GXCompareType.Greater: return DepthFunction.Greater;
                case GXCompareType.NEqual: return DepthFunction.Notequal;
                case GXCompareType.GEqual: return DepthFunction.Gequal;
                case GXCompareType.Always: return DepthFunction.Always;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXCompareType: \"{0}\" in GetOpenGLDepthFunc!", gxCompare);
                    return DepthFunction.Less;
            }
        }

        public static void SetCullState(GXCullMode gXCullMode)
        {
            GL.Enable(EnableCap.CullFace);

            switch (gXCullMode)
            {
                case GXCullMode.None:
                    GL.Disable(EnableCap.CullFace);
                    break;
                case GXCullMode.Front:
                    GL.CullFace(CullFaceMode.Front);
                    break;
                case GXCullMode.Back:
                    GL.CullFace(CullFaceMode.Back);
                    break;
                case GXCullMode.All:
                    GL.CullFace(CullFaceMode.FrontAndBack);
                    break;
            }
        }

        public static void SetBlendState(BlendMode blendMode)
        {
            if (blendMode.Type == GXBlendMode.Blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(GetOpenGLBlendSrc(blendMode.SourceFact), GetOpenGLBlendDest(blendMode.DestinationFact));
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        public static BlendingFactorDest GetOpenGLBlendDest(GXBlendModeControl gxMode)
        {
            switch (gxMode)
            {
                case GXBlendModeControl.Zero: return BlendingFactorDest.Zero;
                case GXBlendModeControl.One: return BlendingFactorDest.One;
                case GXBlendModeControl.SrcColor: return BlendingFactorDest.SrcColor;
                case GXBlendModeControl.InverseSrcColor: return BlendingFactorDest.OneMinusSrcColor;
                case GXBlendModeControl.SrcAlpha: return BlendingFactorDest.SrcAlpha;
                case GXBlendModeControl.InverseSrcAlpha: return BlendingFactorDest.OneMinusSrcAlpha;
                case GXBlendModeControl.DstAlpha: return BlendingFactorDest.DstAlpha;
                case GXBlendModeControl.InverseDstAlpha: return BlendingFactorDest.OneMinusDstAlpha;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXBlendModeControl: \"{0}\" in GetOpenGLBlendDest!", gxMode);
                    return BlendingFactorDest.OneMinusSrcAlpha;
            }
        }

        public static BlendingFactorSrc GetOpenGLBlendSrc(GXBlendModeControl gxMode)
        {
            switch (gxMode)
            {
                case GXBlendModeControl.Zero: return BlendingFactorSrc.Zero;
                case GXBlendModeControl.One: return BlendingFactorSrc.One;
                case GXBlendModeControl.SrcColor: return BlendingFactorSrc.SrcColor;
                case GXBlendModeControl.InverseSrcColor: return BlendingFactorSrc.OneMinusSrcColor;
                case GXBlendModeControl.SrcAlpha: return BlendingFactorSrc.SrcAlpha;
                case GXBlendModeControl.InverseSrcAlpha: return BlendingFactorSrc.OneMinusSrcAlpha;
                case GXBlendModeControl.DstAlpha: return BlendingFactorSrc.DstAlpha;
                case GXBlendModeControl.InverseDstAlpha: return BlendingFactorSrc.OneMinusDstAlpha;
                default:
                    WLog.Warning(LogCategory.Rendering, null, "Unsupported GXBlendModeControl: \"{0}\" in GetOpenGLBlendSrc!", gxMode);
                    return BlendingFactorSrc.SrcAlpha;
            };
        }
    }
}

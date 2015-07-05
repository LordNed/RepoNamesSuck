using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Rendering
{
    public class Texture2D : IDisposable
    {
        public TextureWrapMode WrapS
        {
            get { return m_wrapS; }
            set
            {
                m_wrapS = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)m_wrapS);
            }
        }

        public TextureWrapMode WrapT
        {
            get { return m_wrapT; }
            set
            {
                m_wrapT = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)m_wrapT);
            }
        }

        public TextureMinFilter MinFilter
        {
            get { return m_minFilter; }
            set
            {
                m_minFilter = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)m_minFilter);
            }
        }

        public TextureMagFilter MagFilter
        {
            get { return m_magFilter; }
            set
            {
                m_magFilter = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)m_magFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.ClampToBorder, (int)m_magFilter);
            }
        }

        public Color BorderColor
        {
            get { return m_borderColor; }
            set
            {
                m_borderColor = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] {m_borderColor.R, m_borderColor.G, m_borderColor.B, m_borderColor.A});
            }
        }

        public float MipMapBias
        {
            get { return m_mipMapBias; }
            set
            {
                m_mipMapBias = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, m_mipMapBias);
            }
        }

        public byte[] PixelData
        {
            get { return m_pixelData; }
            set
            {
                m_pixelData = value;
                GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, m_pixelData);
            }
        }

        public ushort Width
        {
            get { return m_width; }
        }

        public ushort Height
        {
            get { return m_height; }
        }

        private TextureWrapMode m_wrapS;
        private TextureWrapMode m_wrapT;
        private TextureMinFilter m_minFilter;
        private TextureMagFilter m_magFilter;
        private Color m_borderColor;
        private float m_mipMapBias;
        private ushort m_width;
        private ushort m_height;

        private int m_textureBuffer;
        private byte[] m_pixelData;

        public Texture2D(ushort width, ushort height)
        {
            // Generate a buffer on the GPU to store our texture data.
            GL.GenTextures(1, out m_textureBuffer);

            m_width = width;
            m_height = height;

            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;
            MinFilter = TextureMinFilter.Nearest;
            MagFilter = TextureMagFilter.Nearest;
            BorderColor = new Color(1f, 0f, 1f, 1f);
            MipMapBias = 0f;
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, m_textureBuffer);
        }

        public static Texture2D GenerateCheckerboard()
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.PixelData = new byte[] {    0, 0, 0, 255, 255, 255, 255, 255,
                                                255, 255, 255, 255, 0, 0, 0, 255 };

            return texture;
        }

        public static Texture2D GenerateWhite()
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.PixelData = new byte[] {    255, 255, 255, 255, 255, 255, 255, 255, 
                                                255, 255, 255, 255, 255, 255, 255, 255 };

            return texture;
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref m_textureBuffer);
        }
    }
}

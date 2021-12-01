using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace RenderLib
{
    public class FastBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public int[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public static FastBitmap FromBitmap(Bitmap bitmap)
        {
            FastBitmap fastBitmap = new FastBitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int g = 0; g < bitmap.Height; g++)
                {
                    fastBitmap.Bits[g * bitmap.Width + i] = bitmap.GetPixel(i, g).ToArgb();
                }
            }
            return fastBitmap;
        }
        public FastBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            Disposed = false;
        }

        public void Clear()
        {
            for (int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = 0;
            }
        }
        public void SetPixel(int x, int y, Color color)
        {
            int col = color.ToArgb();
            Bits[x + (y * Width)] = col;
        }

        public void SetPixel(int x, int y, int color)
        {
            Bits[x + (y * Width)] = color;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            Color color = Color.FromArgb(Bits[index]);
            return color;
        }

        public void Dispose()
        {
            BitsHandle.Free();
            Disposed = true;
        }
    }

    class Texture
    {
        public FastBitmap Texels { get; private set; }

        public Texture(string filepath)
        {
            Texels = FastBitmap.FromBitmap((Bitmap)Image.FromFile(filepath));
        }

        public Texture(Bitmap bmp)
        {
            Texels = FastBitmap.FromBitmap(bmp);
        }

        public Color GetTexel(float u, float v)
        {
            if (u < 0 || v < 0 || u > 1 || v > 1)
                throw new Exception("Invalid values of u or v!");

            int x_texel = (int)Math.Floor(Texels.Width * u);
            int y_texel = (int)Math.Floor(Texels.Height * v);

            return Texels.GetPixel(x_texel, y_texel);
        }
    }

    class MipMap
    {
        public List<Texture> MipLevels { get; protected set; }

        public MipMap(List<Texture> textures)
        {
            MipLevels.AddRange(textures);
        }

        public int GetMipLevel(int x_min, int x_max, int y_min, int y_max)
        {
            int result = MipLevels.Count - (int)Math.Ceiling(Math.Log((x_max - x_min) * (y_max - y_min), 4));

            return result < 0 ? 0 : result;
        }
    }
}

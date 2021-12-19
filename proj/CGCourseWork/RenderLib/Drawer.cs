using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;


namespace RenderLib
{
    public class Drawer
    {
        int width, height;
        public FastBitmap FrameBuffer { get; private set; }
        public float[,] DepthBuffer { get; private set; }

        public Drawer(int width, int height)
        {
            this.width = width;
            this.height = height;
            FrameBuffer = new FastBitmap(width, height);
            DepthBuffer = new float[width, height];
        }

        public void DrawScene(Scene scene)
        {
            var pols = VerticesShading(scene);

            // тут ещё нужны этапы: растеризация, наложение текстур и теневой карты
            PixelShading(pols, scene.Camera);

            ZBuffer(pols);
        }

        private List<PolygonInfo> VerticesShading(Scene scene)
        {
            var matr_move = Matrix4x4.CreateTranslation(scene.Model.Pivot.Center - scene.Camera.Pivot.Center);
            var model_view_matr = scene.Model.ToWorldMatrix * matr_move * scene.Camera.Pivot.LocalCoordsMatrix * scene.Camera.PerspectiveClip;

            var vertices = scene.Model.Vertices.Clone();
            List<PolygonInfo> visible_pols = new List<PolygonInfo>();
            List<float> ws = new List<float>();

            foreach (var v in vertices)
            {
                v.Transform(model_view_matr, out float w);
                ws.Add(w);
            }

            Vector3 light_dir = Vector3.Transform(scene.LightSource.LightDirection, model_view_matr);

            foreach (var pol in scene.Model.Polygons)
            {
                if (scene.Camera.IsVisible(vertices, pol))
                {
                    visible_pols.Add(new PolygonInfo(vertices[pol[0]], vertices[pol[1]], vertices[pol[2]], scene.Model.GetPolNormal(pol).Transform(model_view_matr)));
                    visible_pols[visible_pols.Count - 1].Texture = scene.Model.Texture;

                    for (int i = 0; i < 3; i++)
                    {
                        visible_pols[visible_pols.Count - 1].LightLevelsOnVertices[i] = scene.LightSource.GetAngleIntensity(visible_pols[visible_pols.Count - 1].Vertices[i].Normal, light_dir);
                        visible_pols[visible_pols.Count - 1].Ws[i] = ws[pol[i]];
                    }
                }
            }

            return visible_pols;
        }

        private void PixelShading(List<PolygonInfo> pols, Camera cam)
        {
            foreach (var pol in pols)
            {
                for (int i = 0; i < 3; i++)
                    pol.ScreenVertices[i] = new ScreenVertex(cam.ScreenProjection(pol.Vertices[i].Position), pol.Ws[i], pol.Vertices[i].TextureCoords);

                var first = pol.ScreenVertices[0];
                var second = pol.ScreenVertices[1];
                var third = pol.ScreenVertices[2];

                if (first.ScreenY > second.ScreenY)
                    SystemAddon.Swap(ref first, ref second);
                if (first.ScreenY > third.ScreenY)
                    SystemAddon.Swap(ref first, ref third);
                if (second.ScreenY > third.ScreenY)
                    SystemAddon.Swap(ref second, ref third);

                int min_y = first.ScreenY;
                int max_y = third.ScreenY;


                float a = (second.ScreenY - first.ScreenY) * (third.Z - first.Z) - (second.Z - first.Z) * (third.ScreenY - first.ScreenY);
                float b = (second.Z - first.Z) * (third.ScreenX - first.ScreenX) - (second.ScreenX - first.ScreenX) * (third.Z - first.Z);
                float c = (second.ScreenX - first.ScreenX) * (third.ScreenY - first.ScreenY) - (second.ScreenY - first.ScreenY) * (third.ScreenX - first.ScreenX);
                float d = -(a * first.ScreenX + b * first.ScreenY + c * first.Z);

                for (int yi = Math.Max(0, min_y); yi <= Math.Min(max_y, height - 1); yi++)
                {
                    float beta, alpha = (float)(yi - min_y) / (max_y - min_y);
                    int x1 = MathAddon.RoundToInt(MathAddon.Lepr(first.ScreenX, third.ScreenX, alpha));
                    float u1 = MathAddon.Lepr(first.U / first.W, third.U / third.W, alpha);
                    float v1 = MathAddon.Lepr(first.V / first.W, third.V / third.W, alpha);
                    float w_11 = MathAddon.Lepr(1 / first.W, 1 / third.W, alpha);

                    int x2;
                    float u2, v2, w_12;

                    if (yi >= second.ScreenY)
                    {
                        if (third.ScreenY - second.ScreenY != 0)
                        {
                            beta = (float)(yi - second.ScreenY) / (third.ScreenY - second.ScreenY);
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(second.ScreenX, third.ScreenX, beta));
                            u2 = MathAddon.Lepr(second.U / second.W, third.U / third.W, beta);
                            v2 = MathAddon.Lepr(second.V / second.W, third.V / third.W, beta);
                            w_12 = MathAddon.Lepr(1 / second.W, 1 / third.W, beta);
                        }
                        else
                        {
                            x2 = second.ScreenX;
                            u2 = second.U / second.W;
                            v2 = second.V / second.W;
                            w_12 = 1 / second.W;
                        }
                    }
                    else
                    {
                        if (second.ScreenY - first.ScreenY != 0)
                        {
                            beta = (float)(yi - first.ScreenY) / (second.ScreenY - first.ScreenY);
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(first.ScreenX, second.ScreenX, beta));
                            u2 = MathAddon.Lepr(first.U / first.W, second.U / second.W, beta);
                            v2 = MathAddon.Lepr(first.V / first.W, second.V / second.W, beta);
                            w_12 = MathAddon.Lepr(1 / first.W, 1 / second.W, beta);
                        }
                        else
                        {
                            x2 = second.ScreenX;
                            u2 = second.U / second.W;
                            v2 = second.V / second.W;
                            w_12 = 1 / second.W;
                        }
                    }

                    if (x1 > x2)
                    {
                        SystemAddon.Swap(ref x1, ref x2);
                        SystemAddon.Swap(ref u1, ref u2);
                        SystemAddon.Swap(ref v1, ref v2);
                    }

                    float zi = -(a * x1 + b * yi + d) / c;

                    for (int xi = Math.Max(x1, 0); xi < Math.Min(x2, width - 1); xi++, zi -= a / c)
                    {
                        Color texel;
                        float gamma = (float)(xi - x1) / (x2 - x1);
                        float u = x2 - x1 != 0 ? MathAddon.Lepr(u1, u2, gamma) : u1;
                        float v = x2 - x1 != 0 ? MathAddon.Lepr(v1, v2, gamma) : v1;
                        float w_1 = x2 - x1 != 0 ? MathAddon.Lepr(w_11, w_12, gamma) : w_11;

                        u /= w_1;
                        v /= w_1;

                        texel = pol.Texture.GetTexel(u, v);

                        pol.Pixels.Add(new FragmentInfo(xi, yi, zi, texel));
                    }
                }
            }
        }


        private void ZBuffer(List<PolygonInfo> pols)
        {
            FrameBuffer.Clear();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    DepthBuffer[i, j] = 1; // Установка макс. значения (см. однородное пространство отсечения)

            foreach (var p in pols)
            {
                foreach (var pixel in p.Pixels)
                {
                    if (pixel.Depth < DepthBuffer[pixel.ScreenX, pixel.ScreenY])
                    {
                        DepthBuffer[pixel.ScreenX, pixel.ScreenY] = pixel.Depth;

                        FrameBuffer.SetPixel(pixel.ScreenX, pixel.ScreenY, pixel.Color);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Класс для описания промежуточных данных при обработке полигонов для получения конечного изображения
    /// </summary>
    class PolygonInfo
    {
        public Vertex[] Vertices { get; set; }
        public Vector3 Normal { get; set; }
        public float[] LightLevelsOnVertices { get; set; }
        public float[] Ws { get; set; } // Четвёртая координата каждой вершины
        public ScreenVertex[] ScreenVertices { get; set; }
        public List<FragmentInfo> Pixels { get; set; }

        public Texture Texture { get; set; }

        public PolygonInfo(Vertex v1, Vertex v2, Vertex v3, Vector3 norm)
        {
            Vertices = new Vertex[] { v1, v2, v3 };
            Normal = norm;
            LightLevelsOnVertices = new float[3];
            Ws = new float[3];
            Pixels = new List<FragmentInfo>();
            ScreenVertices = new ScreenVertex[3];
        }

        public PolygonInfo(Vertex[] verts, Vector3 norm)
        {
            Vertices = verts;
            Normal = norm;
            LightLevelsOnVertices = new float[3];
            Ws = new float[3];
            Pixels = new List<FragmentInfo>();
            ScreenVertices = new ScreenVertex[3];
        }
    }

    class ScreenVertex
    {
        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector2 TextureCoords => new Vector2(U, V);
        public float U { get; set; }
        public float V { get; set; }

        public ScreenVertex(int x, int y, float z, float w, float u, float v)
        {
            ScreenX = x;
            ScreenY = y;
            Z = z;
            W = w;
            U = u;
            V = v;
        }

        public ScreenVertex(Vector3 v, float w, Vector2 uv)
        {
            ScreenX = (int)v.X;
            ScreenY = (int)v.Y;
            Z = v.Z;
            W = w;
            U = uv.X;
            V = uv.Y;
        }
    }

    /// <summary>
    /// Класс для описания точки в экранных координатах и её глубины
    /// </summary>
    class FragmentInfo
    {
        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public float Depth { get; set; }
        public Color Color { get; set; }

        public FragmentInfo(int x, int y, float z, Color c)
        {
            ScreenX = x;
            ScreenY = y;
            Depth = z;
            Color = c;
        }

        public FragmentInfo(Vector3 v, Color c)
        {
            ScreenX = (int)v.X;
            ScreenY = (int)v.Y;
            Depth = v.Z;
            Color = c;
        }
    }
}

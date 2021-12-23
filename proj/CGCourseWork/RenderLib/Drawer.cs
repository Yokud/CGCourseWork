using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;


namespace RenderLib
{
    public class Drawer
    {
        static object lock_obj;

        int width, height;
        public FastBitmap FrameBuffer { get; private set; }
        public float[,] DepthBuffer { get; private set; }

        public Drawer(int width, int height)
        {
            this.width = width;
            this.height = height;
            FrameBuffer = new FastBitmap(width, height);
            DepthBuffer = new float[width, height];

            lock_obj = new object();
        }

        public void DrawScene(Scene scene)
        {
            var pols = VerticesShading(scene);

            // тут ещё нужны этапы: растеризация, наложение текстур и теневой карты
            PixelShading(pols, scene.Camera, scene.LightSource);

            ZBuffer(pols);
        }

        private List<PolygonInfo> VerticesShading(Scene scene)
        {
            var matr_move = Matrix4x4.CreateTranslation(scene.Terrain.VisibleTerrainModel.Pivot.Center - scene.Camera.Pivot.Center);
            var model_view_matr = scene.Terrain.VisibleTerrainModel.ToWorldMatrix * matr_move * scene.Camera.Pivot.LocalCoordsMatrix * scene.Camera.PerspectiveClip;

            var vertices = new List<Vertex>(scene.Terrain.VisibleTerrainModel.Vertices);
            List<PolygonInfo> visible_pols = new List<PolygonInfo>();
            List<float> ws = new List<float>();
            List<float> light_levels = new List<float>();

            Vector3 light_dir = scene.LightSource.Pivot.ToGlobalCoords(scene.LightSource.LightDirection);

            foreach (var v in vertices)
            {
                Vector3 v_norm = scene.Terrain.VisibleTerrainModel.Pivot.ToGlobalCoords(v.Normal);

                float light_level = scene.LightSource.GetAngleIntensity(Vector3.Normalize(v_norm), Vector3.Normalize(light_dir)); 

                v.Transform(model_view_matr, out float w);
                ws.Add(w);
                light_levels.Add(light_level);
            }            

            foreach (var pol in scene.Terrain.VisibleTerrainModel.Polygons)
            {
                if (scene.Camera.IsVisible(vertices, pol))
                {
                    visible_pols.Add(new PolygonInfo(vertices[pol[0]], vertices[pol[1]], vertices[pol[2]], scene.Terrain.VisibleTerrainModel.GetPolNormal(pol).Transform(model_view_matr)));

                    for (int i = 0; i < 3; i++)
                    {
                        visible_pols[visible_pols.Count - 1].Ws[i] = ws[pol[i]];
                        visible_pols[visible_pols.Count - 1].LightLevelsOnVertices[i] = light_levels[pol[i]];
                    }
                }
            }

            return visible_pols;
        }

        private void PixelShading(List<PolygonInfo> pols, Camera cam, Light light)
        {
            foreach (var pol in pols)
            {
                for (int i = 0; i < 3; i++)
                    pol.ScreenVertices[i] = new ScreenVertex(cam.ScreenProjection(pol.Vertices[i].Position), pol.Ws[i], pol.Vertices[i].TextureCoords, pol.LightLevelsOnVertices[i]);

                var first = pol.ScreenVertices[0];
                var second = pol.ScreenVertices[1];
                var third = pol.ScreenVertices[2];

                int x_min, x_max, y_min, y_max;
                x_min = MathAddon.Min3(first.ScreenX, second.ScreenX, third.ScreenX);
                x_max = MathAddon.Max3(first.ScreenX, second.ScreenX, third.ScreenX);
                y_min = MathAddon.Min3(first.ScreenY, second.ScreenY, third.ScreenY);
                y_max = MathAddon.Max3(first.ScreenY, second.ScreenY, third.ScreenY);

                // Коэффициенты плоскости
                float a = (second.ScreenY - first.ScreenY) * (third.Z - first.Z) - (second.Z - first.Z) * (third.ScreenY - first.ScreenY);
                float b = (second.Z - first.Z) * (third.ScreenX - first.ScreenX) - (second.ScreenX - first.ScreenX) * (third.Z - first.Z);
                float c = (second.ScreenX - first.ScreenX) * (third.ScreenY - first.ScreenY) - (second.ScreenY - first.ScreenY) * (third.ScreenX - first.ScreenX);
                float d = -(a * first.ScreenX + b * first.ScreenY + c * first.Z);

                List<Point> outline_points = new List<Point>();

                for (int i = 0; i < 3; i++)
                {
                    var line = Brezenhem(pol.ScreenVertices[i % 3].ScreenX, pol.ScreenVertices[i % 3].ScreenY, pol.ScreenVertices[(i + 1) % 3].ScreenX, pol.ScreenVertices[(i + 1) % 3].ScreenY);
                    outline_points.AddRange(line);
                }
                
                var outline_segs = ClusterizeByY(outline_points);

                foreach (var seg in outline_segs)
                {
                    int yi = seg.Key;
                    int x1 = seg.Value.MinX, x2 = seg.Value.MaxX; ;

                    float det;
                    Vector3 bar_p1 = Baricentric(first.ScreenPos, second.ScreenPos, third.ScreenPos, new Point(x1, yi), out det);
                    Vector3 bar_p2 = Baricentric(first.ScreenPos, second.ScreenPos, third.ScreenPos, new Point(x2, yi), out det);

                    // Полигон проецируется в отрезок на экран
                    if (MathAddon.IsEqual(det, 0))
                        continue;

                    // Перспективно-корректное ткстурирование
                    float w_11 = 1f / first.W * bar_p1.X + 1f / second.W * bar_p1.Y + 1f / third.W * bar_p1.Z;
                    float w_12 = 1f / first.W * bar_p2.X + 1f / second.W * bar_p2.Y + 1f / third.W * bar_p2.Z;

                    Vector2 uv_w1 = first.TextureCoords / first.W * bar_p1.X + second.TextureCoords / second.W * bar_p1.Y + third.TextureCoords / third.W * bar_p1.Z;
                    Vector2 uv_w2 = first.TextureCoords / first.W * bar_p2.X + second.TextureCoords / second.W * bar_p2.Y + third.TextureCoords / third.W * bar_p2.Z;

                    float dw_1 = (w_12 - w_11) / (x2 - x1 + 1);
                    Vector2 d_uv_w = (uv_w2 - uv_w1) / (x2 - x1 + 1);

                    Vector2 uv_w = uv_w1;
                    float w_1 = w_11;


                    // Закраска Гуро
                    float I1 = bar_p1.X * first.Intensity + bar_p1.Y * second.Intensity + bar_p1.Z * third.Intensity;
                    float I2 = bar_p2.X * first.Intensity + bar_p2.Y * second.Intensity + bar_p2.Z * third.Intensity;

                    float dI = (I2 - I1) / (x2 - x1);
                    float I_start = I1;

                    float zi = -(a * x1 + b * yi + d) / c;
                    for (int xi = x1; xi <= x2; xi++)
                    {
                        Color texel = pol.Texture.GetTexel(uv_w.X / w_1, uv_w.Y / w_1);

                        pol.Pixels.Add(new FragmentInfo(xi, yi, zi, light.GetColorByIntensity(texel, I_start)));

                        uv_w += d_uv_w;
                        w_1 += dw_1;
                        I_start += dI;
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

        private Vector3 Baricentric(Point a, Point b, Point c, Point p, out float det_res)
        {
            Vector3 bar = new Vector3();

            float det = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            det_res = det;
            bar.X = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / det;
            bar.Y = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / det;
            bar.Z = 1 - bar.X - bar.Y;

            return bar;
        }

        private List<Point> Brezenhem(int x_start, int y_start, int x_end, int y_end)
        {
            List<Point> points = new List<Point>();

            bool swap_f;
            int dx = x_end - x_start, dy = y_end - y_start;
            int sx = Math.Sign(dx), sy = Math.Sign(dy);

            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            if ((swap_f = dy > dx))
                SystemAddon.Swap(ref dx, ref dy);

            int error = 2 * dy - dx;
            int x = x_start, y = y_start;
            for (int i = 0; i <= dx; i++)
            {
                points.Add(new Point(x, y));
                if (error >= 0)
                {
                    if (swap_f)
                        x += sx;
                    else
                        y += sy;
                    error -= 2 * dx;
                }

                if (swap_f)
                    y += sy;
                else
                    x += sx;

                error += 2 * dy;
            }

            return points;
        }

        private Dictionary<int, SegmentX> ClusterizeByY(List<Point> points)
        {
            Dictionary<int, SegmentX> outline = new Dictionary<int, SegmentX>();

            Parallel.ForEach(points, point =>
            {
                lock (lock_obj)
                {
                    if (!outline.ContainsKey(point.Y))
                        outline.Add(point.Y, new SegmentX(point.X, point.X));
                    else
                    {
                        outline[point.Y].MinX = Math.Min(outline[point.Y].MinX, point.X);
                        outline[point.Y].MaxX = Math.Max(outline[point.Y].MaxX, point.X);
                    }
                }
            });

            return outline;
        }


        class SegmentX
        {
            public int MinX { get; set; }
            public int MaxX { get; set; }

            public SegmentX(int x_min, int x_max)
            {
                MinX = x_min;
                MaxX = x_max;
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

        public Point ScreenPos => new Point(ScreenX, ScreenY);
        public Vector2 TextureCoords => new Vector2(U, V);
        public float U { get; set; }
        public float V { get; set; }

        public float Intensity { get; set; }

        public ScreenVertex(int x, int y, float z, float w, float u, float v, float i)
        {
            ScreenX = x;
            ScreenY = y;
            Z = z;
            W = w;
            U = u;
            V = v;
            Intensity = i;
        }

        public ScreenVertex(Vector3 v, float w, Vector2 uv, float i)
        {
            ScreenX = (int)v.X;
            ScreenY = (int)v.Y;
            Z = v.Z;
            W = w;
            U = uv.X;
            V = uv.Y;
            Intensity = i;
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

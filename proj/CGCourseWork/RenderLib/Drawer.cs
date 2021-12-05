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
            var matr_move = Matrix4x4.CreateTranslation(scene.Model.Pivot.Center - scene.Camera.Pivot.Center);
            var model_view_matr = scene.Model.ToWorldMatrix * matr_move * scene.Camera.Pivot.LocalCoordsMatrix * scene.Camera.PerspectiveClip;

            var temp_model = (PolModel)scene.Model.Clone();
            List<float> w_list = new List<float>();

            foreach (var v in temp_model.Vertices)
            {
                v.Transform(model_view_matr);
            }

            List<int> visible_pols = new List<int>();
            for (int i = 0; i < temp_model.Polygons.Count; i++)
            {
                if (scene.Camera.IsVisible(temp_model, i))
                    visible_pols.Add(i);
            }

            ZBuffer(scene.Camera, temp_model, visible_pols, w_list);
        }

        private void ZBuffer(Camera cam, PolModel model, List<int> vis_pols, List<float> w_list)
        {
            Color texel;

            FrameBuffer.Clear();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    DepthBuffer[i, j] = 1; // Установка макс. значения (см. однородное пространство отсечения)

            foreach (int i in vis_pols)
            {
                Vertex first = new Vertex(cam.ScreenProjection(model.Vertices[model.Polygons[i][0]].Position), model.Vertices[model.Polygons[i][0]].TextureCoords);
                Vertex second = new Vertex(cam.ScreenProjection(model.Vertices[model.Polygons[i][1]].Position), model.Vertices[model.Polygons[i][1]].TextureCoords);
                Vertex third = new Vertex(cam.ScreenProjection(model.Vertices[model.Polygons[i][2]].Position), model.Vertices[model.Polygons[i][2]].TextureCoords);

                if (first.Position.Y > second.Position.Y)
                    SystemAddon.Swap(ref first, ref second);
                if (first.Position.Y > third.Position.Y)
                    SystemAddon.Swap(ref first, ref third);
                if (second.Position.Y > third.Position.Y)
                    SystemAddon.Swap(ref second, ref third);

                float min_y = first.Position.Y;
                float max_y = third.Position.Y;

                // Коэффициенты плоскости полигона
                float a = (second.Position.Y - first.Position.Y) * (third.Position.Z - first.Position.Z) - (second.Position.Z - first.Position.Z) * (third.Position.Y - first.Position.Y);
                float b = (second.Position.Z - first.Position.Z) * (third.Position.X - first.Position.X) - (second.Position.X - first.Position.X) * (third.Position.Z - first.Position.Z);
                float c = (second.Position.X - first.Position.X) * (third.Position.Y - first.Position.Y) - (second.Position.Y - first.Position.Y) * (third.Position.X - first.Position.X);
                float d = -(a * first.Position.X + b * first.Position.Y + c * first.Position.Z);

                float u_min = MathAddon.Min3(first.TextureCoords.X, second.TextureCoords.X, third.TextureCoords.X);
                float u_max = MathAddon.Max3(first.TextureCoords.X, second.TextureCoords.X, third.TextureCoords.X);

                float v_min = MathAddon.Min3(first.TextureCoords.Y, second.TextureCoords.Y, third.TextureCoords.Y);
                float v_max = MathAddon.Max3(first.TextureCoords.Y, second.TextureCoords.Y, third.TextureCoords.Y);

                for (int yi = Math.Max(0, MathAddon.RoundToInt(min_y)); yi <= Math.Min(max_y, height - 1); yi++)
                {
                    int x1 = MathAddon.RoundToInt(MathAddon.Lepr(first.Position.X, third.Position.X, (yi - min_y) / (max_y - min_y)));
                    int x2;

                    if (yi >= second.Position.Y)
                    {
                        if (third.Position.Y - second.Position.Y != 0)
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(second.Position.X, third.Position.X, (yi - second.Position.Y) / (third.Position.Y - second.Position.Y)));
                        else
                            x2 = MathAddon.RoundToInt(second.Position.X);
                    }
                    else
                    {
                        if (second.Position.Y - first.Position.Y != 0)
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(first.Position.X, second.Position.X, (yi - first.Position.Y) / (second.Position.Y - first.Position.Y)));
                        else
                            x2 = MathAddon.RoundToInt(second.Position.X);
                    }

                    if (x1 > x2)
                    {
                        SystemAddon.Swap(ref x1, ref x2);
                    }  

                    float zi = -(a * x1 + b * yi + d) / c;

                    float min_x = MathAddon.Min3(first.Position.X, second.Position.X, third.Position.X);
                    float max_x = MathAddon.Max3(first.Position.X, second.Position.X, third.Position.X);

                    for (int xi = Math.Max(x1, 0); xi <= Math.Min(x2, width - 1); xi++, zi -= a / c)
                    {
                        if (zi < DepthBuffer[xi, yi])
                        {
                            DepthBuffer[xi, yi] = zi;

                            float u = MathAddon.Lepr(u_min, u_max, (xi - min_x) / (max_x - min_x));
                            float v = MathAddon.Lepr(v_min, v_max, (yi - min_y) / (max_y - min_y));

                            texel = model.Texture.GetTexel(u, v);

                            FrameBuffer.SetPixel(xi, yi, texel);
                        }
                    }
                }
            }
        }
    }
}

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
            var matr_move = Matrix4x4.CreateTranslation(scene.Model.Pivot.Center) * Matrix4x4.CreateTranslation(-scene.Camera.Pivot.Center);
            var model_view_matr = scene.Model.ToWorldMatrix * matr_move * scene.Camera.Pivot.LocalCoordsMatrix * scene.Camera.PerspectiveClip;

            var temp_model = (PolModel)scene.Model.Clone();

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
            //temp_model.DeleteComponents(deleted_pols);

            ZBuffer(scene.Camera, temp_model, visible_pols);
        }

        private void ZBuffer(Camera cam, PolModel model, List<int> vis_pols)
        {
            Color texel;

            FrameBuffer.Clear();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    DepthBuffer[i, j] = 1; // Установка мин. значения (см. однородное пространство отсечения)

            foreach (int i in vis_pols)
            {
                Vector3 first = cam.ScreenProjection(model.Vertices[model.Polygons[i][0]].Position);
                Vector3 second = cam.ScreenProjection(model.Vertices[model.Polygons[i][1]].Position);
                Vector3 third = cam.ScreenProjection(model.Vertices[model.Polygons[i][2]].Position);

                float min_u = MathAddon.Min3(model.Vertices[model.Polygons[i][0]].TextureCoords.X, model.Vertices[model.Polygons[i][1]].TextureCoords.X, model.Vertices[model.Polygons[i][2]].TextureCoords.X);
                float max_u = MathAddon.Max3(model.Vertices[model.Polygons[i][0]].TextureCoords.X, model.Vertices[model.Polygons[i][1]].TextureCoords.X, model.Vertices[model.Polygons[i][2]].TextureCoords.X);

                float min_v = MathAddon.Min3(model.Vertices[model.Polygons[i][0]].TextureCoords.Y, model.Vertices[model.Polygons[i][1]].TextureCoords.Y, model.Vertices[model.Polygons[i][2]].TextureCoords.Y);
                float max_v = MathAddon.Max3(model.Vertices[model.Polygons[i][0]].TextureCoords.Y, model.Vertices[model.Polygons[i][1]].TextureCoords.Y, model.Vertices[model.Polygons[i][2]].TextureCoords.Y);

                if (first.Y > second.Y)
                    SystemAddon.Swap(ref first, ref second);
                if (first.Y > third.Y)
                    SystemAddon.Swap(ref first, ref third);
                if (second.Y > third.Y)
                    SystemAddon.Swap(ref second, ref third);

                float min_x = first.X;
                float max_x = third.X;

                float min_y = first.Y;
                float max_y = third.Y;

                // Коэффициенты плоскости полигона
                float a = (second.Y - first.Y) * (third.Z - first.Z) - (second.Z - first.Z) * (third.Y - first.Y);
                float b = (second.Z - first.Z) * (third.X - first.X) - (second.X - first.X) * (third.Z - first.Z);
                float c = (second.X - first.X) * (third.Y - first.Y) - (second.Y - first.Y) * (third.X - first.X);
                float d = -(a * first.X + b * first.Y + c * first.Z);

                for (int yi = Math.Max(0, MathAddon.RoundToInt(min_y)); yi <= Math.Min(max_y, height - 1); yi++)
                {
                    int x1 = MathAddon.RoundToInt(MathAddon.Lepr(min_x, max_x, (yi - min_y) / (max_y - min_y)));
                    int x2;

                    if (yi >= second.Y)
                    {
                        if (third.Y - second.Y != 0)
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(second.X, third.X, (yi - second.Y) / (third.Y - second.Y)));
                        else
                            x2 = MathAddon.RoundToInt(Math.Max(third.X, second.X));
                    }
                    else
                    {
                        if (second.Y - first.Y != 0)
                            x2 = MathAddon.RoundToInt(MathAddon.Lepr(first.X, second.X, (yi - first.Y) / (second.Y - first.Y)));
                        else
                            x2 = MathAddon.RoundToInt(Math.Max(second.X, first.X));
                    }

                    if (x1 > x2)
                        SystemAddon.Swap(ref x1, ref x2);

                    float zi = -(a * x1 + b * yi + d) / c;
                    float z_start = zi, z_end = zi + -a / c * (x2 - x1 + 1);

                    float u1 = MathAddon.Lepr(min_u, max_u, (x1 - MathAddon.Min3(first.X, second.X, third.X)) / (MathAddon.Max3(first.X, second.X, third.X) - MathAddon.Min3(first.X, second.X, third.X)));
                    float u2 = MathAddon.Lepr(min_u, max_u, (x2 - MathAddon.Min3(first.X, second.X, third.X)) / (MathAddon.Max3(first.X, second.X, third.X) - MathAddon.Min3(first.X, second.X, third.X)));

                    float v1 = MathAddon.Lepr(min_v, max_v, (yi - min_y) / (max_y - min_y));

                    for (int xi = Math.Max(x1, 0); xi <= Math.Min(x2, width - 1); xi++, zi -= a / c)
                    {
                        if (zi < DepthBuffer[xi, yi])
                        {
                            DepthBuffer[xi, yi] = zi;

                            float u = x2 - x1 != 0 ? MathAddon.Lepr(u1 / z_start, u2 / z_end, ((float)xi - x1) / (x2 - x1)) : u1 / z_start;
                            float v = MathAddon.Lepr(v1 / z_start, v1 / z_end, (yi - min_y) / (max_y - min_y));

                            u /= 1 / zi;
                            v /= 1 / zi;

                            texel = model.Texture.GetTexel(u, v);

                            FrameBuffer.SetPixel(xi, yi, texel);
                        }
                    }
                }
            }
        }
    }
}

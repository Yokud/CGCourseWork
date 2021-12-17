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
            var pols = GetVisiblePols(scene);

            // тут ещё нужны этапы: растеризация, наложение текстур и теневой карты

            ZBuffer(pols);
        }

        private List<PolygonInfo> GetVisiblePols(Scene scene)
        {
            // тут нужно добавить расчёт освещения по Ламберту

            var matr_move = Matrix4x4.CreateTranslation(scene.Model.Pivot.Center - scene.Camera.Pivot.Center);
            var model_view_matr = scene.Model.ToWorldMatrix * matr_move * scene.Camera.Pivot.LocalCoordsMatrix * scene.Camera.PerspectiveClip;

            var temp_model = (PolModel)scene.Model.Clone();
            List<PolygonInfo> visible_pols = new List<PolygonInfo>();

            foreach (var v in temp_model.Vertices)
                v.Transform(model_view_matr);

            foreach (var pol in temp_model.Polygons)
            {
                if (scene.Camera.IsVisible(temp_model, pol))
                    visible_pols.Add(new PolygonInfo(temp_model.GetPolVertices(pol), temp_model.GetPolNormal(pol)));
            }

            return visible_pols;
        }

        private void ZBuffer(List<PolygonInfo> pols)
        {
            FrameBuffer.Clear();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    DepthBuffer[i, j] = 1; // Установка макс. значения (см. однородное пространство отсечения)

            foreach (var p in pols)
            {
                foreach (var pixel in p.pixels)
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
        public FragmentInfo[] ScreenVertices { get; set; }
        public List<FragmentInfo> pixels { get; set; }

        public PolygonInfo(Vertex v1, Vertex v2, Vertex v3, Vector3 norm)
        {
            Vertices = new Vertex[] { v1, v2, v3 };
            Normal = norm;
            LightLevelsOnVertices = new float[3];
            Ws = new float[3];
            pixels = new List<FragmentInfo>();
            ScreenVertices = new FragmentInfo[3];
        }

        public PolygonInfo(Vertex[] verts, Vector3 norm)
        {
            Vertices = verts;
            Normal = norm;
            LightLevelsOnVertices = new float[3];
            Ws = new float[3];
            pixels = new List<FragmentInfo>();
            ScreenVertices = new FragmentInfo[3];
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
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RenderLib;

namespace UI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Vector3 center = new Vector3(0, 0, 0);
            float sideLen = 100;
            var d = sideLen / 2;
            List<Vertex> Vertices = new List<Vertex>()
                {
                    new Vertex(new Vector3(center.X - d, center.Y - d, center.Z), new Vector2(0, 0)),
                    new Vertex(new Vector3(center.X - d, center.Y + d, center.Z), new Vector2(0, 1)),
                    new Vertex(new Vector3(center.X + d, center.Y - d, center.Z), new Vector2(1, 0)),
                    new Vertex(new Vector3(center.X + d, center.Y + d, center.Z), new Vector2(1, 1)),
                };

            List<RenderLib.Polygon> Indexes = new List<RenderLib.Polygon>()
                {
                    new RenderLib.Polygon(0, 1, 2),
                    new RenderLib.Polygon(3, 2, 1),
                };

            PolModel model = new PolModel(Vertices, Indexes, Pivot.BasePivot(center));
            //model.Rotate((float)Math.PI * 17f / 36f, Axis.Y);

            Camera cam = new Camera(Pivot.BasePivot(0, 40, 600), 512, 512, 10, 1000);
            cam.RotateAt(center, -(float)Math.PI / 4, Axis.X);

            DirectionalLight light = new DirectionalLight(Pivot.BasePivot(0, 50, 100), Vector3.Normalize(new Vector3(0, -50, -100)));

            Terrain terr = new Terrain(300, 300, 64, 64);

            Scene scene = new Scene(terr, cam, light);
            Drawer draw = new Drawer(cam.ScreenWidth, cam.ScreenHeight);
            Facade fac = new Facade(scene, draw);

            MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
        }
    }

    public static partial class SystemAddon
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}

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
                    new Vertex(new Vector3(center.X - d , center.Y - d, center.Z - d), new Vector2(1, 0)) ,
                    new Vertex(new Vector3(center.X - d , center.Y - d, center.Z + d), new Vector2(0, 0)) ,
                    new Vertex(new Vector3(center.X - d , center.Y + d, center.Z - d), new Vector2(1, 1)) ,
                    new Vertex(new Vector3(center.X - d , center.Y + d, center.Z + d), new Vector2(0, 1)) ,
                    new Vertex(new Vector3(center.X + d , center.Y - d, center.Z - d), new Vector2(0, 0)) ,
                    new Vertex(new Vector3(center.X + d , center.Y - d, center.Z + d), new Vector2(1, 1)) ,
                    new Vertex(new Vector3(center.X + d , center.Y + d, center.Z - d), new Vector2(0, 1)) ,
                    new Vertex(new Vector3(center.X + d , center.Y + d, center.Z + d), new Vector2(1, 0)) ,
                };

            List<RenderLib.Polygon> Indexes = new List<RenderLib.Polygon>()
                {
                    new RenderLib.Polygon(0,1,3) ,
                    new RenderLib.Polygon(0,2,3) ,
                    new RenderLib.Polygon(0,1,5) ,
                    new RenderLib.Polygon(0,4,5) ,
                    new RenderLib.Polygon(4,5,7) ,
                    new RenderLib.Polygon(4,6,7) ,
                    new RenderLib.Polygon(7,3,2) ,
                    new RenderLib.Polygon(7,6,2) ,
                    new RenderLib.Polygon(3,1,7) ,
                    new RenderLib.Polygon(1,7,5) ,
                    new RenderLib.Polygon(2,0,6) ,
                    new RenderLib.Polygon(0,6,4)
                };

            PolModel model = new PolModel(Vertices, Indexes, new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\UI\imgs\test.png"), Pivot.BasePivot(center));
            model.Rotate((float)Math.PI / 4, Axis.Y);

            Camera cam = new Camera(Pivot.BasePivot(0, 0, 150f), 400, 480, 10, 100);
            cam.RotateAt(center, -(float)Math.PI / 4, Axis.X);

            Scene scene = new Scene(model, cam);
            Drawer draw = new Drawer(cam.ScreenWidth, cam.ScreenHeight);
            Facade fac = new Facade(scene, draw);

            MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
        }
    }

    public static class ABC
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

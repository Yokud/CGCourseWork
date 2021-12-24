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

            Camera cam = new Camera(Pivot.BasePivot(0, 50, 600), 512, 512, 10, 1000);
            cam.RotateAt(center, -(float)Math.PI / 4, Axis.X);

            DirectionalLight light = new DirectionalLight(Pivot.BasePivot(0, 50, 100), Vector3.Normalize(new Vector3(0, -50, -100)));

            List<Texture> textures = new List<Texture>() {new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\water.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\sand.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\grass.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\rock.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\snow.jpg")};
            Terrain terr = new Terrain(300, 300, 64, 64, textures);

            Scene scene = new Scene(terr, cam, light);
            Drawer draw = new Drawer(cam.ScreenWidth, cam.ScreenHeight);
            Facade fac = new Facade(scene, draw);

            fac.RotateTerrain((float)Math.PI / 4, Axis.Y);
            fac.MoveTerrain(10, 10);

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

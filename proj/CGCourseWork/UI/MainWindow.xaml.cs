using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
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
using HeightMapLib;
using RenderLib;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace UI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Texture> textures;
        Facade fac;
        Vector3 Center;
        public HeightMap Map { get; private set; }
        int visWidth, visHeight;

        double rot_dx = 0, rot_dy = 0, rot_dz = 0;
        private bool dragStarted1 = false;
        private bool dragStarted2 = false;
        private bool dragStarted3 = false;

        CultureInfo ci;


        VideoCapture capture;

        public MainWindow()
        {
            InitializeComponent();

            Center = new Vector3(0, 0, 0);

            textures = new List<Texture>() {new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\water.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\sand.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\grass.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\rock.jpg"),
                                                            new Texture(@"D:\Repos\CGCourseWork\proj\CGCourseWork\textures\snow.jpg")};

            ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            capture = new VideoCapture(0);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            GenMapWindow mapWindow = new GenMapWindow();

            if (mapWindow.ShowDialog() == true)
            {
                int seed = -1, octs = 1;
                if (mapWindow.seed)
                    seed = mapWindow.Seed;

                if (mapWindow.oct)
                    octs = mapWindow.Octaves;

                float lacun = 2f, pers = 0.5f;
                if (mapWindow.lac)
                    lacun = mapWindow.Lacunarity;
                if (mapWindow.pers)
                    pers = mapWindow.Persistence;

                Map = new HeightMap(mapWindow.MapWidth, mapWindow.MapHeight, new PerlinNoise(mapWindow.Scale, octs, lacun, pers, seed));
                StateText.Text = $"Карта высот была сгенерирована с параметрами: {mapWindow.MapWidth} {mapWindow.MapHeight} {mapWindow.Scale} {3} {octs} {lacun} {pers} {seed}";
            }
        }

        private void ChangeAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int dx = int.Parse(DeltaX_textbox.Text);
                int dy = int.Parse(DeltaY_textbox.Text);

                fac.MoveTerrain(dx, dy);

                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void RotateAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int d_angle = int.Parse(DeltaAngle_textbox.Text);

                fac.RotateTerrain(MathAddon.DegToRad(d_angle), Axis.Y);

                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void ScaleAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float kx = float.Parse(ScaleX_textbox.Text, NumberStyles.Any, ci);
                float ky = float.Parse(ScaleY_textbox.Text, NumberStyles.Any, ci);
                float kz = float.Parse(ScaleZ_textbox.Text, NumberStyles.Any, ci);

                fac.ScaleTerrain(kx, ky, kz);

                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void slValue1_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragStarted1 = true;
        }

        private void slValue1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            fac.RotateLight(MathAddon.DegToRad((float)(slValue1.Value - rot_dx)), Axis.X);
            rot_dx = slValue1.Value;

            dragStarted1 = false;

            MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
        }

        private void slValue2_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragStarted2 = true;
        }

        private void slValue2_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            fac.RotateLight(MathAddon.DegToRad((float)(slValue2.Value - rot_dy)), Axis.Y);
            rot_dy = slValue2.Value;

            dragStarted2 = false;

            MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
        }

        private void slValue1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted1 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue1.Value - rot_dx)), Axis.X);
                rot_dx = slValue1.Value;
                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
        }

        private void slValue2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted2 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue2.Value - rot_dy)), Axis.Y);
                rot_dy = slValue2.Value;
                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
        }

        private void slValue3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted3 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue3.Value - rot_dz)), Axis.Z);
                rot_dz = slValue3.Value;
                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
            }
        }

        private void slValue3_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragStarted3 = true;
        }

        private void slValue3_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            fac.RotateLight(MathAddon.DegToRad((float)(slValue3.Value - rot_dz)), Axis.Z);
            rot_dz = slValue3.Value;

            dragStarted3 = false;

            MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();
        }

        private async void CreateLand_Click(object sender, RoutedEventArgs e)
        {
            SetLimitWindow limWindow = new SetLimitWindow();

            limWindow.Owner = this;

            if (limWindow.ShowDialog() == true)
            {
                visWidth = limWindow.VisWidth;
                visHeight = limWindow.VisHeight;
                StateText.Text += $" {visWidth} {visHeight}";


                Camera cam = new Camera(Pivot.BasePivot(0, 50, 600), 512, 512, 10, 1000);
                cam.RotateAt(Center, -(float)Math.PI / 4, Axis.X);

                DirectionalLight light = new DirectionalLight(Pivot.BasePivot(0, 0, 700), new Vector3(0, 0, -1));

                Terrain terr = new Terrain(Map, visWidth, visHeight, textures);

                Scene scene = new Scene(terr, cam, light);
                Drawer draw = new Drawer(cam.ScreenWidth, cam.ScreenHeight);
                fac = new Facade(scene, draw);
                fac.RotateLight(MathAddon.DegToRad((float)slValue1.Value), Axis.X);
                fac.RotateLight(MathAddon.DegToRad((float)slValue2.Value), Axis.Y);
                fac.RotateLight(MathAddon.DegToRad((float)slValue3.Value), Axis.Z);
                rot_dx = slValue1.Value;
                rot_dy = slValue2.Value;
                rot_dz = slValue3.Value;

                MainFrame.Source = fac.DrawScene().Bitmap.ToBitmapImage();

                ChangeTerrainView.IsEnabled = true;
                RotateTerrainView.IsEnabled = true;
                ScaleTerrainView.IsEnabled = true;
                RotateLightSourse.IsEnabled = true;

                await Task.Run(() => Capturing());
            }
        }


        static void Capturing()
        {
            int markersX = 1;
            int markersY = 1;
            int markersLength = 80;
            int markersSeparation = 30;
            Dictionary ArucoDict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100);
            GridBoard ArucoBoard = new GridBoard(markersX, markersY, markersLength, markersSeparation, ArucoDict);
            PrintArucoBoard(ArucoBoard, markersX, markersY, markersLength, markersSeparation);
        }

        static void PrintArucoBoard(GridBoard ArucoBoard, int markersX = 1, int markersY = 1, int markersLength = 80, int markersSeparation = 30)
        {
            int borderBits = 1;

            System.Drawing.Size imageSize = new System.Drawing.Size();
            Mat boardImage = new Mat();
            imageSize.Width = markersX * (markersLength + markersSeparation) - markersSeparation + 2 * markersSeparation;
            imageSize.Height = markersY * (markersLength + markersSeparation) - markersSeparation + 2 * markersSeparation;
            ArucoBoard.Draw(imageSize, boardImage, markersSeparation, borderBits);

            var img = boardImage.ToBitmap();
            img.Save(@"D:\Repos\CGCourseWork\proj\CGCourseWork\markers\aruco.png");
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

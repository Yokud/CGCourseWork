﻿using System;
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


        static VideoCapture capture;
        bool captured = false;

        public MainWindow()
        {
            InitializeComponent();

            Center = new Vector3(0, 0, 0);

            textures = new List<Texture>() {new Texture(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\textures\water.jpg"),
                                                            new Texture(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\textures\sand.jpg"),
                                                            new Texture(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\textures\grass.jpg"),
                                                            new Texture(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\textures\rock.jpg"),
                                                            new Texture(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\textures\snow.jpg")};

            ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            try
            {
                capture = new VideoCapture(0);
                captured = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                Close();
            }
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
                StateText.Text = $"Карта высот была сгенерирована с параметрами: {Map.Width} {Map.Height} {Map.Scale} {Map.Octaves} {Map.Lacunarity} {Map.Persistence} {Map.Seed}";
            }
    
        }

        private void ChangeAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int dx = int.Parse(DeltaX_textbox.Text);
                int dy = int.Parse(DeltaY_textbox.Text);

                fac.MoveTerrain(dx, dy);
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

                fac.RotateTerrain(MathAddon.DegToRad(d_angle), Axis.Z);
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
        }

        private void slValue1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted1 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue1.Value - rot_dx)), Axis.X);
                rot_dx = slValue1.Value;
            }
        }

        private void slValue2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted2 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue2.Value - rot_dy)), Axis.Y);
                rot_dy = slValue2.Value;
            }
        }

        private void slValue3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted3 && RotateLightSourse.IsEnabled)
            {
                fac.RotateLight(MathAddon.DegToRad((float)(slValue3.Value - rot_dz)), Axis.Z);
                rot_dz = slValue3.Value;
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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            captured = false;
            capture.Dispose();
        }

        private void CreateLand_Click(object sender, RoutedEventArgs e)
        {
            SetLimitWindow limWindow = new SetLimitWindow();

            limWindow.Owner = this;

            if (limWindow.ShowDialog() == true)
            {
                visWidth = limWindow.VisWidth;
                visHeight = limWindow.VisHeight;
                StateText.Text += $" {visWidth} {visHeight}";


                Camera cam = new Camera(Pivot.BasePivot(0, 0, 0), 640, 480, 150, 100000);

                AreaLight light = new AreaLight(Pivot.BasePivot(0, -60, 700), new Vector3(0, 0, -1), width:640, height:480);

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

                ChangeTerrainView.IsEnabled = true;
                RotateTerrainView.IsEnabled = true;
                ScaleTerrainView.IsEnabled = true;
                RotateLightSourse.IsEnabled = true;

                Capturing();
            }
        }


        void Capturing()
        {
            int markersX = 1;
            int markersY = 1;
            int markersLength = 160;
            int markersSeparation = 10;
            Dictionary ArucoDict = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_100);
            GridBoard ArucoBoard = new GridBoard(markersX, markersY, markersLength, markersSeparation, ArucoDict);
            //PrintArucoBoard(ArucoBoard, markersX, markersY, markersLength, markersSeparation);

            DetectorParameters ArucoParameters = DetectorParameters.GetDefault();

            string cameraConfigurationFile = @"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\cameraParameters.xml";
            FileStorage fs = new FileStorage(cameraConfigurationFile, FileStorage.Mode.Read);
            if (!fs.IsOpened)
            {
                Console.WriteLine("Could not open configuration file " + cameraConfigurationFile);
                return;
            }
            Mat cameraMatrix = new Mat(new System.Drawing.Size(3, 3), DepthType.Cv32F, 1);
            Mat distortionMatrix = new Mat(1, 8, DepthType.Cv32F, 1);
            fs["cameraMatrix"].ReadMat(cameraMatrix);
            fs["dist_coeffs"].ReadMat(distortionMatrix);

            while (captured)
            {
                Mat frame = new Mat();
                frame = capture.QueryFrame();

                if (!frame.IsEmpty)
                {
                    VectorOfInt ids = new VectorOfInt(); // name/id of the detected markers
                    VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF(); // corners of the detected marker
                    VectorOfVectorOfPointF rejected = new VectorOfVectorOfPointF(); // rejected contours
                    ArucoInvoke.DetectMarkers(frame, ArucoDict, corners, ids, ArucoParameters, rejected);

                    // If we detected at least one marker
                    if (ids.Size != 0)
                    {
                        ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(255, 0, 255));

                        Mat rvecs = new Mat(); // rotation vector
                        Mat tvecs = new Mat(); // translation vector
                        ArucoInvoke.EstimatePoseSingleMarkers(corners, markersLength, cameraMatrix, distortionMatrix, rvecs, tvecs);

                        Mat rvecMat = rvecs.Row(0);
                        Mat tvecMat = tvecs.Row(0);
                        VectorOfDouble rvec = new VectorOfDouble();
                        VectorOfDouble tvec = new VectorOfDouble();
                        
                        double[] values = new double[3];
                        rvecMat.CopyTo(values);
                        rvec.Push(values);
                        tvecMat.CopyTo(values);
                        tvec.Push(values);
                        ArucoInvoke.DrawAxis(frame,
                                                cameraMatrix,
                                                distortionMatrix,
                                                rvec,
                                                tvec,
                                                markersLength * 0.5f);

                        Matrix4x4 rot = GetRotationMatrixFromRotationVector(rvec).ToMatrix4x4();
                        
                        Vector4 t = new Vector4((float)tvec[0], (float)tvec[1], (float)tvec[2], 1);

                        //Console.WriteLine("Marker pos: " + t.ToString());

                        //t = Vector4.Transform(t, -rot); // положение камеры относительно маркера
                        Vector3 pos = new Vector3(t.X, t.Y, t.Z);

                        rot = Matrix4x4.Transpose(rot);
                        Vector3 x_a = new Vector3(rot.M11, rot.M12, rot.M13);
                        Vector3 y_a = new Vector3(rot.M21, rot.M22, rot.M23);
                        Vector3 z_a = new Vector3(rot.M31, rot.M32, rot.M33);

                        fac.SetCamera(pos, x_a, y_a, z_a);

                        MainFrame.Source = CVAddon.ConcatTwoImages(frame.ToImage<Rgba, byte>().Flip(FlipType.Horizontal).ToBitmap(), fac.DrawScene(false).Bitmap).ToBitmapImage();
                    }
                    else
                        MainFrame.Source = frame.ToImage<Rgba, byte>().Flip(FlipType.Horizontal).ToBitmap().ToBitmapImage();
     
                    CvInvoke.WaitKey(24);
                }
            }
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
            img.Save(@"D:\Repos\GitHub\CGCourseWork\proj\CGCourseWork\markers\aruco.png");
        }

        Mat GetRotationMatrixFromRotationVector(VectorOfDouble rvec)
        {
            Mat rmat = new Mat();
            CvInvoke.Rodrigues(rvec, rmat);

            return rmat;
        }
    }

    public static class CVAddon
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

        public static Bitmap ConcatTwoImages(Bitmap b1, Bitmap b2)
        {
            var concat = new Bitmap(b1);
            var gr = Graphics.FromImage(concat);
            gr.DrawImage(b2, 0, 0);

            return concat;
        }

        public static Matrix4x4 ToMatrix4x4(this Affine3d m)
        {
            double[] temp = m.GetValues();

            Matrix4x4 matr = new Matrix4x4() { M11 = (float)temp[0], M12 = (float)temp[1], M13 = (float)temp[2], 
                                               M21 = (float)temp[3], M22 = (float)temp[4], M23 = (float)temp[5], 
                                               M31 = (float)temp[6], M32 = (float)temp[7], M33 = (float)temp[8], 
                                               M44 = 1 };

            return matr;
        }

        public static Matrix4x4 ToMatrix4x4(this Mat m)
        {
            double[,] tmp = (double[,])m.GetData();

            double[] temp = new double[tmp.Length];

            int i = 0;
            foreach (double d in tmp)
                temp[i++] = d;

            Matrix4x4 matr = new Matrix4x4()
            {
                M11 = (float)temp[0],
                M12 = (float)temp[1],
                M13 = (float)temp[2],
                M21 = (float)temp[3],
                M22 = (float)temp[4],
                M23 = (float)temp[5],
                M31 = (float)temp[6],
                M32 = (float)temp[7],
                M33 = (float)temp[8],
                M44 = 1
            };

            return matr;
        }
    }
}

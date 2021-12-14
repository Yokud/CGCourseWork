using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RenderLib
{
    public interface IProjectable
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }

        int ScreenNearDist { get; }
        int ScreenFarDist { get; }

        Matrix4x4 PerspectiveClip { get; }

        Matrix4x4 OrtogonalClip { get; }

        bool IsVisible(Vector3 p);
        bool IsVisible(Vertex v);
        bool IsVisible(PolModel model, int pol_num);
        Vector3 ScreenProjection(Vector3 p);
    }

    public class Camera : Object3D, IProjectable
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public int ScreenNearDist { get; private set; }
        public int ScreenFarDist { get; private set; }

        private static double fov = Math.PI / 2;
        private float r, t, tg_fov = (float)Math.Tan(fov / 2);

        public Camera(Pivot p, int width, int height, int near_dist, int far_dist)
        {
            Pivot = p;
            ScreenWidth = width;
            ScreenHeight = height;
            ScreenNearDist = near_dist;
            ScreenFarDist = far_dist;

            t = ScreenNearDist * tg_fov;
            r = t * ((float)width / height);
        }

        public Matrix4x4 PerspectiveClip => new Matrix4x4
        (
            ScreenNearDist / r, 0, 0, 0,
            0, ScreenNearDist / t, 0, 0,
            0, 0, (ScreenFarDist + ScreenNearDist) / (ScreenNearDist - ScreenFarDist), -1,
            0, 0, 2 * ScreenNearDist * ScreenFarDist / (ScreenNearDist - ScreenFarDist), 0
        );

        public Matrix4x4 OrtogonalClip => new Matrix4x4
        (
            1f / r, 0, 0, 0,
            0, 1f / t, 0, 0,
            0, 0, -2.0f / (ScreenFarDist - ScreenNearDist), 0,
            0, 0, (ScreenFarDist + ScreenNearDist) / (ScreenNearDist - ScreenFarDist), 1
        );

        public Vector3 Position
        {
            get { return Pivot.Center; }
        }

        public bool IsVisible(Vector3 p)
        {
            float min = -1, max = 1;

            return min <= p.X && p.X <= max && min <= p.Y && p.Y <= max && min <= p.Z && p.Z <= max;
        }

        public bool IsVisible(Vertex v)
        {
            return IsVisible(v.Position);
        }

        public bool IsVisible(PolModel model, int pol_num)
        {
            return IsVisible(model.Vertices[model.Polygons[pol_num][0]]) && IsVisible(model.Vertices[model.Polygons[pol_num][1]]) && IsVisible(model.Vertices[model.Polygons[pol_num][2]]);
        }

        public override void Move(float dx, float dy, float dz)
        {
            Pivot.Move(dx, dy, dz);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
        }

        public override void Scale(float kx, float ky, float kz)
        {
            Pivot.Scale(kx, ky, kz);
        }

        public void MoveTo(Vector3 p)
        {
            Pivot = new Pivot(p, Pivot.XAxis, Pivot.YAxis, Pivot.ZAxis);
        }

        public void RotateAt(Vector3 p, float angle, Axis axis)
        {
            Pivot.RotateAt(p, angle, axis);
        }

        public Vector3 ScreenProjection(Vector3 p)
        {
            float x = ScreenWidth / 2.0f * (1 + p.X);
            float y = ScreenHeight - ScreenHeight / 2.0f * (1 + p.Y);

            return new Vector3(x, y, p.Z);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RenderLib
{
    class Camera : Object3D
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public int ScreenNearDist { get; private set; }
        public int ScreenFarDist { get; private set; }

        public Camera(Pivot p, int width, int height, int near_dist, int far_dist)
        {
            Pivot = p;
            ScreenWidth = width;
            ScreenHeight = height;
            ScreenNearDist = near_dist;
            ScreenFarDist = far_dist;
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

        public Vector2 ScreenProjection(Vector3 p)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;

namespace RenderLib
{
    public class Camera : Object3D
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public float ScreenNearDist { get; private set; }
        public float ScreenFarDist { get; private set; }

        private static double fov = Math.PI / 2;
        private float r, t, tg_fov = (float)Math.Tan(fov / 2);

        public Camera(Pivot p, int width, int height, float near_dist, float far_dist)
        {
            Pivot = p;
            ScreenWidth = width;
            ScreenHeight = height;
            ScreenNearDist = near_dist;
            ScreenFarDist = far_dist;

            t = ScreenNearDist * tg_fov;
            r = t * ((float)ScreenWidth / ScreenHeight);
        }

        public Matrix4x4 PerspectiveClip => new Matrix4x4
        (
            ScreenNearDist / r, 0, 0, 0,
            0, ScreenNearDist / t, 0, 0,
            0, 0, (ScreenFarDist + ScreenNearDist) / (ScreenNearDist - ScreenFarDist), -1f,
            0, 0, 2f * ScreenNearDist * ScreenFarDist / (ScreenNearDist - ScreenFarDist), 0
        );

        public Matrix4x4 OrtogonalClip => new Matrix4x4
        (
            1f / r, 0, 0, 0,
            0, 1f / t, 0, 0,
            0, 0, -2.0f / (ScreenFarDist - ScreenNearDist), 0,
            0, 0, (ScreenFarDist + ScreenNearDist) / (ScreenNearDist - ScreenFarDist), 1f
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

        public bool IsVisible(Vector4 p)
        {
            float min = -1, max = 1;

            return min <= p.X && p.X <= max && min <= p.Y && p.Y <= max && min <= p.Z && p.Z <= max;
        }

        public bool IsVisible(Vertex v)
        {
            return IsVisible(v.Position);
        }

        public bool IsVisible(PolModel model, Polygon pol)
        {
            return IsVisible(model.GetPolVertex(pol, 0)) && IsVisible(model.GetPolVertex(pol, 1)) && IsVisible(model.GetPolVertex(pol, 2));
        }

        public bool IsVisible(List<Vertex> vertices, Polygon pol)
        {
            return IsVisible(vertices[pol[0]]) && IsVisible(vertices[pol[1]]) && IsVisible(vertices[pol[2]]);
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

        public void RotateTo(Vector3 xa, Vector3 ya, Vector3 za)
        {
            Pivot.XAxis = xa;
            Pivot.YAxis = ya;
            Pivot.ZAxis = za;
        }

        public void RotateAt(Vector3 p, float angle, Axis axis)
        {
            Pivot.RotateAt(p, angle, axis);
        }

        public Vector2 ToScreenProjection(Vector3 p)
        {
            float x = ScreenWidth / 2.0f * (1 + p.X);
            float y = ScreenHeight - ScreenHeight / 2.0f * (1 + p.Y);

            x = MathAddon.RoundToInt(x);
            y = MathAddon.RoundToInt(y);

            return new Vector2((int)x >= ScreenWidth ? ScreenWidth - 1 : x, (int)y >= ScreenHeight ? ScreenHeight - 1 : y);
        }

        public Vector3 FromScreenProjection(Vector3 p)
        {
            float x = p.X * 2f / ScreenWidth - 1;
            float y = (ScreenHeight - p.Y) * 2f / ScreenHeight - 1;

            return new Vector3(x, y, p.Z);
        }

        public Vector4 ToScreenProjection(Vector4 p)
        {
            float x = ScreenWidth / 2.0f * (1 + p.X / p.W);
            float y = ScreenHeight - ScreenHeight / 2.0f * (1 + p.Y / p.W);

            return new Vector4(MathAddon.RoundToInt(x), MathAddon.RoundToInt(y), p.Z / p.W, p.W);
        }

        public Vector4 FromScreenProjection(Vector4 p)
        {
            float x = p.X * 2f / ScreenWidth - 1;
            float y = (ScreenHeight - p.Y) * 2f / ScreenHeight - 1;

            return new Vector4(x * p.W, y * p.W, p.Z * p.W, p.W);
        }
    }
}

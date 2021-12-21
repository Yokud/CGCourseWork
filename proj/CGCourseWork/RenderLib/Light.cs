using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public abstract class Light : Object3D
    {
        protected static readonly float min_intensity = 0.2f;
        protected float intensity, diffuse_coef;
        public abstract float MaxIntensity { get; protected set; }
        public abstract float DiffuseCoef { get; protected set; }

        public abstract float GetAngleIntensity(Vector3 normal, Vector3 light_dir);
        public Color GetColorByIntensity(Color c, float intensity)
        {
            if (intensity < min_intensity)
                return Color.FromArgb(c.A, MathAddon.RoundToInt(c.R * min_intensity), MathAddon.RoundToInt(c.G * min_intensity), MathAddon.RoundToInt(c.B * min_intensity));
            else if (intensity > MaxIntensity)
                return c;
            else
                return Color.FromArgb(c.A, MathAddon.RoundToInt(c.R * intensity), MathAddon.RoundToInt(c.G * intensity), MathAddon.RoundToInt(c.B * intensity));
        }
    }

    public class DirectionalLight : Light, IProjectable
    {
        private static double fov = Math.PI / 2;
        private float r, t, tg_fov = (float)Math.Tan(fov / 2);

        public DirectionalLight(Pivot p, Vector3 l_dir, float max_intensity = 1f, float dif_coef = 1f, int width = 512, int height = 512)
        {
            Pivot = p;
            MaxIntensity = max_intensity;
            DiffuseCoef = dif_coef;
            LightDirection = l_dir;

            ScreenWidth = width;
            ScreenHeight = height;
            ScreenNearDist = 0.001f;
            ScreenFarDist = 1e10f;

            t = ScreenNearDist * tg_fov;
            r = t * ((float)width / height);
        }

        public override float MaxIntensity
        {
            get => intensity;
            protected set
            {
                if (value < 0 || value > 1)
                    throw new Exception("Недопустимое значение интенсивности света!\n");
                else
                    intensity = value;
            }
        }

        public override float DiffuseCoef
        {
            get => diffuse_coef;
            protected set
            {
                if (value < 0 || value > 1)
                    throw new Exception("Недопустимое значение коэффициента диффузного отражения!\n");
                else
                    diffuse_coef = value;
            }
        }

        public Vector3 LightDirection { get; private set; }

        public int ScreenWidth { get; private set; }

        public int ScreenHeight { get; private set; }

        public float ScreenNearDist { get; private set; }

        public float ScreenFarDist { get; private set; }

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

        public override float GetAngleIntensity(Vector3 normal, Vector3 light_dir)
        {
            float intensity = MaxIntensity * diffuse_coef * Vector3.Dot(normal, light_dir);

            return intensity < 0 ? 0 : intensity;
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

        public bool IsVisible(PolModel model, Polygon pol)
        {
            return IsVisible(model.GetPolVertex(pol, 0)) && IsVisible(model.GetPolVertex(pol, 1)) && IsVisible(model.GetPolVertex(pol, 2));
        }

        public override void Move(float dx, float dy, float dz)
        {
            Pivot.Move(dx, dy, dz);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
        }

        public void RotateAt(Vector3 p, float angle, Axis axis)
        {
            Pivot.RotateAt(p, angle, axis);
        }

        public override void Scale(float kx, float ky, float kz)
        {
            return;
        }

        public Vector3 ScreenProjection(Vector3 p)
        {
            float x = ScreenWidth / 2.0f * (1 + p.X);
            float y = ScreenHeight - ScreenHeight / 2.0f * (1 + p.Y);

            return new Vector3(x, y, p.Z);
        }
    }
}

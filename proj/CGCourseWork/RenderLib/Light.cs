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
        protected static readonly float min_intensity = 0.175f;
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

    public class DirectionalLight : Light
    {
        private static double fov = Math.PI / 2;
        private float r, t, tg_fov = (float)Math.Tan(fov / 2);

        public DirectionalLight(Pivot p, Vector3 l_dir, float dif_coef = 1f, int width = 512, int height = 512)
        {
            Pivot = p;
            MaxIntensity = 1f;
            DiffuseCoef = dif_coef;
            LightDirection = l_dir;

            ScreenWidth = width;
            ScreenHeight = height;
            ScreenNearDist = 0.05f;
            ScreenFarDist = 1e6f;

            t = ScreenNearDist * tg_fov;
            r = t * ((float)ScreenWidth / ScreenHeight);
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
            0, 0, (ScreenFarDist + ScreenNearDist) / (ScreenNearDist - ScreenFarDist), -1f,
            0, 0, 2 * ScreenNearDist * ScreenFarDist / (ScreenNearDist - ScreenFarDist), 0
        );

        public Matrix4x4 OrtogonalClip => new Matrix4x4
        (
            2f / ScreenWidth, 0, 0, 0,
            0, 2f / ScreenHeight, 0, 0,
            0, 0, -1f / (ScreenFarDist - ScreenNearDist), 0,
            0, 0, 0, 1f
        );

        public override float GetAngleIntensity(Vector3 normal, Vector3 light_dir)
        {
            float intensity = MaxIntensity * diffuse_coef * Vector3.Dot(normal, light_dir);

            return intensity < 0 ? 0 : intensity;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public abstract class Light : Object3D
    {
        protected float intensity;
        public abstract float Intensity { get; protected set; }
    }

    public class DirectionalLight : Light, IProjectable
    {
        public DirectionalLight(Pivot p, float intensity, Vector3 l_dir)
        {
            Pivot = p;
            Intensity = intensity;
            LightDirection = l_dir;
        }

        public override float Intensity
        {
            get => intensity;
            protected set
            {
                if (value < 0)
                    throw new Exception("Недопустимое значение интенсивности света!\n");
                else
                    intensity = value;
            }
        }

        public Vector3 LightDirection { get; private set; }

        public int ScreenWidth => throw new NotImplementedException();

        public int ScreenHeight => throw new NotImplementedException();

        public int ScreenNearDist => throw new NotImplementedException();

        public int ScreenFarDist => throw new NotImplementedException();

        public Matrix4x4 PerspectiveClip => throw new NotImplementedException();

        public Matrix4x4 OrtogonalClip => throw new NotImplementedException();

        public bool IsVisible(Vector3 p)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(Vertex v)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(PolModel model, Polygon pol)
        {
            throw new NotImplementedException();
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
            return;
        }

        public Vector3 ScreenProjection(Vector3 p)
        {
            throw new NotImplementedException();
        }
    }
}

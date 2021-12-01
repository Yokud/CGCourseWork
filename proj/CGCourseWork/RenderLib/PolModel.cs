using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RenderLib
{
    class PolModel : Object3D
    {
        public List<Vertex> Vertices { get; protected set; }
        public List<Polygon> Polygons { get; protected set; }
        public List<Vector3> Normals { get; protected set; }
        public Texture Texture { get; protected set; }

        public PolModel(List<Vertex> verts, List<Polygon> pols, Texture texture, Pivot p)
        {
            Vertices = verts;
            Polygons = pols;
            Texture = texture;
            Pivot = p;

            CalcNormals();
        }

        public override void Move(float dx, float dy, float dz)
        {
            Pivot.Move(dx, dy, dz);

            foreach (var v in Vertices)
                v.Move(dx, dy, dz);

            OnMoveEvent(dx, dy, dz);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);

            foreach (var v in Vertices)
                v.Rotate(angle, axis);

            OnRotateEvent(angle, axis);
        }

        public override void Scale(float kx, float ky, float kz)
        {
            Pivot.Scale(kx, ky, kz);

            foreach (var v in Vertices)
                v.Scale(kx, ky, kz);

            OnScaleEvent(kx, ky, kz);
        }

        protected void CalcNormals()
        {
            foreach (var pol in Polygons)
            {
                Vector3 a = Vertices[pol.Vertices[2]].Position - Vertices[pol.Vertices[0]].Position;
                Vector3 b = Vertices[pol.Vertices[1]].Position - Vertices[pol.Vertices[0]].Position;

                Normals.Add(Vector3.Cross(a, b));
            }
        }
    }
}

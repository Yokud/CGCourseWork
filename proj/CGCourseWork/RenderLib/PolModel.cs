using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    class PolModel : Object3D
    {
        public List<Vertex> Vertices { get; protected set; }
        public List<Polygon> Polygons { get; protected set; }
        public Texture Texture { get; protected set; }

        public PolModel(List<Vertex> verts, List<Polygon> pols, Texture texture, Pivot p)
        {
            Vertices = verts;
            Polygons = pols;
            Texture = texture;
            Pivot = p;
        }

        public override void Move(float dx, float dy, float dz)
        {
            Pivot.Move(dx, dy, dz);

            foreach (var v in Vertices)
                v.Move(dx, dy, dz);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);

            foreach (var v in Vertices)
                v.Rotate(angle, axis);
        }

        public override void Scale(float kx, float ky, float kz)
        {
            Pivot.Scale(kx, ky, kz);

            foreach (var v in Vertices)
                v.Scale(kx, ky, kz);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RenderLib
{
    public class PolModel : Object3D, ICloneable
    {
        public List<Vertex> Vertices { get; protected set; }
        public List<Polygon> Polygons { get; protected set; }
        public List<Vector3> Normals { get; protected set; }
        public Texture Texture { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts">Координаты вершин в пространстве модели</param>
        /// <param name="pols"></param>
        /// <param name="texture"></param>
        /// <param name="p"></param>
        public PolModel(List<Vertex> verts, List<Polygon> pols, Texture texture, Pivot p)
        {
            Vertices = verts;
            Polygons = pols;
            Texture = texture;
            Pivot = p;

            Normals = new List<Vector3>();

            CalcNormals();
            CalcAdjPols();
        }

        public override void Move(float dx, float dy, float dz)
        {
            Pivot.Move(dx, dy, dz);

            OnMoveEvent(dx, dy, dz);
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);

            OnRotateEvent(angle, axis);
        }

        public override void Scale(float kx, float ky, float kz)
        {
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

                Normals.Add(Vector3.Normalize(Vector3.Cross(a, b)));
            }
        }

        protected void CalcAdjPols()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].AdjacentPolygons.Clear();
                for (int j = 0; j < Polygons.Count; j++)
                    if (Polygons[j].Vertices[0] == i || Polygons[j].Vertices[1] == i || Polygons[j].Vertices[2] == i)
                        Vertices[i].AdjacentPolygons.Add(j);
            }
        }

        public object Clone()
        {
            return new PolModel(Vertices, Polygons, Texture, Pivot);
        }

        public Matrix4x4 ToModelMatrix => Pivot.LocalCoordsMatrix;

        public Matrix4x4 ToWorldMatrix => Pivot.GlobalCoordsMatrix;
    }
}

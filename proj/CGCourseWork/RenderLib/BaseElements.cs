using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RenderLib
{
    public enum Axis { X, Y, Z}

    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector2 TextureCoords { get; set; }
        public Vector3 Normal { get; set; }
        public List<int> AdjacentPolygons { get; set; }

        public Vertex(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public Vertex(Vector3 pos)
        {
            Position = pos;
        }

        public Vertex(Vector3 pos, Vector2 texcoords, Vector3 norm)
        {
            Position = pos;
            TextureCoords = texcoords;
            Normal = norm;
        }

        public void Move(float dx, float dy, float dz)
        {
            Position += new Vector3(dx, dy, dz);
        }

        public void Scale(float kx, float ky, float kz)
        {
            var v = new Vector4(Position.X, Position.Y, Position.Z, 1);
            var matr = new Matrix4x4() { M11 = kx, M22 = ky, M33 = kz, M44 = 1 };

            var new_v = Vector4.Transform(v, matr);

            Position = new Vector3(new_v.X / new_v.W, new_v.Y / new_v.W, new_v.Z / new_v.W);
        }

        public void Rotate(float angle, Axis axis)
        {
            var rotation = axis == Axis.X ? Matrix4x4.CreateRotationX(angle) : axis == Axis.Y ? Matrix4x4.CreateRotationY(angle) : Matrix4x4.CreateRotationZ(angle);
            Position = Vector3.Transform(Position, rotation);
        }
    }

    public class Polygon
    {
        public int[] Vertices { get; set; }
        public Vector3 Normal { get; private set; }

        public Polygon(int v1, int v2, int v3, List<Vertex> verts = null)
        {
            Vertices = new int[3] { v1, v2, v3 };

            if (verts != null)
            {
                CalcNormal(verts);
            }
        }

        public Vector3 CalcNormal(List<Vertex> verts)
        {
            Vector3 a = verts[Vertices[2]].Position - verts[Vertices[0]].Position;
            Vector3 b = verts[Vertices[1]].Position - verts[Vertices[0]].Position;

            Normal = Vector3.Normalize(Vector3.Cross(a, b));

            return Normal;
        }
    }

    public class Pivot
    {
        /// <summary>
        /// Pivot position in world coord system
        /// </summary>
        public Vector3 Center { get; private set; }
        /// <summary>
        /// must be length equal one
        /// </summary>
        public Vector3 XAxis { get; set; }
        public Vector3 YAxis { get; set; }
        public Vector3 ZAxis { get; set; }
        /// <summary>
        /// Proections to local ort 
        /// Orts must be length equal one
        /// </summary>
        public Matrix4x4 LocalCoordsMatrix => new Matrix4x4
            (
                XAxis.X, YAxis.X, ZAxis.X, 0,
                XAxis.Y, YAxis.Y, ZAxis.Y, 0,
                XAxis.Z, YAxis.Z, ZAxis.Z, 0,
                0, 0, 0, 1
            );
        /// <summary>
        /// Invert tranform to world coords system
        /// </summary>
        public Matrix4x4 GlobalCoordsMatrix => new Matrix4x4
            (
                XAxis.X, XAxis.Y, XAxis.Z, 0,
                YAxis.X, YAxis.Y, YAxis.Z, 0,
                ZAxis.X, ZAxis.Y, ZAxis.Z, 0,
                0, 0, 0, 1
            );
        public Pivot(Vector3 center, Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            Center = center;
            XAxis = xaxis;
            YAxis = yaxis;
            ZAxis = zaxis;
        }
        public static Pivot BasePivot(Vector3 center) => new Pivot(center, new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        public void Move(Vector3 v)
        {
            Center += v;
        }

        public void Rotate(float angle, Axis axis)
        {
            XAxis = XAxis.Rotate(angle, axis);
            YAxis = YAxis.Rotate(angle, axis);
            ZAxis = ZAxis.Rotate(angle, axis);
        }
        public void RotateAt(Vector3 point, float angle, Axis axis)
        {
            //creating basis with center in point
            var rotationBasis = Pivot.BasePivot(point);
            //transforming to basis coords 4 points : , Center , Center + XAxis , Center + YAxis , Center + ZAxis
            var center = Center - point;
            var xaxis = center + XAxis;
            var yaxis = center + YAxis;
            var zaxis = center + ZAxis;
            //rotating this points in local basis and tranforming to global
            var newCenter = rotationBasis.ToGlobalCoords(center.Rotate(angle, axis));
            var newx = rotationBasis.ToGlobalCoords(xaxis.Rotate(angle, axis));
            var newy = rotationBasis.ToGlobalCoords(yaxis.Rotate(angle, axis));
            var newz = rotationBasis.ToGlobalCoords(zaxis.Rotate(angle, axis));
            //creating new basis from this points
            Center = newCenter;
            XAxis = newx - Center;
            YAxis = newy - Center;
            ZAxis = newz - Center;
        }
        public Vector3 ToGlobalCoords(Vector3 local)
        {
            return Vector3.Transform(local, GlobalCoordsMatrix) + Center;
        }
        public Vector3 ToLocalCoords(Vector3 global)
        {
            return Vector3.Transform(global - Center, LocalCoordsMatrix);
        }
    }

    public abstract class Object3D
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace RenderLib
{
    public enum Axis { X, Y, Z }

    static class MathAddon
    {
        /// <summary>
        /// Угол между векторами
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float Angle(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length() * v2.Length()));
        }

        /// <summary>
        /// Поворот вектора
        /// </summary>
        /// <param name="v"></param>
        /// <param name="angle">Угол поворота</param>
        /// <param name="axis">Ось поворота</param>
        public static Vector3 Rotate(this Vector3 v, float angle, Axis axis)
        {
            Matrix4x4 rotation = axis == Axis.X ? Matrix4x4.CreateRotationX(angle) : axis == Axis.Y ? Matrix4x4.CreateRotationY(angle) : Matrix4x4.CreateRotationZ(angle);
            return v.Transform(rotation);
        }

        /// <summary>
        /// Проекция первого вектора на второй
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 Proection(Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v1, v2) / Vector3.Dot(v2, v2) * v2;
        }

        /// <summary>
        /// Перемещение
        /// </summary>
        /// <param name="v"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        public static Vector3 Move(this Vector3 v, float dx, float dy, float dz)
        {
            return v + new Vector3(dx, dy, dz);
        }

        /// <summary>
        /// Преобразование по матрице
        /// </summary>
        /// <param name="v"></param>
        /// <param name="matrix4X4">Матрица преобразования</param>
        public static Vector3 Transform(this Vector3 v, Matrix4x4 matrix4X4)
        {
            Vector4 new_v = Vector4.Transform(v.ToVec4(), matrix4X4);
            return new_v.FromVec4();
        }

        public static Vector3 Transform(this Vector3 v, Matrix4x4 matrix4X4, out float w)
        {
            Vector4 new_v = Vector4.Transform(v.ToVec4(), matrix4X4);
            w = new_v.W;
            return new_v.FromVec4();
        }

        /// <summary>
        /// Перевод в однородную систему координат
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Vector4 ToVec4(this Vector3 v, float w = 1f)
        {
            return new Vector4(v.X, v.Y, v.Z, w);
        }

        /// <summary>
        /// Перевод из однородной системы координат
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 FromVec4(this Vector4 v)
        {
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }

        public static int Lepr(int min, int max, float amount)
        {
            return MathAddon.RoundToInt(min + (max - min) * amount);
        }

        public static float Lepr(float min, float max, float amount)
        {
            return min + (max - min) * amount;
        }

        public static int RoundToInt(double d)
        {
            return (int)Math.Round(d, MidpointRounding.AwayFromZero);
        }

        public static bool IsEqual(double a, double b)
        {
            double eps = 1e-6;

            return Math.Abs(a - b) < eps;
        }

        public static int Max3(float a, float b, float c)
        {
            float m = a;

            if (m < b)
                m = b;

            if (m < c)
                m = c;

            return (int)m;
        }

        public static int Min3(float a, float b, float c)
        {
            float m = a;

            if (m > b)
                m = b;

            if (m > c)
                m = c;

            return (int)m;
        }
    }
}
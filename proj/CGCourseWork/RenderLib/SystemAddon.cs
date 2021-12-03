using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    static class SystemAddon
    {
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
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

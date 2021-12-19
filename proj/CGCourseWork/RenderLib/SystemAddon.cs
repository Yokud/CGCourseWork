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

        public static List<T> Clone<T>(this List<T> list)
        {
            List<T> temp = new List<T>(list);
            return temp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    class PolModel : Object3D
    {
        public List<Vertex> Vertises { get; protected set; }
        public List<Polygon> Polygons { get; protected set; }

        public Texture Texture { get; protected set; }
    }
}

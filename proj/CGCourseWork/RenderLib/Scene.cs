using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public class Scene
    {
        public Camera Camera { get; private set; }
        public Terrain Terrain { get; private set; }
        public DirectionalLight LightSource { get; private set; }

        public Scene(Terrain terr, Camera cam, DirectionalLight light)
        {
            Terrain = terr;
            Camera = cam;
            LightSource = light;
        }

        public List<Object3D> Objects => new List<Object3D>() { Terrain.VisibleTerrainModel, Camera, LightSource };
    }
}

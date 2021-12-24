using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public class Facade
    {
        Scene Scene { get; set; }
        Drawer Drawer { get; set; }

        public Facade(Scene scene, Drawer drawer)
        {
            Scene = scene;
            Drawer = drawer;
        }

        public FastBitmap DrawScene()
        {
            Drawer.DrawScene(Scene);
            return Drawer.FrameBuffer;
        }

        public void RotateTerrain(float angle, Axis axis)
        {
            Scene.RotateTerrain(angle, axis);
        }

        public void ScaleTerrain(float kx, float ky, float kz)
        {
            Scene.ScaleTerrain(kx, ky, kz);
        }

        public void MoveTerrain(int dx, int dz)
        {
            Scene.MoveTerrainLimits(dx, dz);
        }
    }
}

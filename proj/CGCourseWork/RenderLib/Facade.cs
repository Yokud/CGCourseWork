using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public class Facade
    {
        public Scene Scene { get; private set; }
        public Drawer Drawer { get; private set; }

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
    }
}

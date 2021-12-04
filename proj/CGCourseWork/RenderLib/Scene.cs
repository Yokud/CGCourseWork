﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderLib
{
    public class Scene
    {
        public Camera Camera { get; private set; }
        public PolModel Model { get; private set; }

        public Scene(PolModel model, Camera cam)
        {
            Model = model;
            Camera = cam;
        }

        public List<Object3D> Objects => new List<Object3D>() { Model, Camera };
    }
}
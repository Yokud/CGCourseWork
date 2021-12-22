using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeightMapLib;

namespace RenderLib
{
    public class Terrain
    {
        HeightMap map;
        int t_vis_width, t_vis_height;
        int t_vis_index_x, t_vis_index_y;
        TerrainVisibleSection terrain_model;
        float rot_coef_x, rot_coef_y, rot_coef_z;
        float scale_coef_x, scale_coef_y, scale_coef_z;


        public Terrain(HeightMap map, int width, int height)
        {
            this.map = map;
            t_vis_width = width;
            t_vis_height = height;

            t_vis_index_x = t_vis_index_y = 0;

            rot_coef_x = rot_coef_y = rot_coef_z = 0;
            scale_coef_x = scale_coef_y = scale_coef_z = 0;

            terrain_model = new TerrainVisibleSection();
        }

        void GetModelFromMap()
        {
            List<Vertex> verts = new List<Vertex>();
        }
    }

    class TerrainVisibleSection : PolModel
    {
        public TerrainVisibleSection(List<Vertex> verts, List<Polygon> pols, Texture texture, Pivot p) : base(verts, pols, texture, p)
        {

        }
    }
}

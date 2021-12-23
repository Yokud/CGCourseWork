using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
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


        public Terrain(int width, int height, int vis_width, int vis_height) : this(new HeightMap(width, height, new PerlinNoise(12)), vis_width, vis_height)
        {

        }

        public Terrain(HeightMap map, int vis_width, int vis_height)
        {
            this.map = map;
            t_vis_width = vis_width;
            t_vis_height = vis_height;

            t_vis_index_x = t_vis_index_y = 0;

            rot_coef_x = rot_coef_y = rot_coef_z = 0;
            scale_coef_x = scale_coef_y = scale_coef_z = 0;

            map.SaveToBmp(@"D:\Repos\CGCourseWork\proj\CGCourseWork\RenderLib\heightmaps\", "test");
            map.Normalize();

            terrain_model = new TerrainVisibleSection(vis_width, vis_height, map.NoiseMap);
        }

        internal TerrainVisibleSection VisibleTerrainModel => terrain_model;
    }

    internal class TerrainVisibleSection : PolModel
    {
        MipMap[] land_textures;

        public TerrainVisibleSection(int width, int height, float[,] height_map)
        {
            float topLeftX = (width - 1) / 2f;
            float topLeftZ = (height - 1) / 2f;
            int vert_index = 0;

            float height_coef = 3f * (float)Math.Sqrt(2 * width * height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Vertices.Add(new Vertex((x - topLeftX) * 10f, height_map[x, y] * height_coef, (y - topLeftZ) * 10f, (float)x / width, (float)y / height));

                    if (x < width - 1 && y < height - 1)
                    {
                        Polygons.Add(new Polygon(vert_index, vert_index + width + 1, vert_index + width));
                        Polygons.Add(new Polygon(vert_index + width + 1, vert_index, vert_index + 1));
                    }

                    vert_index++;
                }

            RecalcNormals();
            RecalcAdjPols();
            CorrectNormals();
        }

        private void CorrectNormals()
        {
            Vector3 mult = new Vector3(0, -1, 0);

            for (int i = 0; i < Normals.Count; i++)
                if (Normals[i].Y < 0)
                    Normals[i] *= mult;

            RecalcVertexNormals();
        }
    }
}

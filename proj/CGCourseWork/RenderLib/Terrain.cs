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
        int t_vis_index_x, t_vis_index_z;
        List<Texture> land_textures;
        TerrainVisibleSection terrain_model;

        Vector3 scale_coefs;


        public Terrain(int width, int height, int vis_width, int vis_height, List<Texture> textures) : this(new HeightMap(width, height, 
                                                                                                        new PerlinNoise(Math.Max(vis_width, vis_height) / 3, 3, seed:12345)), 
                                                                                                        vis_width, vis_height,
                                                                                                        textures)
        { }

        public Terrain(HeightMap map, int vis_width, int vis_height, List<Texture> textures)
        {
            this.map = map;

            t_vis_width = vis_width;
            t_vis_height = vis_height;

            t_vis_index_x = t_vis_index_z = 0;

            scale_coefs = new Vector3(1, 1, 1);

            map.Normalize();

            land_textures = textures;
            terrain_model = new TerrainVisibleSection(vis_width, vis_height, map.NoiseMap, textures);
        }

        internal TerrainVisibleSection VisibleTerrainModel => terrain_model;


        public void Rotate(float angle, Axis axis)
        {
            terrain_model.Rotate(angle, axis);
        }

        public void Scale(float kx, float ky, float kz)
        {
            terrain_model.Scale(kx, ky, kz);

            scale_coefs *= new Vector3(kx, ky, kz);
        }

        public void Move(int dx, int dz)
        {
            if (t_vis_index_x + dx > -1 && t_vis_index_x + dx + t_vis_width < map.Width && t_vis_index_z + dz > -1 && t_vis_index_z + dz + t_vis_height < map.Height)
            {
                t_vis_index_x += dx;
                t_vis_index_z += dz;
                terrain_model.TerrainUpdate(map.NoiseMap, t_vis_index_x, t_vis_index_z, scale_coefs);
            }
        }
    }

    internal class TerrainVisibleSection : PolModel
    {
        List<Texture> land_textures;
        List<TextureType> pols_texture_types;
        float height_coef;
        int width, height;

        enum TextureType { WATER, SAND, GRASS, ROCK, SNOW }

        public TerrainVisibleSection(int width, int height, float[,] height_map, List<Texture> textures)
        {
            this.width = width;
            this.height = height;

            float topLeftX = (width - 1) / 2f;
            float topLeftZ = (height - 1) / 2f;
            int vert_index = 0;

            height_coef = (float)Math.Sqrt(height_map.GetLength(0) * height_map.GetLength(1));

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Vertices.Add(new Vertex((x - topLeftX) * 10f, height_map[x, y] * height_coef, (y - topLeftZ) * 10f, (float)x / width * 3f, (float)y / height * 3f));

                    if (x < width - 1 && y < height - 1)
                    {
                        Polygons.Add(new Polygon(vert_index, vert_index + width + 1, vert_index + width));
                        Polygons.Add(new Polygon(vert_index + width + 1, vert_index, vert_index + 1));
                    }

                    vert_index++;
                }

            land_textures = textures;
            pols_texture_types = new List<TextureType>();

            RecalcNormals();
            RecalcAdjPols();
            CorrectNormals();
            SetPolsTextures();
        }

        public void TerrainUpdate(float[,] height_map, int start_x, int start_y, Vector3 scale_coefs)
        {
            Vertices.Clear();
            pols_texture_types.Clear();

            float topLeftX = (width - 1) / 2f;
            float topLeftZ = (height - 1) / 2f;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Vertices.Add(new Vertex((x - topLeftX) * 10f, height_map[x + start_x, y + start_y] * height_coef, (y - topLeftZ) * 10f, (float)x / width * 3f, (float)y / height * 3f));


            if (!(MathAddon.IsEqual(scale_coefs.X, 1) && MathAddon.IsEqual(scale_coefs.Y, 1) && MathAddon.IsEqual(scale_coefs.Z, 1)))
                Scale(scale_coefs.X, scale_coefs.Y, scale_coefs.Z);

            RecalcAdjPols();
            RecalcNormals();
            CorrectNormals();
            SetPolsTextures();
        }

        private void CorrectNormals()
        {
            Vector3 mult = new Vector3(0, -1, 0);

            for (int i = 0; i < Normals.Count; i++)
                if (Normals[i].Y < 0)
                    Normals[i] *= mult;

            RecalcVertexNormals();
        }

        private void SetPolsTextures()
        {
            foreach (var pol in Polygons)
            {
                float avg_height = (Vertices[pol[0]].Position.Y + Vertices[pol[1]].Position.Y + Vertices[pol[2]].Position.Y) / 3f;

                if (avg_height < 0.2 * height_coef)
                    pols_texture_types.Add(TextureType.WATER);
                else if (avg_height < 0.275 * height_coef)
                    pols_texture_types.Add(TextureType.SAND);
                else if (avg_height < 0.55 * height_coef)
                    pols_texture_types.Add(TextureType.GRASS);
                else if (avg_height < 0.875 * height_coef)
                    pols_texture_types.Add(TextureType.ROCK);
                else
                    pols_texture_types.Add(TextureType.SNOW);
            }
        }

        public Texture GetTexture(Polygon pol)
        {
            int i = Polygons.IndexOf(pol);

            return land_textures[(int)pols_texture_types[i]];
        }
    }
}

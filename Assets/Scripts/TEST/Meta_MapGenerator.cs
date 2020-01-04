using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta_MapGenerator
{
    public enum ETerrain
    {
        //EBasic,
        EFlat_1,
        EFlat_2,
        EMountains,
        ERoughRocks,
        EGulch,
        EValley
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class CHUNK
    {
        public static Vector3Int Size = new Vector3Int(30, 50, 30);
        public static float heightVariance = 1f;
        public static float smoothnessVariance = 0.5f;
        public static float colorVariance = 0.3f;
    }

    public class TERRAIN
    {
        private float smoothness = 1f; //higher value meens smoother
        private Color color;
        private float height;

        public TERRAIN(float _baseSmoothness, Color _baseColor, float _baseHeight)
        {
            smoothness = _baseSmoothness + StaticMaths.GetRandomFloat(-CHUNK.smoothnessVariance, CHUNK.smoothnessVariance);

            color = new Color(
                _baseColor.r + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.g + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.b + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance)
                );

            height = _baseHeight + StaticMaths.GetRandomFloat(-CHUNK.heightVariance, CHUNK.heightVariance);
        }

        public static TERRAIN GetTerrainData(ETerrain _eTerrain)
        {
            switch (_eTerrain)
            {
                //case EBiome.EBasic:
                //    return new BIOME(5, Color.gray);
                case ETerrain.EFlat_1:
                    return new TERRAIN(8, Color.green, 1f);
                case ETerrain.EFlat_2:
                    return new TERRAIN(10, Color.yellow, 1f);
                case ETerrain.ERoughRocks:
                    return new TERRAIN(1, Color.red, 10f); //0.9
                case ETerrain.EMountains:
                    return new TERRAIN(7, Color.blue, 30f);
                case ETerrain.EGulch:
                    return new TERRAIN(7, Color.grey, -10f);
                case ETerrain.EValley:
                    return new TERRAIN(3, Color.grey, -7f);
            }

            return new TERRAIN(5, Color.gray, 0f);
        }

        public float GetSmoothness()
        {
            return smoothness;
        }

        public Color GetColor()
        {
            return color;
        }

        public float GetHeight()
        {
            return height;
        }

        public static ETerrain GetRandomTerrain()
        {
            System.Array values = System.Enum.GetValues(typeof(ETerrain));
            return (ETerrain)Random.Range(0, values.Length);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta_MapGenerator
{
    public enum EBiome
    {
        //EBasic,
        EGrassland,
        EDesert,
        EMountains,
        EGulch
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class CHUNK
    {
        public static Vector3Int Size = new Vector3Int(20, 20, 20);
        public static float heightVariance = 1f;
        public static float smoothnessVariance = 0.5f;
        public static float colorVariance = 0.3f;
    }

    public class BIOME
    {
        private float smoothness = 1f; //higher value meens smoother
        private Color color;
        private float height;

        public BIOME(float _baseSmoothness, Color _baseColor, float _baseHeight)
        {
            smoothness = _baseSmoothness + StaticMaths.GetRandomFloat(-CHUNK.smoothnessVariance, CHUNK.smoothnessVariance);

            color = new Color(
                _baseColor.r + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.g + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.b + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance)
                );

            height = _baseHeight + StaticMaths.GetRandomFloat(-CHUNK.heightVariance, CHUNK.heightVariance);
        }

        public static BIOME GetBiomeData(EBiome _eBiome)
        {
            switch (_eBiome)
            {
                //case EBiome.EBasic:
                //    return new BIOME(5, Color.gray);
                case EBiome.EGrassland:
                    return new BIOME(8, Color.green, 1f);
                case EBiome.EDesert:
                    return new BIOME(10, Color.yellow, 1f);
                case EBiome.EMountains:
                    return new BIOME(1, Color.red, 10f); //0.9
                case EBiome.EGulch:
                    return new BIOME(2, Color.grey, -10f);
            }

            return new BIOME(5, Color.gray, 0f);
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

        public static EBiome GetRandomBiome()
        {
            System.Array values = System.Enum.GetValues(typeof(EBiome));
            return (EBiome)Random.Range(0, 4);
        }
    }
}

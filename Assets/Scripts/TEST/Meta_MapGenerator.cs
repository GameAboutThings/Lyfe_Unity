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
        EMountains
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class CHUNK
    {
        public static Vector3Int Size = new Vector3Int(50, 50, 50);
    }

    public class BIOME
    {
        private float smoothness = 1f; //higher value meens smoother
        private Color color;

        public BIOME(float _smoothness, Color _baseColor)
        {
            smoothness = _smoothness;
            color = _baseColor;
        }

        public static BIOME GetBiomeData(EBiome _eBiome)
        {
            switch (_eBiome)
            {
                //case EBiome.EBasic:
                //    return new BIOME(5, Color.gray);
                case EBiome.EGrassland:
                    return new BIOME(10, Color.green);
                case EBiome.EDesert:
                    return new BIOME(10, Color.yellow);
                case EBiome.EMountains:
                    return new BIOME(2, Color.red);
            }

            return new BIOME(5, Color.gray);
        }

        public float GetSmoothness()
        {
            return smoothness;
        }

        public Color GetColor()
        {
            return color;
        }

        public static EBiome GetRandomBiome()
        {
            System.Array values = System.Enum.GetValues(typeof(EBiome));
            return (EBiome)Random.Range(0, values.Length);
        }
    }
}

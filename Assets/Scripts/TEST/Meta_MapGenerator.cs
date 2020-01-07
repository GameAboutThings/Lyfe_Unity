using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta_MapGenerator
{
    public enum ETerrain
    {
        //every terrain type added here has to be added to the GetTerrainMap method, too
        EFlat_1,
        EFlat_2,
        EMountains,
        EExtremeMountain,
        ERoughRocks,
        EGulch,
        EValley,
        EHighPlateau,
        ELowPlatou
    }

    class MISC
    {
        public static int seedLength = 18;
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class CHUNK
    {
        public static Vector3Int Size = new Vector3Int(30, 70, 30);
        public static float heightVariance = 1f;
        public static float smoothnessVariance = 0.5f;
        public static float colorVariance = 0.3f;
    }

    public class TERRAIN
    {
        private float smoothness = 1f; //higher value meens smoother
        private Color color;
        private float height;
        private int frequency;

        public TERRAIN(int _frequency, float _baseSmoothness, Color _baseColor, float _baseHeight)
        {
            frequency = _frequency;

            smoothness = _baseSmoothness + StaticMaths.GetRandomFloat(-CHUNK.smoothnessVariance, CHUNK.smoothnessVariance);

            color = new Color(
                _baseColor.r + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.g + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance),
                _baseColor.b + StaticMaths.GetRandomFloat(-CHUNK.colorVariance, CHUNK.colorVariance)
                );

            height = _baseHeight + StaticMaths.GetRandomFloat(-CHUNK.heightVariance, CHUNK.heightVariance);
        }

        public static Dictionary<ETerrain, TERRAIN> GetTerrainMap()
        {
            Dictionary<ETerrain, TERRAIN> terrainMap = new Dictionary<ETerrain, TERRAIN>();

            terrainMap.Add(ETerrain.EFlat_1, new TERRAIN(3, 8, Color.green, 1f));
            terrainMap.Add(ETerrain.EFlat_2, new TERRAIN(2, 10, Color.yellow, 1f));
            terrainMap.Add(ETerrain.ERoughRocks, new TERRAIN(1, 1, Color.red, 10f));
            terrainMap.Add(ETerrain.EMountains, new TERRAIN(1, 7, Color.blue, 30f));
            terrainMap.Add(ETerrain.EExtremeMountain, new TERRAIN(1, 5, Color.blue, 50f));
            terrainMap.Add(ETerrain.EGulch, new TERRAIN(1, 7, Color.grey, -10f));
            terrainMap.Add(ETerrain.EValley, new TERRAIN(1, 3, Color.grey, -7f));
            terrainMap.Add(ETerrain.EHighPlateau, new TERRAIN(1, 8, Color.grey, 10f));
            terrainMap.Add(ETerrain.ELowPlatou, new TERRAIN(1, 8, Color.grey, -2f));

            return terrainMap;
        }

        public static TERRAIN GetTerrainData(ETerrain _eTerrain)
        {
            return GetTerrainMap()[_eTerrain];
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

        public int GetFrequency()
        {
            return frequency;
        }

        public static ETerrain GetRandomTerrain()
        {
            System.Array values = System.Enum.GetValues(typeof(ETerrain));
            return (ETerrain)Random.Range(0, values.Length);
        }

        public static ETerrain GetRandomTerrainByFrequency()
        {
            List<ETerrain> terrainSet = new List<ETerrain>();

            Dictionary<ETerrain, TERRAIN> terrainMap = GetTerrainMap();

            System.Array values = System.Enum.GetValues(typeof(ETerrain));
            for (int i = 0; i < values.Length; i++)
            {
                int frequency = terrainMap[(ETerrain)values.GetValue(i)].GetFrequency();

                for (int j = 0; j < frequency; j++)
                {
                    terrainSet.Add((ETerrain)values.GetValue(i));
                }
            }

            return terrainSet[Random.Range(0, terrainSet.Count)];
        }
    }
}

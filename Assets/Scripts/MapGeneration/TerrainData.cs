using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class TerrainData
{
    private float smoothness = 1f; //higher value meens smoother
    private Color color;
    private float height;
    private int frequency;

    public TerrainData(int _frequency, float _baseSmoothness, Color _baseColor, float _baseHeight)
    {
        frequency = _frequency;

        smoothness = _baseSmoothness + StaticMaths.GetRandomFloat(-TERRAIN.smoothnessVariance, TERRAIN.smoothnessVariance);

        color = new Color(
            _baseColor.r + StaticMaths.GetRandomFloat(-TERRAIN.colorVariance, TERRAIN.colorVariance),
            _baseColor.g + StaticMaths.GetRandomFloat(-TERRAIN.colorVariance, TERRAIN.colorVariance),
            _baseColor.b + StaticMaths.GetRandomFloat(-TERRAIN.colorVariance, TERRAIN.colorVariance)
            );

        height = _baseHeight + StaticMaths.GetRandomFloat(-TERRAIN.heightVariance, TERRAIN.heightVariance);
    }

    public static TERRAIN.ETerrain GetRandomTerrain()
    {
        System.Array values = System.Enum.GetValues(typeof(TERRAIN.ETerrain));
        return (TERRAIN.ETerrain)Random.Range(0, values.Length);
    }

    public static TerrainData GetTerrainData(TERRAIN.ETerrain _eTerrain)
    {
        return GetTerrainMap()[_eTerrain];
    }

    public static Dictionary<TERRAIN.ETerrain, TerrainData> GetTerrainMap()
    {
        Dictionary<TERRAIN.ETerrain, TerrainData> terrainMap = new Dictionary<TERRAIN.ETerrain, TerrainData>();

        terrainMap.Add(TERRAIN.ETerrain.EFlat_1, new TerrainData(3, 8, Color.green, 1f));
        terrainMap.Add(TERRAIN.ETerrain.EFlat_2, new TerrainData(2, 10, Color.yellow, 1f));
        terrainMap.Add(TERRAIN.ETerrain.ERoughRocks, new TerrainData(1, 1, Color.red, 10f));
        terrainMap.Add(TERRAIN.ETerrain.EMountains, new TerrainData(1, 7, Color.blue, 30f));
        terrainMap.Add(TERRAIN.ETerrain.EExtremeMountain, new TerrainData(1, 5, Color.blue, 50f));
        terrainMap.Add(TERRAIN.ETerrain.EGulch, new TerrainData(1, 7, Color.grey, -10f));
        terrainMap.Add(TERRAIN.ETerrain.EValley, new TerrainData(1, 3, Color.grey, -7f));
        terrainMap.Add(TERRAIN.ETerrain.EHighPlateau, new TerrainData(1, 8, Color.grey, 10f));
        terrainMap.Add(TERRAIN.ETerrain.ELowPlatou, new TerrainData(1, 8, Color.grey, -2f));

        return terrainMap;
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

    public static TERRAIN.ETerrain GetRandomTerrainByFrequency()
    {
        List<TERRAIN.ETerrain> terrainSet = new List<TERRAIN.ETerrain>();

        Dictionary<TERRAIN.ETerrain, TerrainData> terrainMap = GetTerrainMap();

        System.Array values = System.Enum.GetValues(typeof(TERRAIN.ETerrain));
        for (int i = 0; i < values.Length; i++)
        {
            int frequency = terrainMap[(TERRAIN.ETerrain)values.GetValue(i)].GetFrequency();

            for (int j = 0; j < frequency; j++)
            {
                terrainSet.Add((TERRAIN.ETerrain)values.GetValue(i));
            }
        }

        return terrainSet[Random.Range(0, terrainSet.Count)];
    }
}

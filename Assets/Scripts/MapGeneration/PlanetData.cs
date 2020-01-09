using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class PlanetData
{
    private int seaLevel = 0; //is multiplied with the discrete heights in the terrain
    private Color belowZero = new Color(204f / 255f, 102f / 255f, 0f);
    private Color zero = new Color(51f / 255f, 102f / 255f, 0f); //seaLevel
    private Color levelOne = new Color(170f / 255f, 170f / 255f, 170f / 255f);
    private Color levelTwo = new Color(0.9f, 0.9f, 0.9f);
    private Vector2 terrainSize = TERRAIN.baseSize;

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++ CONSTRUCTORS ++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public PlanetData()
    {

    }

    public PlanetData(int _seaLevel)
    {
        seaLevel = _seaLevel;
    }

    public PlanetData(Vector2 _terrainSize)
    {
        terrainSize = _terrainSize;
    }

    public PlanetData(int _seaLevel, Vector2 _terrainSize)
    {
        seaLevel = _seaLevel;
        terrainSize = _terrainSize;
    }

    public PlanetData(int _seaLevel, Vector2 _terrainSize, Color _colorBelowZero, Color _colorZero, Color _colorLevelOne, Color _colorLevelTwo)
    {
        seaLevel = _seaLevel;
        terrainSize = _terrainSize;
        belowZero = _colorBelowZero;
        zero = _colorZero;
        levelOne = _colorLevelOne;
        levelTwo = _colorLevelTwo;
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++ FUNCTIONS ++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public static PlanetData GetRandomPlanetData()
    {
        return new PlanetData(Random.Range(- PLANET.seaLevelVariance.Item1, PLANET.seaLevelVariance.Item2),
                            TERRAIN.baseSize + new Vector2(Random.Range(-TERRAIN.sizeVariance.x, TERRAIN.sizeVariance.x)
                                                        , Random.Range(-TERRAIN.sizeVariance.y, TERRAIN.sizeVariance.y))
                            );
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++ GETTER +++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public int GetSeaLevel()
    {
        return seaLevel;
    }

    public Color GetColorBelowZero()
    {
        return belowZero;
    }

    public Color GetColorZero()
    {
        return zero;
    }

    public Color GetColorLevelOne()
    {
        return levelOne;
    }

    public Color GetColorLevelTwo()
    {
        return levelTwo;
    }

    public Vector2 GetTerrainSize()
    {
        return terrainSize;
    }
}

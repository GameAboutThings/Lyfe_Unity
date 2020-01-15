using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class MapChunk : ChunkTemplate
{
    Map map;
    Vector3[,] heightMap;
    List<GameObject> tiles;

    HexTileMapGenerator_V2 mapGenerator;

    int tileCountX = 50;
    int tileCountY = 50;
    int tileCountZ = 50;

    private float spacingX = 1f;
    private float spacingZ = 1f;
    private float granularity = .2f;
    private Vector2 seedOffset;

    PlanetData planetData;

    Vector3 actualCenter;

    public MapChunk(Map _map, Vector3 _center, int _tileCountX, int _tileCountY, int _tileCountZ)
    {
        map = _map;
        center = _center;
        tileCountX = _tileCountX;
        tileCountY = _tileCountY;
        tileCountZ = _tileCountZ;

        planetData = _map.GetPlanetData();
        mapGenerator = _map.GetMapGenerator();
        seedOffset = mapGenerator.GetSeed().GetWorldOffset();
    }

    public void GenerateMap()
    {
        heightMap = GenerateComplexHeightMap();

        if (tiles == null)
            tiles = new List<GameObject>();

        for (int x = 0; x < tileCountX; x++)
        {
            for (int z = 0; z < tileCountZ; z++)
            {
                Vector3 pos = heightMap[x, z];

                float y = pos.y;

                if (float.IsNaN(y))
                    y = 0;

                //move hex to its spot
                if (z % 2 == 0)
                {
                    pos = new Vector3(pos.x * TILES.Offset.x - (0.5f * tileCountX),
                                    y,
                                    pos.z * TILES.Offset.z - (0.5f * tileCountZ));
                }
                else
                {
                    pos = new Vector3(pos.x * TILES.Offset.x + (TILES.Offset.x / 2f) - (0.5f * tileCountX),
                                    y,
                                    pos.z * TILES.Offset.z - (0.5f * tileCountZ));
                }

                if (x == tileCountX / 2 && z == tileCountZ / 2)
                    actualCenter = new Vector3(pos.x, 0, pos.z);

                GameObject tile = map.SetTile(pos);
                tiles.Add(tile);
                SetTileInfo(tile, x, z);
            }
        }
    }

    private void SetTileInfo(GameObject _tile, int _x, int _z)
    {
        _tile.GetComponentInChildren<MeshRenderer>().material.SetColor("Color_8F328A1B", CalculateTileColor(_tile.transform.position));
        //_tile.GetComponentInChildren<MeshRenderer>().material.color = CalculateTileColor(_tile.transform.position);     
    }

    public void Clear()
    {
        if (tiles == null)
            return;

        while (tiles.Count != 0)
        {
            mapGenerator.DestroyGameObject(tiles[0]);
            tiles.Remove(tiles[0]);
        }
    }

    private Color CalculateTileColor(Vector3 _tilePosition)
    {
        Color belowZero = planetData.GetColorBelowZero();
        Color zero = planetData.GetColorZero();
        Color levelOne = planetData.GetColorLevelOne();
        Color levelTwo = planetData.GetColorLevelTwo();

        float[] distances = new float[] {
            Mathf.Abs(_tilePosition.y - (-7f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (0f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (7f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (12f + mapGenerator.GetSeaLevel()))
        };

        Color averageColor = new Color(
            //first component (RED)
            CalculateWeightedValue(new float[] { belowZero.r, zero.r, levelOne.r, levelTwo.r }, distances),
            //second component (GREEN)
            CalculateWeightedValue(new float[] { belowZero.g, zero.g, levelOne.g, levelTwo.g }, distances),
            //third component (BLUE)
            CalculateWeightedValue(new float[] { belowZero.b, zero.b, levelOne.b, levelTwo.b }, distances)
            );

        return averageColor;
    }

    private float CalculateTileSmoothness(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        TerrainChunk[,] chunks = mapGenerator.GetTerrainChunks();

        int dim = mapGenerator.GetTerrainDimension();

        float[] smoothnesses = new float[dim * dim];

        for (int x = 0; x < dim; x++)
        {
            for (int y = 0; y < dim; y++)
            {
                smoothnesses[y * dim + x] = chunks[x, y].GetTerrainData().GetSmoothness();
            }
        }

        float smoothness = CalculateWeightedValue(/*new float[] { s00, s10, s20, s01, s11, s12, s20, s21, s22 }*/smoothnesses, GetDistancesToChunkCenters(pos2D));

        return smoothness;
    }

    private float CalculateTileHeight(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        TerrainChunk[,] chunks = mapGenerator.GetTerrainChunks();

        int dim = mapGenerator.GetTerrainDimension();

        float[] heights = new float[dim * dim];

        for (int x = 0; x < dim; x++)
        {
            for (int y = 0; y < dim; y++)
            {
                heights[y * dim + x] = chunks[x, y].GetTerrainData().GetHeight();
            }
        }

        float smoothness = CalculateWeightedValue(/*new float[] { h00, h10, h20, h01, h11, h12, h20, h21, h22 }*/ heights, GetDistancesToChunkCenters(pos2D));

        return smoothness;
    }

    private float[] GetDistancesToChunkCenters(Vector2 _tilePosition)
    {
        Vector2 pos2D = _tilePosition;

        TerrainChunk[,] chunks = mapGenerator.GetTerrainChunks();

        int dim = mapGenerator.GetTerrainDimension();
        float[] distances = new float[dim * dim];

        for (int x = 0; x < dim; x++)
        {
            for (int y = 0; y < dim; y++)
            {
                distances[y * dim + x] = StaticMaths.Distance2D(pos2D, StaticMaths.ThreeDTo2D(chunks[x, y].GetCenter(), StaticMaths.EPlane.E_XZ));
            }
        }

        return distances; //new float[] { d00, d10, d20, d01, d11, d12, d20, d21, d22 };
    }

    private float CalculateWeightedValue(float[] _values, float[] _distances)
    {
        float summands = 0;
        float sumDistances = 0;
        float maxDistance = 3 * (map.GetMapGenerator().GetPlanetData().GetTerrainSize().x + map.GetMapGenerator().GetPlanetData().GetTerrainSize().y) / 2f;
        int maxWeight = TERRAIN.maxWeight;


        for (int i = 0; i < _values.Length; i++)
        {
            for (int j = 1; j <= maxWeight; j = j + 1)
            {
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    sumDistances += _distances[i];
                    summands++;
                }
                else
                    j = maxWeight + 2;
            }
        }

        float value = 0;

        for (int i = 0; i < _values.Length; i++)
        {
            for (int j = 1; j < maxWeight; j++)
            {
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    value += (_values[i] * (sumDistances - _distances[i]) / sumDistances);
                }
                else
                    j = maxWeight;
            }
        }

        value /= summands;

        return value;
    }

    private Vector3 GetHeightAt(float _x, float _z)
    {
        Vector3 pos = new Vector3(_x * spacingX, 0, _z * spacingZ);
        float y = StaticMaths.Cap(
            Mathf.PerlinNoise(_x * granularity + seedOffset.x, _z * granularity + seedOffset.y) * 10f / CalculateTileSmoothness(pos)
                + CalculateTileHeight(pos),
            -tileCountY,
            tileCountY);

        float heightStep = mapGenerator.GetDiscreteHeightStep();

        if (heightStep != -1)
        {
            y = StaticMaths.Descretize(y, heightStep);
        }

        return new Vector3(pos.x, y, pos.z);
    }

    private Vector3[,] GenerateComplexHeightMap()
    {
        Vector3 centerTemp = StaticMaths.DivideVector3D(center, TILES.Offset);

        Vector3[,] heightMap = new Vector3[tileCountX + 1, tileCountZ + 1];

        for (int z = 0; z <= tileCountZ; z++)
        {
            for (int x = 0; x <= tileCountX; x++)
            {
                heightMap[x, z] = GetHeightAt(x + centerTemp.x, z + centerTemp.z);
            }
        }

        return heightMap;
    }

    public Vector3 GetActualCenter()
    {
        return actualCenter;
    }

    public void SetCenter(Vector3 _position)
    {
        center = _position;
    }
}

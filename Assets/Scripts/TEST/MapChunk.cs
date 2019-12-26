using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class MapChunk : MonoBehaviour
{
    private GameObject hexTilePrefab;

    HexTileMapGenerator mapGenerator;

    Vector3[,] heightMap;
    EBiome biome;
    BIOME biomeData;

    List<GameObject> tiles;

    Vector3 center;

    Color color;

    int chunkSizeX = 50;
    int chunkSizeY = 50;
    int chunkSizeZ = 50;

    private float spacingX = 1f;
    private float spacingZ = 1f;
    private float granularity = .2f;

    MapChunk above;
    MapChunk below;
    MapChunk right;
    MapChunk left;

    public void InitChunk(EBiome _biome, Vector3 _center, int _chunkSizeX, int _chunkSizeY, int _chunkSizeZ, GameObject _hexTilePrefab, HexTileMapGenerator _mapGenerator)
    {
        biome = _biome;
        biomeData = BIOME.GetBiomeData(biome);
        center = _center;
        chunkSizeX = _chunkSizeX;
        chunkSizeY = _chunkSizeY;
        chunkSizeZ = _chunkSizeZ;
        hexTilePrefab = _hexTilePrefab;
        mapGenerator = _mapGenerator;

        color = biomeData.GetColor();
    }

    public void SetNeighbourChunk(MapChunk _chunk, UTIL.EPosition _ePosition)
    {
        switch (_ePosition)
        {
            case UTIL.EPosition.EAbove:
                if (above != null)
                    return;
                above = _chunk;
                above.SetNeighbourChunk(this, UTIL.EPosition.EBelow);
                break;
            case UTIL.EPosition.EBelow:
                if (below != null)
                    return;
                below = _chunk;
                below.SetNeighbourChunk(this, UTIL.EPosition.EAbove);
                break;
            case UTIL.EPosition.ERight:
                if (right != null)
                    return;
                right = _chunk;
                right.SetNeighbourChunk(this, UTIL.EPosition.ELeft);
                break;
            case UTIL.EPosition.ELeft:
                if (left != null)
                    return;
                left = _chunk;
                left.SetNeighbourChunk(this, UTIL.EPosition.ERight);
                break;
        }
    }

    public MapChunk GetNeighbourChunk(UTIL.EPosition _ePosition)
    {
        switch (_ePosition)
        {
            case UTIL.EPosition.EAbove:
                return above;
            case UTIL.EPosition.EBelow:
                return below;
            case UTIL.EPosition.ERight:
                return right;
            case UTIL.EPosition.ELeft:
                return left;
        }
        return null;
    }

    public void GenerateMap()
    {
        if (heightMap == null)
            heightMap = GenerateComplexHeightMap();

        if (tiles == null)
            tiles = new List<GameObject>();

        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int z = 0; z < chunkSizeZ; z++)
            {
                Vector3 pos = heightMap[x, z];

                //move hex to its spot
                if (z % 2 == 0)
                {
                    pos = new Vector3(pos.x * TILES.Offset.x - (0.5f * chunkSizeX),
                                    pos.y, 
                                    pos.z * TILES.Offset.z - (0.5f * chunkSizeZ));
                }
                else
                {
                    pos = new Vector3(pos.x * TILES.Offset.x + (TILES.Offset.x / 2) - (0.5f * chunkSizeX), 
                                    pos.y, 
                                    pos.z * TILES.Offset.z - (0.5f * chunkSizeZ));
                }
                GameObject tile = Instantiate(hexTilePrefab, pos, Quaternion.identity);
                tiles.Add(tile);
                SetTileInfo(tile, x, z);
            }
        }
    }

    private void SetTileInfo(GameObject _tile, int _x, int _z)
    {
        _tile.GetComponentInChildren<MeshRenderer>().material.color = CalculateTileColor(_tile.transform.position);
    }

    public Vector3 GetCenter()
    {
        return center;
    }

    public void Clear()
    {
        while (tiles.Count != 0)
        {
            Destroy(tiles[0]);
            tiles.Remove(tiles[0]);
        }
    }

    private Color CalculateTileColor(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        MapChunk[,] chunks = mapGenerator.GetChunks();
        Color c00 = chunks[0, 0].GetColor();
        Color c10 = chunks[1, 0].GetColor();
        Color c20 = chunks[2, 0].GetColor();
        Color c01 = chunks[0, 1].GetColor();
        Color c11 = chunks[1, 1].GetColor();
        Color c21 = chunks[2, 1].GetColor();
        Color c02 = chunks[0, 2].GetColor();
        Color c12 = chunks[1, 2].GetColor();
        Color c22 = chunks[2, 2].GetColor();

        Vector2 cen00 = StaticMaths.ThreeDTo2D(chunks[0, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen10 = StaticMaths.ThreeDTo2D(chunks[1, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen20 = StaticMaths.ThreeDTo2D(chunks[2, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen01 = StaticMaths.ThreeDTo2D(chunks[0, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen11 = StaticMaths.ThreeDTo2D(chunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen21 = StaticMaths.ThreeDTo2D(chunks[2, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen02 = StaticMaths.ThreeDTo2D(chunks[0, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen12 = StaticMaths.ThreeDTo2D(chunks[1, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen22 = StaticMaths.ThreeDTo2D(chunks[2, 2].GetCenter(), StaticMaths.EPlane.E_XZ);

        float d00 = StaticMaths.Distance2D(pos2D, cen00);
        float d10 = StaticMaths.Distance2D(pos2D, cen10);
        float d20 = StaticMaths.Distance2D(pos2D, cen20);
        float d01 = StaticMaths.Distance2D(pos2D, cen01);
        float d11 = StaticMaths.Distance2D(pos2D, cen11);
        float d21 = StaticMaths.Distance2D(pos2D, cen21);
        float d02 = StaticMaths.Distance2D(pos2D, cen02);
        float d12 = StaticMaths.Distance2D(pos2D, cen12);
        float d22 = StaticMaths.Distance2D(pos2D, cen22);

        float sumDistances = d00 + d10 + d20 + d01 + d11 + d21 + d02 + d12 + d22;

        Color averageColor = new Color(
            //first component (RED)
            CalculateWeightedValue(new float[] {c00.r, c10.r, c20.r, c01.r, c11.r, c21.r, c02.r, c12.r, c22.r}, new float[] {d00, d10, d20, d01, d11, d12, d20, d21, d22}),
            //second component (GREEN)
            CalculateWeightedValue(new float[] {c00.g, c10.g, c20.g, c01.g, c11.g, c21.g, c02.g, c12.g, c22.g}, new float[] {d00, d10, d20, d01, d11, d12, d20, d21, d22}),
            //third component (BLUE)
            CalculateWeightedValue(new float[] {c00.b, c10.b, c20.b, c01.b, c11.b, c21.b, c02.b, c12.b, c22.b}, new float[] {d00, d10, d20, d01, d11, d12, d20, d21, d22})
            );

        return averageColor;
    }

    private float CalculateTileSmoothness(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        MapChunk[,] chunks = mapGenerator.GetChunks();
        float s00 = chunks[0, 0].biomeData.GetSmoothness();
        float s10 = chunks[1, 0].biomeData.GetSmoothness();
        float s20 = chunks[2, 0].biomeData.GetSmoothness();
        float s01 = chunks[0, 1].biomeData.GetSmoothness();
        float s11 = chunks[1, 1].biomeData.GetSmoothness();
        float s21 = chunks[2, 1].biomeData.GetSmoothness();
        float s02 = chunks[0, 2].biomeData.GetSmoothness();
        float s12 = chunks[1, 2].biomeData.GetSmoothness();
        float s22 = chunks[2, 2].biomeData.GetSmoothness();

        Vector2 cen00 = StaticMaths.ThreeDTo2D(chunks[0, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen10 = StaticMaths.ThreeDTo2D(chunks[1, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen20 = StaticMaths.ThreeDTo2D(chunks[2, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen01 = StaticMaths.ThreeDTo2D(chunks[0, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen11 = StaticMaths.ThreeDTo2D(chunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen21 = StaticMaths.ThreeDTo2D(chunks[2, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen02 = StaticMaths.ThreeDTo2D(chunks[0, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen12 = StaticMaths.ThreeDTo2D(chunks[1, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen22 = StaticMaths.ThreeDTo2D(chunks[2, 2].GetCenter(), StaticMaths.EPlane.E_XZ);

        float d00 = StaticMaths.Distance2D(pos2D, cen00);
        float d10 = StaticMaths.Distance2D(pos2D, cen10);
        float d20 = StaticMaths.Distance2D(pos2D, cen20);
        float d01 = StaticMaths.Distance2D(pos2D, cen01);
        float d11 = StaticMaths.Distance2D(pos2D, cen11);
        float d21 = StaticMaths.Distance2D(pos2D, cen21);
        float d02 = StaticMaths.Distance2D(pos2D, cen02);
        float d12 = StaticMaths.Distance2D(pos2D, cen12);
        float d22 = StaticMaths.Distance2D(pos2D, cen22);

        float smoothness = CalculateWeightedValue(new float[] { s00, s10, s20, s01, s11, s12, s20, s21, s22 }, new float[] { d00, d10, d20, d01, d11, d12, d20, d21, d22 });

        return  smoothness;
    }

    private float CalculateWeightedValue(float[] _values, float[] _distances)
    {
        float summands = 0;
        float sumDistances = 0;
        float diagonal = Mathf.Sqrt(Mathf.Pow(CHUNK.Size.x * TILES.Offset.x, 2) + Mathf.Pow(CHUNK.Size.y * TILES.Offset.y, 2));
        float maxDistance = (3f / 2f) * diagonal;
        int maxWeight = 15;


        for(int i = 0 ; i < _values.Length; i++)
        {
            for (int j = 1; j < maxWeight; j++)
            {
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    sumDistances += _distances[i];
                    summands++;
                }
                else
                    j = maxWeight;
            }
        }

        float value = 0;

        for(int i = 0 ; i < _values.Length; i++)
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

    public Vector3 GetHeightAt(float _x, float _z)
    {
        Vector3 pos = new Vector3(_x * spacingX, 0, _z * spacingZ);
        float y = StaticMaths.Cap(Mathf.PerlinNoise(_x * granularity, _z * granularity) * 10f / CalculateTileSmoothness(pos), -chunkSizeY, chunkSizeY);
        return new Vector3(pos.x, y, pos.z);
    }

    public Color GetColor()
    {
        return color;
    }

    public Vector3[,] GenerateComplexHeightMap()
    {
        Vector3 centerTemp = StaticMaths.DivideVector3D(center, TILES.Offset);

        Vector3[,] heightMap = new Vector3[chunkSizeX + 1, chunkSizeZ + 1];

        for (int z = 0; z <= chunkSizeZ; z++)
        {
            for (int x = 0; x <= chunkSizeX; x++)
            {
                heightMap[x, z] = GetHeightAt(x + centerTemp.x, z + centerTemp.z);
            }
        }

        return heightMap;
    }

    public BIOME GetBiomeData()
    {
        return biomeData;
    }

    public EBiome GetBiome()
    {
        return biome;
    }
}

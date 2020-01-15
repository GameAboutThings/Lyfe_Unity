using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject hexTilePrefab;

    HexTileMapGenerator_V2 mapGenerator;

    PlanetData planetData;

    int chunkTileCountX = 50;
    int chunkTileCountY = 50;
    int chunkTileCountZ = 50;

    private int mapDimensions = MAP.mapDimensions;

    private Vector3 actualCenter;

    private MapChunk[,] mapChunks;

    public void InitMap(int _tileCountX, int _tileCountY, int _tileCountZ, HexTileMapGenerator_V2 _mapGenerator)
    {
        chunkTileCountX = _tileCountX;
        chunkTileCountY = _tileCountY;
        chunkTileCountZ = _tileCountZ;
        mapGenerator = _mapGenerator;
        planetData = _mapGenerator.GetPlanetData();
        InitChunks();
    }

    private void InitChunks()
    {
        mapChunks = new MapChunk[mapDimensions, mapDimensions];
        for (int x = 0; x < mapDimensions; x++)
        {
            for (int z = 0; z < mapDimensions; z++)
            {
                mapChunks[x, z] = new MapChunk(this,
                            new Vector3((x - mapDimensions / 2f) * chunkTileCountX * TILES.Offset.x, 0, (z - mapDimensions / 2f) * chunkTileCountZ * TILES.Offset.z) + transform.position,
                            chunkTileCountX, 
                            chunkTileCountY, 
                            chunkTileCountZ);
            }
        }
    }

    public void GenerateMap()
    {
        for (int x = 0; x < mapDimensions; x++)
        {
            for (int z = 0; z < mapDimensions; z++)
            {
                mapChunks[x, z].GenerateMap();

                if (Mathf.FloorToInt(mapDimensions / 2) == x && Mathf.FloorToInt(mapDimensions / 2) == z)
                {
                    actualCenter = mapChunks[x, z].GetActualCenter();
                }
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < mapDimensions; x++)
        {
            for (int z = 0; z < mapDimensions; z++)
            {
                mapChunks[x, z].Clear();
            }
        }
    }

    public GameObject SetTile(Vector3 _position)
    {
        GameObject tile = Instantiate(hexTilePrefab, _position, Quaternion.identity);

        tile.transform.parent = transform;

        return tile;
    }

    public Vector3 GetActualCenter()
    {
        return actualCenter;
    }

    public PlanetData GetPlanetData()
    {
        return planetData;
    }

    public HexTileMapGenerator_V2 GetMapGenerator()
    {
        return mapGenerator;
    }

    public void MoveTo(Vector3 _position)
    {
        //really.... really need to rework this one
        transform.position = _position;
        for (int x = 0; x < mapDimensions; x++)
        {
            for (int z = 0; z < mapDimensions; z++)
            {
                mapChunks[x, z].SetCenter(new Vector3((x - mapDimensions / 2f) * chunkTileCountX * TILES.Offset.x, 0, (z - mapDimensions / 2f) * chunkTileCountZ * TILES.Offset.z) + transform.position);
            }
        }
        Clear();
        GenerateMap();
    }
}

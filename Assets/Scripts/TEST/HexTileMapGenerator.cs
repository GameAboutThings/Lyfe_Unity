using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileMapGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject hexTilePrefab;

    int mapWidth = 25;
    int mapHeight = 12;

    float tileOffsetX = 1.73f;
    float tileOffsetZ = 1.5f;

    void Start()
    {
        CreateMap();
    }

    private void CreateMap()
    {
        TerrainGenerator_Simple tg = new TerrainGenerator_Simple();
        Vector3[,] heightMap = tg.GenerateHeightMap();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                Vector3 pos = heightMap[x, z];

                //move hex to its spot
                if (z % 2 == 0)
                {
                    pos = new Vector3(pos.x * tileOffsetX, pos.y, pos.z * tileOffsetZ);
                }
                else
                {
                    pos = new Vector3(pos.x * tileOffsetX + (tileOffsetX / 2), pos.y, pos.z * tileOffsetZ);
                }
                GameObject tile = Instantiate(hexTilePrefab, pos, Quaternion.identity);
                SetTileInfo(tile, x, z);
            }
        }
    }

    private void SetTileInfo(GameObject _tile, int _x, int _z)
    {

    }
}

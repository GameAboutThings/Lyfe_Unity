using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator_Simple
{
    private int sizeX = 50;
    private float sizeY = 5f; //deviance from absolute 0
    private int sizeZ = 50;
    private int smoothingRoutines = 10;
    private float spacingX = 1f;
    private float spacingZ = 1f;

    public Vector3[,] GenerateHeightMap()
    {
        Vector3[,] heightMap = new Vector3[sizeX + 1, sizeZ + 1];

        for (int z = 0; z <= sizeZ; z++)
        {
            for (int x = 0; x <= sizeX; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 4f;
                Debug.Log(x + "/" + z + " = " + y);
                heightMap[x, z] = new Vector3(x * spacingX, y, z * spacingZ);
            }
        }

        return heightMap;
    }

    public Vector3[,] GenerateHeightMap(int _sizeX, int _sizeY, int _sizeZ, int _smoothingRoutines, float _spacingX, float _spacingZ)
    {
        sizeX = _sizeX;
        sizeY = _sizeY;
        sizeZ = _sizeZ;
        smoothingRoutines = _smoothingRoutines;
        spacingX = _spacingX;
        spacingZ = _spacingZ;

        return GenerateHeightMap();
    }
}

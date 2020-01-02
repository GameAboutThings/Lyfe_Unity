using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator_Simple
{
    private float heightCap = Meta_MapGenerator.CHUNK.Size.y; //deviance from absolute 0
    private float smoothness = 5;
    private float spacingX = 1f;
    private float spacingZ = 1f;
    private float granularity = .2f;

    public TerrainGenerator_Simple(int _heightCap, float _smoothness, float _spacingX, float _spacingZ, float _granularity)
    {
        heightCap = _heightCap;
        smoothness = _smoothness;
        spacingX = _spacingX;
        spacingZ = _spacingZ;
        granularity = _granularity;
    }

    public TerrainGenerator_Simple()
    {

    }

    public Vector3 GetHeightAt(float _x, float _z)
    {
        return GetHeightAt(_x, _z, smoothness);
    }

    public Vector3 GetHeightAt(float _x, float _z, float _smoothness)
    {
        float y = StaticMaths.Cap(Mathf.PerlinNoise(_x * granularity, _z * granularity) * 10f / _smoothness, -heightCap, heightCap);
        return new Vector3(_x * spacingX, y, _z * spacingZ);
    }

    public Vector3[,] GenerateHeightMap(int _sizeX, int _sizeZ, Vector3 _center)
    {
        Vector3[,] heightMap = new Vector3[_sizeX + 1, _sizeZ + 1];

        for (int z = 0; z <= _sizeZ; z++)
        {
            for (int x = 0; x <= _sizeX; x++)
            {
                heightMap[x, z] = GetHeightAt(x + _center.x, z + _center.z);
            }
        }

        return heightMap;
    }
}

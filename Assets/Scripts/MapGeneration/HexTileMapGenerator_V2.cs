using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class HexTileMapGenerator_V2 : MonoBehaviour
{
    [SerializeField]
    GameObject playerCamera;
    [SerializeField]
    GameObject waterLevel;
    [SerializeField]
    GameObject mapPrefab;
    [SerializeField]
    bool loadMoreChunks = false;

    Seed seed;

    int chunkTileCountX = MAP.tileCount.x;
    int chunkTileCountY = MAP.tileCount.y;
    int chunkTileCountZ = MAP.tileCount.z;

    float discreteHeightStep = 1f; //-1 for continuous 

    Vector3 currentCenter; //world position of the currently visible central chunk
    Vector2Int currentStoragePosition; //where in the storage the current center chunk is positioned

    int terrainDimension = MISC.TerrainDimension;

    bool rearrangingBlock = false;

    //[(0,2) (1,2) (2,2)]
    //[(0,1) (1,1) (2,1)]
    //[(0,0) (1,0) (2,0)]
    GameObject mapObject;
    Map map; //part of the map that's currently loaded
    TerrainChunk[,] terrainChunks;
    MeshList<TerrainChunk> terrainChunkStorage;

    PlanetData planetData;

    void Start()
    {
        seed = new Seed();
        InstantiateMap();
    }

    void FixedUpdate()
    {
        UpdateMap();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            map.Clear();
            Destroy(mapObject);
            InstantiateMap();
        }

        DrawLinesToChunkCenters();
    }

    private void InstantiateMap()
    {
        planetData = PlanetData.GetRandomPlanetData();

        TERRAIN.ETerrain[,] terrain = new TERRAIN.ETerrain[terrainDimension, terrainDimension];

        for (int i = 0; i < terrainDimension; i++)
            for (int j = 0; j < terrainDimension; j++)
                terrain[i, j] = TerrainData.GetRandomTerrainByFrequency();


        mapObject = Instantiate(mapPrefab, new Vector3(), Quaternion.identity);
        map = mapObject.GetComponent<Map>();
        map.InitMap(chunkTileCountX, chunkTileCountY, chunkTileCountZ, this);

        InstantiateChunks(terrain);

        map.MoveTo(currentCenter);
        waterLevel.transform.position = map.GetActualCenter();
    }

    private void InstantiateChunks(TERRAIN.ETerrain[,] _startTerrain)
    {
        terrainChunks = new TerrainChunk[terrainDimension, terrainDimension];
        terrainChunkStorage = new MeshList<TerrainChunk>();
        int halfDim = GetTerrainDimCenter();
        for (int x = 0; x < terrainDimension; x++)
        {
            for (int z = 0; z < terrainDimension; z++)
            {
                terrainChunks[x, z] = new TerrainChunk(_startTerrain[x, z], 
                                                        new Vector3(planetData.GetTerrainSize().x * (x - halfDim), 0, planetData.GetTerrainSize().y * (z - halfDim)) + currentCenter,
                                                        planetData.GetTerrainSize(),
                                                        this);
            }
        }

        

        terrainChunkStorage.SetCenter(terrainChunks[halfDim, halfDim]);
        //first do the middle line
        for (int x = 0; x < halfDim; x++)
        {
            terrainChunkStorage.AddLeft(-x, 0, terrainChunks[halfDim - x - 1, halfDim]);
            terrainChunkStorage.AddRight(x, 0, terrainChunks[halfDim + x + 1, halfDim]);
        }

        //the in parallel always do the line above and below in one set
        for (int y = 0; y < halfDim; y++)
        {
            terrainChunkStorage.AddAbove(0, y, terrainChunks[halfDim, halfDim + y + 1]);
            terrainChunkStorage.AddBelow(0, - y, terrainChunks[halfDim, halfDim - y - 1]);

            for (int x = 0; x < halfDim; x++)
            {
                terrainChunkStorage.AddLeft(-x, y + 1, terrainChunks[halfDim - x - 1, halfDim + y + 1]);
                terrainChunkStorage.AddRight(x, y + 1, terrainChunks[halfDim + x + 1, halfDim + y + 1]);
            }
            for (int x = 0; x < halfDim; x++)
            {
                terrainChunkStorage.AddLeft(-x, -(y + 1), terrainChunks[halfDim - x - 1, halfDim - y - 1]);
                terrainChunkStorage.AddRight(x, -(y + 1), terrainChunks[halfDim + x + 1, halfDim - y - 1]);
            }
        }

        ReloadChunkArrayFromStorage();

        currentStoragePosition = new Vector2Int(0, 0);
    }

    private void UpdateMap()
    {
        if (rearrangingBlock || !loadMoreChunks)
            return;

        Vector3 cameraPos = playerCamera.transform.position;
        UTIL.EPosition playerPositionMap = GetCameraPositionToCenterMap(cameraPos);
        UTIL.EPosition playerPositionTerrain = GetCameraPositionToCenterTerrain(cameraPos);
        RearrangeChunksAroundCenter(playerPositionMap, playerPositionTerrain);
    }

    private UTIL.EPosition GetCameraPositionToCenterMap(Vector3 _position)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);
        UTIL.EPosition pos = UTIL.EPosition.ECenter;


        if (StaticMaths.WithinBoundingBox(pos2D,
            StaticMaths.ThreeDTo2D(map.GetActualCenter(), StaticMaths.EPlane.E_XZ),
            new Vector2(chunkTileCountX * TILES.Offset.x, chunkTileCountZ * TILES.Offset.z) + StaticMaths.ThreeDTo2D(TILES.Offset, StaticMaths.EPlane.E_XZ) * 2))
        {
            pos = UTIL.EPosition.ECenter;
        }
        else
        {
            if (pos2D.y < mapObject.transform.position.z + (chunkTileCountZ * TILES.Offset.z))
            {
                pos = UTIL.EPosition.EAbove;
            }
            else if (pos2D.y > mapObject.transform.position.z - (chunkTileCountZ * TILES.Offset.z))
            {
                pos = UTIL.EPosition.EBelow;
            }
            else if (pos2D.x < mapObject.transform.position.x + (chunkTileCountX * TILES.Offset.x))
            {
                pos = UTIL.EPosition.ELeft;
            }
            else if (pos2D.x > mapObject.transform.position.x - (chunkTileCountX * TILES.Offset.x))
            {
                pos = UTIL.EPosition.ERight;
            }
        }

        return pos;
    }

    private UTIL.EPosition GetCameraPositionToCenterTerrain(Vector3 _position)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);
        UTIL.EPosition pos = UTIL.EPosition.ECenter;

        int halfDim = GetTerrainDimCenter();

        if (StaticMaths.WithinBoundingBox(pos2D,
            StaticMaths.ThreeDTo2D(StaticMaths.MultiplyVector3D(terrainChunks[halfDim, halfDim].GetCenter(), TILES.Offset), StaticMaths.EPlane.E_XZ),
            planetData.GetTerrainSize() + StaticMaths.ThreeDTo2D(TILES.Offset, StaticMaths.EPlane.E_XZ) * 2))
        {
            
        }
        else
        {
            if (pos2D.y < terrainChunks[halfDim, halfDim].GetCenter().z - planetData.GetTerrainSize().y / 2)
            {
                pos = UTIL.EPosition.EAbove;
            }
            else if (pos2D.y > terrainChunks[halfDim, halfDim].GetCenter().z + planetData.GetTerrainSize().y / 2)
            {
                pos = UTIL.EPosition.EBelow;
            }
            else if (pos2D.x < terrainChunks[halfDim, halfDim].GetCenter().x - planetData.GetTerrainSize().x / 2)
            {
                pos = UTIL.EPosition.ELeft;
            }
            else if (pos2D.x > terrainChunks[halfDim, halfDim].GetCenter().x + planetData.GetTerrainSize().x / 2)
            {
                pos = UTIL.EPosition.ERight;
            }
        }

        return pos;
    }

    private void RearrangeChunksAroundCenter(UTIL.EPosition _positionMap, UTIL.EPosition _positionTerrain)
    {
        if (_positionMap == UTIL.EPosition.ECenter)
            return;

        rearrangingBlock = true;

        currentCenter = new Vector3(playerCamera.transform.position.x, 0f, playerCamera.transform.position.z);

        if (_positionTerrain == UTIL.EPosition.EAbove) //player went too far up
        {
            MoveLoadedSectionDown();
        }
        else if (_positionTerrain == UTIL.EPosition.EBelow) //player went too far down
        {
            MoveLoadedSectionUp();
        }
        else if (_positionTerrain == UTIL.EPosition.ELeft) //player went too far left
        {
            MoveLoadedSectionLeft();
        }
        else if (_positionTerrain == UTIL.EPosition.ERight) //player went too far right
        {
            MoveLoadedSectionRight();
        }

        map.MoveTo(currentCenter);
        waterLevel.transform.position = map.GetActualCenter();        

        StartCoroutine(LiftRearrangeBlock());
    }

    private IEnumerator LiftRearrangeBlock()
    {
        yield return new WaitForSeconds(1f);
        rearrangingBlock = false;
    }

    /*
     * Moves down one step in the storage and loads that section
     */
    private void MoveLoadedSectionDown()
    {
        MoveLoadedSectionVertically(false);
    }

    /*
     * Moves up one step in the storage and loads that section
     */
    private void MoveLoadedSectionUp()
    {
        MoveLoadedSectionVertically(true);
    }

    private void MoveLoadedSectionRight()
    {
        MoveLoadedSectionHorizontally(true);
    }

    private void MoveLoadedSectionLeft()
    {
        MoveLoadedSectionHorizontally(false);
    }

    private void MoveLoadedSectionVertically(bool _up)
    {
        int row;

        if (_up)
            row = terrainDimension - 1;
        else
            row = 0;

        //move the center up by one step
        if (_up)
            currentStoragePosition = new Vector2Int(currentStoragePosition.x, currentStoragePosition.y + 1);
        else
            currentStoragePosition = new Vector2Int(currentStoragePosition.x, currentStoragePosition.y - 1);

        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();

        int halfDim = GetTerrainDimCenter();

        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = -halfDim; i <= halfDim; i++)
        {
            if (terrainChunks[i + halfDim, row] == null)
            {
                //Debug.Log(terrainChunks[i + halfDim, row]);
                terrainChunks[i + halfDim, row] = new TerrainChunk(TerrainData.GetRandomTerrainByFrequency(),
                    new Vector3(planetData.GetTerrainSize().x * i, 0, planetData.GetTerrainSize().y * (row - halfDim)) + currentCenter,
                    planetData.GetTerrainSize(),
                    this);

                if (_up)
                    terrainChunkStorage.AddAbove(currentStoragePosition.x + i, currentStoragePosition.y + halfDim - 1, terrainChunks[i + halfDim, row]);
                else
                    terrainChunkStorage.AddBelow(currentStoragePosition.x + i, currentStoragePosition.y - halfDim + 1, terrainChunks[i + halfDim, row]);
            }


        }

        //Reload the chunks after generating the missing ones
        //ReloadChunkArrayFromStorage();
    }

    private void MoveLoadedSectionHorizontally(bool _right)
    {
        int column;

        if (_right)
            column = terrainDimension - 1;
        else
            column = 0;

        //move the center up by one step
        if (_right)
            currentStoragePosition = new Vector2Int(currentStoragePosition.x + 1, currentStoragePosition.y);
        else
            currentStoragePosition = new Vector2Int(currentStoragePosition.x - 1, currentStoragePosition.y);

        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();

        int halfDim = GetTerrainDimCenter();

        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = -halfDim; i <= halfDim; i++)
        {
            if (terrainChunks[column, i + halfDim] == null)
            {
                //Debug.Log(terrainChunks[i + halfDim, row]);
                terrainChunks[column, i + halfDim] = new TerrainChunk(TerrainData.GetRandomTerrainByFrequency(),
                    new Vector3(planetData.GetTerrainSize().x * (column - halfDim), 0, planetData.GetTerrainSize().y * i) + currentCenter,
                    planetData.GetTerrainSize(),
                    this);

                if (_right)
                    terrainChunkStorage.AddRight(currentStoragePosition.x + halfDim - 1, currentStoragePosition.y + i, terrainChunks[column, i + halfDim]);
                else
                    terrainChunkStorage.AddLeft(currentStoragePosition.x - halfDim + 1, currentStoragePosition.y + i, terrainChunks[column, i + halfDim]);
            }


        }

        //Reload the chunks after generating the missing ones
        //ReloadChunkArrayFromStorage();
    }

    public void DestroyGameObject(GameObject _object)
    {
        Destroy(_object);
    }

    private void ReloadChunkArrayFromStorage()
    {
        terrainChunks = terrainChunkStorage.Get2DArray(currentStoragePosition.x - GetTerrainDimCenter(), currentStoragePosition.y - GetTerrainDimCenter(), terrainDimension, terrainDimension);
        //string str = "";
        //for (int y = 0; y < terrainDimension; y++)
        //{
        //    for(int x = 0; x < terrainDimension; x++)
        //    {
        //        if (terrainChunks[x, y] == null)
        //            str += "[ ]";
        //        else
        //            str += "[X]";
        //    }
        //    str += "\n";
        //}
        //Debug.Log(str);
    }

    public Map GetMap()
    {
        return map;
    }

    public TerrainChunk[,] GetTerrainChunks()
    {
        return terrainChunks;
    }

    public float GetDiscreteHeightStep()
    {
        return discreteHeightStep;
    }

    public Seed GetSeed()
    {
        return seed;
    }

    public PlanetData GetPlanetData()
    {
        return planetData;
    }

    public float GetSeaLevel()
    {
        return planetData.GetSeaLevel() * discreteHeightStep;
    }

    public int GetTerrainDimension()
    {
        return terrainDimension;
    }

    private void DrawLinesToChunkCenters()
    {
        Vector3 cameraPos = playerCamera.transform.position;

        for (int x = 0; x < terrainDimension; x++)
        {
            for (int z = 0; z < terrainDimension; z++)
            {
                Debug.DrawLine(cameraPos, terrainChunks[x, z].GetCenter(), new Color(1, (float)x / (float)terrainDimension, (float)z / (float)terrainDimension));
            }
        }
    }

    private int GetTerrainDimCenter()
    {
        return Mathf.FloorToInt(terrainDimension / 2f);
    }
}

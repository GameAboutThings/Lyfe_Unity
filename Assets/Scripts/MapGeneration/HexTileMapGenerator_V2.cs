using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class HexTileMapGenerator_V2 : MonoBehaviour
{
    [SerializeField]
    private GameObject hexTilePrefab;
    [SerializeField]
    GameObject playerCamera;
    [SerializeField]
    GameObject waterLevel;

    [SerializeField]
    string seed;

    int chunkTileCountX = CHUNK.tileCount.x;
    int chunkTileCountY = CHUNK.tileCount.y;
    int chunkTileCountZ = CHUNK.tileCount.z;

    float discreteHeightStep = 1f; //-1 for continuous 

    Vector3 currentCenter; //world position of the currently visible central chunk
    Vector2Int currentStoragePosition; //where in the storage the current center chunk is positioned

    int terrainDimension = 3;

    //[(0,2) (1,2) (2,2)]
    //[(0,1) (1,1) (2,1)]
    //[(0,0) (1,0) (2,0)]
    MapChunk[,] mapChunks; //currently loaded chunks
    TerrainChunk[,] terrainChunks;
    MeshList<TerrainChunk> terrainChunkStorage;

    PlanetData planetData;

    void Start()
    {
        InstantiateMap();
    }

    void FixedUpdate()
    {
        //UpdateMap();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    mapChunks[i, j].Clear();
                    //Destroy(mapChunks[i, j]);
                }


            InstantiateMap();
        }

        DrawLinesToChunkCenters();
        //DrawChunkBounds();
    }

    private void InstantiateMap()
    {
        seed = UTIL.GetMapSeed();

        planetData = PlanetData.GetRandomPlanetData();

        TERRAIN.ETerrain[,] terrain = new TERRAIN.ETerrain[3, 3];

        for (int i = 0; i < terrainDimension; i++)
            for (int j = 0; j < terrainDimension; j++)
                terrain[i, j] = TerrainData.GetRandomTerrainByFrequency();

        terrain[0, 0] = TERRAIN.ETerrain.EMountains;
        terrain[1, 0] = TERRAIN.ETerrain.EMountains;
        terrain[2, 0] = TERRAIN.ETerrain.EMountains;

        InstantiateChunks(terrain);

        waterLevel.transform.position = new Vector3(currentCenter.x, planetData.GetSeaLevel() * discreteHeightStep, currentCenter.z);
    }

    private void InstantiateChunks(TERRAIN.ETerrain[,] _startTerrain)
    {
        mapChunks = new MapChunk[3, 3];
        terrainChunks = new TerrainChunk[terrainDimension, terrainDimension];
        terrainChunkStorage = new MeshList<TerrainChunk>();

        mapChunks[0, 0] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(-chunkTileCountX, 0, -chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[1, 0] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(0, 0, -chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[2, 0] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX, 0, -chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);

        mapChunks[0, 1] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(-chunkTileCountX, 0, 0), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[1, 1] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(0, 0, 0), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[2, 1] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX, 0, 0), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);

        mapChunks[0, 2] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(-chunkTileCountX, 0, chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[1, 2] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(0, 0, chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
        mapChunks[2, 2] = new MapChunk(StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX, 0, chunkTileCountZ), TILES.Offset), chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);

        for (int x = 0; x < terrainDimension; x++)
        {
            for (int z = 0; z < terrainDimension; z++)
            {
                terrainChunks[x, z] = new TerrainChunk(_startTerrain[x, z], 
                                                        new Vector3(planetData.GetTerrainSize().x * (x - Mathf.FloorToInt(terrainDimension / 2f)), 0, planetData.GetTerrainSize().y * (int)(z - Mathf.FloorToInt(terrainDimension / 2f))),
                                                        planetData.GetTerrainSize(),
                                                        this);
            }
        }

        //middle row
        terrainChunkStorage.SetCenter(terrainChunks[1, 1]);
        terrainChunkStorage.AddLeft(0, 0, terrainChunks[0, 1]);
        terrainChunkStorage.AddRight(0, 0, terrainChunks[2, 1]);

        //top row
        terrainChunkStorage.AddAbove(0, 0, terrainChunks[1, 2]);
        terrainChunkStorage.AddLeft(0, 1, terrainChunks[0, 2]);
        terrainChunkStorage.AddRight(0, 1, terrainChunks[2, 2]);

        //bottom row
        terrainChunkStorage.AddBelow(0, 0, terrainChunks[1, 0]);
        terrainChunkStorage.AddLeft(0, -1, terrainChunks[0, 0]);
        terrainChunkStorage.AddRight(0, -1, terrainChunks[2, 0]);

        ReloadChunkArrayFromStorage();

        mapChunks[0, 0].GenerateMap();
        mapChunks[1, 0].GenerateMap();
        mapChunks[2, 0].GenerateMap();
        mapChunks[0, 1].GenerateMap();
        mapChunks[1, 1].GenerateMap();
        mapChunks[2, 1].GenerateMap();
        mapChunks[0, 2].GenerateMap();
        mapChunks[1, 2].GenerateMap();
        mapChunks[2, 2].GenerateMap();

        currentStoragePosition = new Vector2Int(0, 0);
    }

    private void UpdateMap()
    {
        Vector3 cameraPos = playerCamera.transform.position;
        //Vector2 playerChunkIndex = GetCorrespondingChunkIndex(cameraPos);
        UTIL.EPosition playerPosition = GetCameraPosition(cameraPos);

        RearrangeChunksAroundCenter(playerPosition);
    }

    private UTIL.EPosition GetCameraPosition(Vector3 _position)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);
        UTIL.EPosition pos = UTIL.EPosition.ECenter;


        if (StaticMaths.WithinBoundingBox(pos2D,
            StaticMaths.ThreeDTo2D(mapChunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ),
            new Vector2(chunkTileCountX * TILES.Offset.x, chunkTileCountZ * TILES.Offset.z) + StaticMaths.ThreeDTo2D(TILES.Offset, StaticMaths.EPlane.E_XZ) * 2))
        {
            pos = UTIL.EPosition.ECenter;
        }
        else
        {
            if (pos2D.y < mapChunks[1, 1].GetCenter().z - 5f)
            {
                pos = UTIL.EPosition.EAbove;
            }
            else if (pos2D.y > mapChunks[1, 1].GetCenter().z + 5f)
            {
                pos = UTIL.EPosition.EBelow;
            }
            else if (pos2D.x < mapChunks[1, 1].GetCenter().x - 5f)
            {
                pos = UTIL.EPosition.ELeft;
            }
            else if (pos2D.x > mapChunks[1, 1].GetCenter().x + 5f)
            {
                pos = UTIL.EPosition.ERight;
            }
        }

        return pos;
    }

    private void RearrangeChunksAroundCenter(UTIL.EPosition _position)
    {
        if (_position == UTIL.EPosition.ECenter)
            return;

        if (_position == UTIL.EPosition.EAbove) //player went too far up
        {
            currentCenter = mapChunks[1, 0].GetCenter();
            //MoveChunksDown(true);
            MoveLoadedSectionDown();
        }
        else if (_position == UTIL.EPosition.EBelow) //player went too far down
        {
            currentCenter = mapChunks[1, 2].GetCenter();
            //MoveChunksUp(true);
            MoveLoadedSectionUp();
        }
        else if (_position == UTIL.EPosition.ELeft) //player went too far left
        {
            currentCenter = mapChunks[0, 1].GetCenter();
            //MoveChunksRight(true);
            
            MoveLoadedSectionLeft();
        }
        else if (_position == UTIL.EPosition.ERight) //player went too far right
        {
            currentCenter = mapChunks[2, 1].GetCenter();
            //MoveChunksLeft(true);
            MoveLoadedSectionRight();
        }

        waterLevel.transform.position = new Vector3(currentCenter.x, waterLevel.transform.position.y, currentCenter.z);
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
        //[(0,2) (1,2) (2,2)]
        //[(0,1) (1,1) (2,1)]
        //[(0,0) (1,0) (2,0)]

        int row = 0;

        if (_up)
            row = 2;

        for (int i = 0; i < 3; i++)
        {
            mapChunks[i, 2 - row].Clear();
        }

        //move the center up by one step
        currentStoragePosition = new Vector2Int(currentStoragePosition.x, currentStoragePosition.y + (row - 1));

        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();


        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = 0; i < 3; i++)
        {
            if (mapChunks[i, row] == null)
            {
                //mapChunks[i, row] = gameObject.AddComponent<MapChunk>();
                //mapChunks[i, row].InitChunk(TerrainData.GetRandomTerrainByFrequency(), StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX * (i - 1), 0, chunkTileCountZ * (row - 1)), TILES.Offset) + currentCenter, chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
                terrainChunks[i, row] = new TerrainChunk(TerrainData.GetRandomTerrainByFrequency(),
                    StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX * (i - 1), 0, chunkTileCountZ * (row - 1)), TILES.Offset) + currentCenter,
                    planetData.GetTerrainSize(),
                    this);
                if (_up)
                    terrainChunkStorage.AddAbove(currentStoragePosition.x + (i - 1), currentStoragePosition.y, terrainChunks[i, row]);
                    //mapChunkStorage.HardInsert(chunks[i, row], new Vector2Int(currentStoragePosition.x + (i - 1), currentStoragePosition.y + 1));
                else
                    terrainChunkStorage.AddBelow(currentStoragePosition.x + (i - 1), currentStoragePosition.y, terrainChunks[i, row]);
                    //mapChunkStorage.HardInsert(chunks[i, row], new Vector2Int(currentStoragePosition.x + (i - 1), currentStoragePosition.y - 1));
            }
        }

        //Reload the chunks after generating the missing ones
        ReloadChunkArrayFromStorage();

        for (int i = 0; i < 3; i++)
        {
            mapChunks[i, row].GenerateMap();
        }
    }

    private void MoveLoadedSectionHorizontally(bool _right)
    {
        //[(0,2) (1,2) (2,2)]
        //[(0,1) (1,1) (2,1)]
        //[(0,0) (1,0) (2,0)]

        int column = 0;

        if (_right)
            column = 2;

        for (int i = 0; i < 3; i++)
        {
            mapChunks[2 - column, i].Clear();
        }

        //move the center up by one step
        currentStoragePosition = new Vector2Int(currentStoragePosition.x + (column - 1), currentStoragePosition.y);
        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();
        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = 0; i < 3; i++)
        {
            if (mapChunks[column, i] == null)
            {
                //mapChunks[column, i] = gameObject.AddComponent<MapChunk>();
                //mapChunks[column, i].InitChunk(TerrainData.GetRandomTerrainByFrequency(), StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX * (column - 1), 0, chunkTileCountZ * (i - 1)), TILES.Offset) + currentCenter, chunkTileCountX, chunkTileCountY, chunkTileCountZ, hexTilePrefab, this);
                terrainChunks[column, i] = new TerrainChunk(TerrainData.GetRandomTerrainByFrequency(),
                    StaticMaths.MultiplyVector3D(new Vector3(chunkTileCountX * (column - 1), 0, chunkTileCountZ * (i - 1)), TILES.Offset) + currentCenter,
                    planetData.GetTerrainSize(),
                    this);
                if (_right)
                    terrainChunkStorage.AddRight(currentStoragePosition.x, currentStoragePosition.y + (i - 1), terrainChunks[column, i]);
                    //mapChunkStorage.HardInsert(chunks[column, i], new Vector2Int(currentStoragePosition.x + 1, currentStoragePosition.y + (i - 1)));
                else
                    terrainChunkStorage.AddLeft(currentStoragePosition.x, currentStoragePosition.y + (i - 1), terrainChunks[column, i]);
                    //mapChunkStorage.HardInsert(chunks[column, i], new Vector2Int(currentStoragePosition.x - 1, currentStoragePosition.y + (i - 1)));
            }
        }

        //Reload the chunks after generating the missing ones
        ReloadChunkArrayFromStorage();

        for (int i = 0; i < 3; i++)
        {
            mapChunks[column, i].GenerateMap();
        }
    }

    public GameObject SetTile(Vector3 _position)
    {
        GameObject tile = Instantiate(hexTilePrefab, _position, Quaternion.identity);

        tile.transform.parent = transform;

        return tile;
    }

    public void DestroyGameObject(GameObject _object)
    {
        Destroy(_object);
    }



    private void ReloadChunkArrayFromStorage()
    {
        terrainChunks = terrainChunkStorage.Get2DArray(currentStoragePosition.x - 1, currentStoragePosition.y - 1, 3, 3);
    }

    public MapChunk[,] GetMapChunks()
    {
        return mapChunks;
    }

    public TerrainChunk[,] GetTerrainChunks()
    {
        return terrainChunks;
    }

    public float GetDiscreteHeightStep()
    {
        return discreteHeightStep;
    }

    public string GetSeed()
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
        Debug.DrawLine(cameraPos, terrainChunks[0, 0].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[1, 0].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[2, 0].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[0, 1].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[1, 1].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[2, 1].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[0, 2].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[1, 2].GetCenter());
        Debug.DrawLine(cameraPos, terrainChunks[2, 2].GetCenter());
    }

    private void DrawChunkBounds()
    {
        Vector3 c;
        Vector3 dimensions = planetData.GetTerrainSize();
        Vector3 scaling = TILES.Offset;
        Vector3[,,] corners = new Vector3[2, 2, 2];
        for (int x1 = 0; x1 < 3; x1++)
        {
            for (int z1 = 0; z1 < 3; z1++)
            {
                c = terrainChunks[x1, z1].GetCenter();
                //+++
                corners[1, 1, 1] = c + new Vector3(dimensions.x, dimensions.y, dimensions.z) / 2;
                //++-
                corners[1, 1, 0] = c + new Vector3(dimensions.x, dimensions.y, -dimensions.z) / 2;
                //+-+
                corners[1, 0, 1] = c + new Vector3(dimensions.x, -dimensions.y, dimensions.z) / 2;
                //+--
                corners[1, 0, 0] = c + new Vector3(dimensions.x, -dimensions.y, -dimensions.z) / 2;
                //-++
                corners[0, 1, 1] = c + new Vector3(-dimensions.x, dimensions.y, dimensions.z) / 2;
                //-+-
                corners[0, 1, 0] = c + new Vector3(-dimensions.x, dimensions.y, -dimensions.z) / 2;
                //--+
                corners[0, 0, 1] = c + new Vector3(-dimensions.x, -dimensions.y, dimensions.z) / 2;
                //---
                corners[0, 0, 0] = c + new Vector3(-dimensions.x, -dimensions.y, -dimensions.z) / 2;

                Debug.DrawLine(corners[0, 0, 0], corners[1, 0, 0]);
                Debug.DrawLine(corners[0, 0, 0], corners[0, 1, 0]);
                Debug.DrawLine(corners[0, 0, 0], corners[0, 0, 1]);

                Debug.DrawLine(corners[1, 0, 0], corners[1, 1, 0]);
                Debug.DrawLine(corners[1, 0, 0], corners[1, 0, 1]);

                Debug.DrawLine(corners[0, 0, 1], corners[0, 1, 1]);
                Debug.DrawLine(corners[0, 0, 1], corners[1, 0, 1]);

                Debug.DrawLine(corners[1, 0, 1], corners[1, 1, 1]);

                Debug.DrawLine(corners[0, 1, 0], corners[0, 1, 1]);
                Debug.DrawLine(corners[0, 1, 0], corners[1, 1, 0]);

                Debug.DrawLine(corners[0, 1, 1], corners[1, 1, 1]);

                Debug.DrawLine(corners[1, 1, 0], corners[1, 1, 1]);
            }
        }
    }
}

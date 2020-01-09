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
    string seed;

    int chunkSizeX = CHUNK.Size.x;
    int chunkSizeY = CHUNK.Size.y;
    int chunkSizeZ = CHUNK.Size.z;

    float discreteHeightStep = 1f; //-1 for continuous 

    Vector3 currentCenter; //world position of the currently visible central chunk
    Vector2Int currentStoragePosition; //where in the storage the current center chunk is positioned

    //[(0,2) (1,2) (2,2)]
    //[(0,1) (1,1) (2,1)]
    //[(0,0) (1,0) (2,0)]
    MapChunk[,] chunks; //currently loaded chunks

    MeshList<MapChunk> chunkStorage;

    void Start()
    {
        InstantiateMap();
    }

    void FixedUpdate()
    {
        UpdateMap();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    chunks[i, j].Clear();
                    Destroy(chunks[i, j]);
                }


            InstantiateMap();
        }

        //DrawLinesToChunkCenters();
        //DrawChunkBounds();
    }

    private void InstantiateMap()
    {
        seed = UTIL.GetMapSeed();

        ETerrain[,] terrain = new ETerrain[3, 3];

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                terrain[i, j] = TERRAIN.GetRandomTerrainByFrequency();

        //terrain[0, 0] = ETerrain.EMountains;
        //terrain[1, 0] = ETerrain.EMountains;
        //terrain[2, 0] = ETerrain.EMountains;

        InstantiateChunks(terrain);
    }

    private void InstantiateChunks(Meta_MapGenerator.ETerrain[,] _startTerrain)
    {
        chunks = new MapChunk[3, 3];
        chunkStorage = new MeshList<MapChunk>();

        chunks[0, 0] = gameObject.AddComponent<MapChunk>();
        chunks[0, 0].InitChunk(_startTerrain[0, 0], StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, -chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 0] = gameObject.AddComponent<MapChunk>();
        chunks[1, 0].InitChunk(_startTerrain[1, 0], StaticMaths.MultiplyVector3D(new Vector3(0, 0, -chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 0] = gameObject.AddComponent<MapChunk>();
        chunks[2, 0].InitChunk(_startTerrain[2, 0], StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, -chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        chunks[0, 1] = gameObject.AddComponent<MapChunk>();
        chunks[0, 1].InitChunk(_startTerrain[0, 1], StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, 0), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 1] = gameObject.AddComponent<MapChunk>();
        chunks[1, 1].InitChunk(_startTerrain[1, 1], StaticMaths.MultiplyVector3D(new Vector3(0, 0, 0), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 1] = gameObject.AddComponent<MapChunk>();
        chunks[2, 1].InitChunk(_startTerrain[2, 1], StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, 0), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        chunks[0, 2] = gameObject.AddComponent<MapChunk>();
        chunks[0, 2].InitChunk(_startTerrain[0, 2], StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 2] = gameObject.AddComponent<MapChunk>();
        chunks[1, 2].InitChunk(_startTerrain[1, 2], StaticMaths.MultiplyVector3D(new Vector3(0, 0, chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 2] = gameObject.AddComponent<MapChunk>();
        chunks[2, 2].InitChunk(_startTerrain[2, 2], StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, chunkSizeZ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        /*chunkStorage.HardInsert(chunks[0, 0], new Vector2Int(-1, -1));
        chunkStorage.HardInsert(chunks[1, 0], new Vector2Int(0, -1));
        chunkStorage.HardInsert(chunks[2, 0], new Vector2Int(1, -1));

        chunkStorage.HardInsert(chunks[0, 1], new Vector2Int(-1, 0));
        chunkStorage.HardInsert(chunks[1, 1], new Vector2Int(0, 0));
        chunkStorage.HardInsert(chunks[2, 1], new Vector2Int(1, 0));

        chunkStorage.HardInsert(chunks[0, 2], new Vector2Int(-1, 1));
        chunkStorage.HardInsert(chunks[1, 2], new Vector2Int(0, 1));
        chunkStorage.HardInsert(chunks[2, 2], new Vector2Int(1, 1));*/

        //middle row
        chunkStorage.SetCenter(chunks[1, 1]);
        chunkStorage.AddLeft(0, 0, chunks[0, 1]);
        chunkStorage.AddRight(0, 0, chunks[2, 1]);

        //top row
        chunkStorage.AddAbove(0, 0, chunks[1, 2]);
        chunkStorage.AddLeft(0, 1, chunks[0, 2]);
        chunkStorage.AddRight(0, 1, chunks[2, 2]);

        //bottom row
        chunkStorage.AddBelow(0, 0, chunks[1, 0]);
        chunkStorage.AddLeft(0, -1, chunks[0, 0]);
        chunkStorage.AddRight(0, -1, chunks[2, 0]);

        chunks[0, 0].GenerateMap();
        chunks[1, 0].GenerateMap();
        chunks[2, 0].GenerateMap();
        chunks[0, 1].GenerateMap();
        chunks[1, 1].GenerateMap();
        chunks[2, 1].GenerateMap();
        chunks[0, 2].GenerateMap();
        chunks[1, 2].GenerateMap();
        chunks[2, 2].GenerateMap();

        currentStoragePosition = new Vector2Int(0, 0);
    }

    private void UpdateMap()
    {
        Vector3 cameraPos = playerCamera.transform.position;
        //Vector2 playerChunkIndex = GetCorrespondingChunkIndex(cameraPos);
        UTIL.EPosition playerPosition = GetCameraPosition(cameraPos);

        //if (playerChunkIndex.x == 1 && playerChunkIndex.y == 1)
        //    return;

        RearrangeChunksAroundCenter(playerPosition);
    }

    private UTIL.EPosition GetCameraPosition(Vector3 _position)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);
        UTIL.EPosition pos = UTIL.EPosition.ECenter;


        if (StaticMaths.WithinBoundingBox(pos2D,
            StaticMaths.ThreeDTo2D(chunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ),
            new Vector2(chunkSizeX * TILES.Offset.x, chunkSizeZ * TILES.Offset.z) + StaticMaths.ThreeDTo2D(TILES.Offset, StaticMaths.EPlane.E_XZ) * 2))
        {
            pos = UTIL.EPosition.ECenter;
        }
        else
        {
            if (pos2D.y < chunks[1, 1].GetCenter().z - 5f)
            {
                pos = UTIL.EPosition.EAbove;
            }
            else if (pos2D.y > chunks[1, 1].GetCenter().z + 5f)
            {
                pos = UTIL.EPosition.EBelow;
            }
            else if (pos2D.x < chunks[1, 1].GetCenter().x - 5f)
            {
                pos = UTIL.EPosition.ELeft;
            }
            else if (pos2D.x > chunks[1, 1].GetCenter().x + 5f)
            {
                pos = UTIL.EPosition.ERight;
            }
        }
        //if (pos != UTIL.EPosition.ECenter)
            //Debug.Log(pos + " for camera=" + pos2D + " and center=" + chunks[1, 1].GetCenter());

        return pos;
    }

    private void RearrangeChunksAroundCenter(UTIL.EPosition _position)
    {
        if (_position == UTIL.EPosition.ECenter)
            return;

        if (_position == UTIL.EPosition.EAbove) //player went too far up
        {
            currentCenter = chunks[1, 0].GetCenter();
            //MoveChunksDown(true);
            MoveLoadedSectionDown();
        }
        else if (_position == UTIL.EPosition.EBelow) //player went too far down
        {
            currentCenter = chunks[1, 2].GetCenter();
            //MoveChunksUp(true);
            MoveLoadedSectionUp();
        }
        else if (_position == UTIL.EPosition.ELeft) //player went too far left
        {
            currentCenter = chunks[0, 1].GetCenter();
            //MoveChunksRight(true);
            
            MoveLoadedSectionLeft();
        }
        else if (_position == UTIL.EPosition.ERight) //player went too far right
        {
            currentCenter = chunks[2, 1].GetCenter();
            //MoveChunksLeft(true);
            MoveLoadedSectionRight();
        }
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
            chunks[i, 2 - row].Clear();
        }

        //move the center up by one step
        currentStoragePosition = new Vector2Int(currentStoragePosition.x, currentStoragePosition.y + (row - 1));

        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();


        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = 0; i < 3; i++)
        {
            if (chunks[i, row] == null)
            {
                //Debug.Log("trying to generate " + i + "," + row);
                chunks[i, row] = gameObject.AddComponent<MapChunk>();
                chunks[i, row].InitChunk(TERRAIN.GetRandomTerrainByFrequency(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX * (i - 1), 0, chunkSizeZ * (row - 1)), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
                if(_up)
                    chunkStorage.AddAbove(currentStoragePosition.x + (i - 1), currentStoragePosition.y, chunks[i, row]);
                    //chunkStorage.HardInsert(chunks[i, row], new Vector2Int(currentStoragePosition.x + (i - 1), currentStoragePosition.y + 1));
                else
                    chunkStorage.AddAbelow(currentStoragePosition.x + (i - 1), currentStoragePosition.y, chunks[i, row]);
                    //chunkStorage.HardInsert(chunks[i, row], new Vector2Int(currentStoragePosition.x + (i - 1), currentStoragePosition.y - 1));
            }
        }
        for (int i = 0; i < 3; i++)
        {
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            //Debug.Log("generating map for " + i + "," + row);
            //Debug.Log("chunks at that position = " + chunks[i, row]);
            chunks[i, row].GenerateMap();
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
            chunks[2 - column, i].Clear();
        }

        //move the center up by one step
        currentStoragePosition = new Vector2Int(currentStoragePosition.x + (column - 1), currentStoragePosition.y);
        //try to load the currently stored chunks
        ReloadChunkArrayFromStorage();
        //check if any of the loaded chunks is still null and if so, generate them
        for (int i = 0; i < 3; i++)
        {
            if (chunks[column, i] == null)
            {
                chunks[column, i] = gameObject.AddComponent<MapChunk>();
                chunks[column, i].InitChunk(TERRAIN.GetRandomTerrainByFrequency(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX * (column - 1), 0, chunkSizeZ * (i - 1)), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
                if(_right)
                    chunkStorage.AddRight(currentStoragePosition.x, currentStoragePosition.y + (i - 1), chunks[column, i]);
                    //chunkStorage.HardInsert(chunks[column, i], new Vector2Int(currentStoragePosition.x + 1, currentStoragePosition.y + (i - 1)));
                else
                    chunkStorage.AddLeft(currentStoragePosition.x, currentStoragePosition.y + (i - 1), chunks[column, i]);
                    //chunkStorage.HardInsert(chunks[column, i], new Vector2Int(currentStoragePosition.x - 1, currentStoragePosition.y + (i - 1)));
            }
        }
        for (int i = 0; i < 3; i++)
        {
            chunks[column, i].GenerateMap();
        }
    }



    private void ReloadChunkArrayFromStorage()
    {
        chunks = chunkStorage.Get2DArray(new Vector2Int(currentStoragePosition.x - 1, currentStoragePosition.y - 1), new Vector2Int(3, 3));
    }

    public MapChunk[,] GetChunks()
    {
        return chunks;
    }

    public float GetDiscreteHeightStep()
    {
        return discreteHeightStep;
    }

    public string GetSeed()
    {
        return seed;
    }
}

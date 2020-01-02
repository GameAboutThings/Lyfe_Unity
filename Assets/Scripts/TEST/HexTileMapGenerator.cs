﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class HexTileMapGenerator : MonoBehaviour
{


    [SerializeField]
    private GameObject hexTilePrefab;

    [SerializeField]
    GameObject playerCamera;


    int chunkSizeX = CHUNK.Size.x;
    int chunkSizeY = CHUNK.Size.y;
    int chunkSizeZ = CHUNK.Size.z;

    Vector3 currentCenter;

    //[(0,0) (1,0) (2,0)]
    //[(0,1) (1,1) (2,1)]
    //[(0,2) (1,2) (2,2)]
    MapChunk[,] chunks;

    List<MapChunk> chunkStore;

    void Start()
    {
        InstantiateMap();
    }

    void FixedUpdate()
    {
        UpdateMap();
        DrawLinesToChunkCenters();
    }

    private void InstantiateMap()
    {
        InstantiateChunks();
    }

    private void InstantiateChunks()
    {
        //[(0,0) (1,0) (2,0)]
        //[(0,1) (1,1) (2,1)]
        //[(0,2) (1,2) (2,2)]
        chunks = new MapChunk[3, 3];
        chunks[0, 0] = gameObject.AddComponent<MapChunk>();
        chunks[0, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX  , 0, -chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 0] = gameObject.AddComponent<MapChunk>();
        chunks[1, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(0            , 0, -chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 0] = gameObject.AddComponent<MapChunk>();
        chunks[2, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX   , 0, -chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        chunks[0, 1] = gameObject.AddComponent<MapChunk>();
        chunks[0, 1].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX  , 0, 0          ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 1] = gameObject.AddComponent<MapChunk>();
        chunks[1, 1].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(0            , 0, 0          ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 1] = gameObject.AddComponent<MapChunk>();
        chunks[2, 1].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX   , 0, 0          ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        chunks[0, 2] = gameObject.AddComponent<MapChunk>();
        chunks[0, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX  , 0, chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[1, 2] = gameObject.AddComponent<MapChunk>();
        chunks[1, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(0            , 0, chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);
        chunks[2, 2] = gameObject.AddComponent<MapChunk>();
        chunks[2, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX   , 0, chunkSizeZ ), TILES.Offset), chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

        chunks[0, 0].SetNeighbourChunk(chunks[1, 0], UTIL.EPosition.ERight);
        chunks[0, 0].SetNeighbourChunk(chunks[0, 1], UTIL.EPosition.EBelow);

        chunks[1, 0].SetNeighbourChunk(chunks[2, 0], UTIL.EPosition.ERight);
        chunks[1, 0].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.EBelow);

        chunks[2, 0].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.EBelow);

        chunks[0, 1].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.ERight);
        chunks[0, 1].SetNeighbourChunk(chunks[0, 2], UTIL.EPosition.EBelow);

        chunks[1, 1].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.ERight);
        chunks[1, 1].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.EBelow);

        chunks[2, 1].SetNeighbourChunk(chunks[2, 2], UTIL.EPosition.EBelow);

        chunks[0, 2].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.ERight);

        chunks[1, 2].SetNeighbourChunk(chunks[2, 2], UTIL.EPosition.ERight);

        chunkStore = new List<MapChunk>();
        chunkStore.Add(chunks[0, 0]);
        chunkStore.Add(chunks[1, 0]);
        chunkStore.Add(chunks[2, 0]);
        chunkStore.Add(chunks[0, 1]);
        chunkStore.Add(chunks[1, 1]);
        chunkStore.Add(chunks[2, 1]);
        chunkStore.Add(chunks[0, 2]);
        chunkStore.Add(chunks[1, 2]);
        chunkStore.Add(chunks[2, 2]);

        chunks[0, 0].GenerateMap();
        chunks[1, 0].GenerateMap();
        chunks[2, 0].GenerateMap();
        chunks[0, 1].GenerateMap();
        chunks[1, 1].GenerateMap();
        chunks[2, 1].GenerateMap();
        chunks[0, 2].GenerateMap();
        chunks[1, 2].GenerateMap();
        chunks[2, 2].GenerateMap();
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

    //private Vector2 GetCorrespondingChunkIndex(Vector3 _position)
    //{
    //    Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);

    //    for (int x = 0; x < 3; x++)
    //    {
    //        for (int z = 0; z < 3; z++)
    //        {
    //            if (StaticMaths.WithinBoundingBox(pos2D,
    //                StaticMaths.ThreeDTo2D(chunks[x, z].GetCenter(), StaticMaths.EPlane.E_XZ),
    //                new Vector2(chunkSizeX, chunkSizeZ)))
    //            {
    //                return new Vector2(x, z);
    //            }
    //            else if (false)
    //            {
    //                return new Vector2(x, z);
    //            }
    //        }
    //    }
    //    return new Vector2(1, 1);
    //}

    private UTIL.EPosition GetCameraPosition(Vector3 _position)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_position, StaticMaths.EPlane.E_XZ);
        UTIL.EPosition pos = UTIL.EPosition.ECenter;


        if (StaticMaths.WithinBoundingBox(pos2D,
            StaticMaths.ThreeDTo2D(chunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ),
            new Vector2(chunkSizeX * TILES.Offset.x, chunkSizeZ * TILES.Offset.z)))
        {
            pos = UTIL.EPosition.ECenter;
        }
        else
        {
            if (pos2D.y < chunks[1, 1].GetCenter().z - 5f)
            {
                pos =  UTIL.EPosition.EAbove;
            }
            else if (pos2D.y > chunks[1, 1].GetCenter().z + 5f)
            {
                pos =  UTIL.EPosition.EBelow;
            }
            else if (pos2D.x < chunks[1, 1].GetCenter().x - 5f)
            {
                pos =  UTIL.EPosition.ELeft;
            }
            else if (pos2D.x > chunks[1, 1].GetCenter().x + 5f)
            {
                pos =  UTIL.EPosition.ERight;
            }
        }
        if(pos != UTIL.EPosition.ECenter)
            Debug.Log(pos + " for camera=" + pos2D + " and center=" + chunks[1, 1].GetCenter());

        return pos;
    }

    private void RearrangeChunksAroundCenter(UTIL.EPosition _position)
    {
        if (_position == UTIL.EPosition.ECenter)
            return;

        if (_position == UTIL.EPosition.EAbove) //player went too far up
        {
            currentCenter = chunks[1, 0].GetCenter();
            MoveChunksDown(true); 
        }
        else if (_position == UTIL.EPosition.EBelow) //player went too far down
        {
            currentCenter = chunks[1, 2].GetCenter();
            MoveChunksUp(true);
        }
        else if (_position == UTIL.EPosition.ELeft) //player went too far left
        {
            currentCenter = chunks[0, 1].GetCenter();
            MoveChunksRight(true);
        }
        else if (_position == UTIL.EPosition.ERight) //player went too far right
        {
            currentCenter = chunks[2, 1].GetCenter();
            MoveChunksLeft(true);
        }
    }

    private void MoveChunksRight(bool _generateTiles)
    {
        //[(0,0) (1,0) (2,0)]
        //[(0,1) (1,1) (2,1)]
        //[(0,2) (1,2) (2,2)]

        //clearing left row
        chunks[2, 0].Clear();
        chunks[2, 1].Clear();
        chunks[2, 2].Clear();

        chunks[2, 0] = chunks[1, 0];
        chunks[1, 0] = chunks[0, 0];

        chunks[2, 1] = chunks[1, 1];
        chunks[1, 1] = chunks[0, 1];

        chunks[2, 2] = chunks[1, 2];
        chunks[1, 2] = chunks[0, 2];

        chunks[0, 0] = chunks[1, 0].GetNeighbourChunk(UTIL.EPosition.ELeft);
        chunks[0, 1] = chunks[1, 1].GetNeighbourChunk(UTIL.EPosition.ELeft);
        chunks[0, 2] = chunks[1, 2].GetNeighbourChunk(UTIL.EPosition.ELeft);

        if (chunks[0, 0] != null && chunks[0, 1] == null)
            chunks[0, 1] = chunks[0, 0].GetNeighbourChunk(UTIL.EPosition.EBelow);

        if (chunks[0, 1] != null && chunks[0, 0] == null)
            chunks[0, 0] = chunks[0, 1].GetNeighbourChunk(UTIL.EPosition.EAbove);

        if (chunks[0, 1] != null && chunks[0, 2] == null)
            chunks[0, 2] = chunks[0, 1].GetNeighbourChunk(UTIL.EPosition.EBelow);

        if (chunks[0, 2] != null && chunks[0, 1] == null)
            chunks[0, 1] = chunks[0, 2].GetNeighbourChunk(UTIL.EPosition.EAbove);

        if (chunks[0, 0] == null)
        {
            chunks[0, 0] = gameObject.AddComponent<MapChunk>();
            chunks[0, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, -chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[0, 1] != null)
                chunks[0, 0].SetNeighbourChunk(chunks[0, 1], UTIL.EPosition.EBelow);
            if (chunks[1, 0] != null)
                chunks[0, 0].SetNeighbourChunk(chunks[1, 0], UTIL.EPosition.ERight);

            chunkStore.Add(chunks[0, 0]);
        }

        if (chunks[0, 1] == null)
        {
            chunks[0, 1] = gameObject.AddComponent<MapChunk>();
            chunks[0, 1].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, 0), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[0, 0] != null)
                chunks[0, 1].SetNeighbourChunk(chunks[0, 0], UTIL.EPosition.EAbove);
            if (chunks[0, 2] != null)
                chunks[0, 1].SetNeighbourChunk(chunks[0, 2], UTIL.EPosition.EBelow);
            if (chunks[1, 1] != null)
                chunks[0, 1].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.ERight);

            chunkStore.Add(chunks[0, 1]);
        }

        if (chunks[0, 2] == null)
        {
            chunks[0, 2] = gameObject.AddComponent<MapChunk>();
            chunks[0, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 2] != null)
                chunks[0, 2].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.ERight);
            if (chunks[0, 1] != null)
                chunks[0, 2].SetNeighbourChunk(chunks[0, 1], UTIL.EPosition.EAbove);

            chunkStore.Add(chunks[0, 2]);
        }

        if (!_generateTiles)
            return;

        chunks[0, 0].GenerateMap();
        chunks[0, 1].GenerateMap();
        chunks[0, 2].GenerateMap();
    }

    private void MoveChunksLeft(bool _generateTiles)
    {
        //[(0,0) (1,0) (2,0)]
        //[(0,1) (1,1) (2,1)]
        //[(0,2) (1,2) (2,2)]

        //clearing left row
        chunks[0, 0].Clear();
        chunks[0, 1].Clear();
        chunks[0, 2].Clear();

        //moving chunk references left one step in the array
        chunks[0, 0] = chunks[1, 0];
        chunks[1, 0] = chunks[2, 0];

        chunks[0, 1] = chunks[1, 1];
        chunks[1, 1] = chunks[2, 1];

        chunks[0, 2] = chunks[1, 2];
        chunks[1, 2] = chunks[2, 2];

        chunks[2, 0] = chunks[1, 0].GetNeighbourChunk(UTIL.EPosition.ERight);
        chunks[2, 1] = chunks[1, 1].GetNeighbourChunk(UTIL.EPosition.ERight);
        chunks[2, 2] = chunks[1, 2].GetNeighbourChunk(UTIL.EPosition.ERight);

        if (chunks[2, 0] != null && chunks[2, 1] == null)
            chunks[2, 1] = chunks[2, 0].GetNeighbourChunk(UTIL.EPosition.EBelow);

        if (chunks[2, 1] != null && chunks[2, 0] == null)
            chunks[2, 0] = chunks[2, 1].GetNeighbourChunk(UTIL.EPosition.EAbove);

        if (chunks[2, 1] != null && chunks[2, 2] == null)
            chunks[2, 2] = chunks[2, 1].GetNeighbourChunk(UTIL.EPosition.EBelow);

        if (chunks[2, 2] != null && chunks[2, 1] == null)
            chunks[2, 1] = chunks[2, 2].GetNeighbourChunk(UTIL.EPosition.EAbove);

        if (chunks[2, 0] == null)
        {
            chunks[2, 0] = gameObject.AddComponent<MapChunk>();
            chunks[2, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, -chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[2, 1] != null)
                chunks[2, 0].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.EBelow);
            if (chunks[1, 0] != null)
                chunks[2, 0].SetNeighbourChunk(chunks[1, 0], UTIL.EPosition.ELeft);

            chunkStore.Add(chunks[2, 0]);
        }

        if (chunks[2, 1] == null)
        {
            chunks[2, 1] = gameObject.AddComponent<MapChunk>();
            chunks[2, 1].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, 0), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[2, 0] != null)
                chunks[2, 1].SetNeighbourChunk(chunks[2, 0], UTIL.EPosition.EAbove);
            if (chunks[2, 2] != null)
                chunks[2, 1].SetNeighbourChunk(chunks[2, 2], UTIL.EPosition.EBelow);
            if (chunks[1, 1] != null)
                chunks[2, 1].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.ELeft);

            chunkStore.Add(chunks[2, 1]);
        }

        if (chunks[2, 2] == null)
        {
            chunks[2, 2] = gameObject.AddComponent<MapChunk>();
            chunks[2, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 2] != null)
                chunks[2, 2].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.ELeft);
            if (chunks[2, 1] != null)
                chunks[2, 2].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.EAbove);

            chunkStore.Add(chunks[2, 2]);
        }

        if (!_generateTiles)
            return;

        chunks[2, 0].GenerateMap();
        chunks[2, 1].GenerateMap();
        chunks[2, 2].GenerateMap();
    }

    private void MoveChunksUp(bool _generateTiles)
    {
        //[(0,0) (1,0) (2,0)]
        //[(0,1) (1,1) (2,1)]
        //[(0,2) (1,2) (2,2)]

        //clearing lower upper
        chunks[0, 0].Clear();
        chunks[1, 0].Clear();
        chunks[2, 0].Clear();

        //moving chunk references down one step in the array
        chunks[0, 0] = chunks[0, 1];
        chunks[0, 1] = chunks[0, 2];

        chunks[1, 0] = chunks[1, 1];
        chunks[1, 1] = chunks[1, 2];

        chunks[2, 0] = chunks[2, 1];
        chunks[2, 1] = chunks[2, 2];

        //fill empty spots
        chunks[0, 2] = chunks[0, 1].GetNeighbourChunk(UTIL.EPosition.EBelow);
        chunks[1, 2] = chunks[1, 1].GetNeighbourChunk(UTIL.EPosition.EBelow);
        chunks[2, 2] = chunks[2, 1].GetNeighbourChunk(UTIL.EPosition.EBelow);

        if (chunks[0, 2] != null && chunks[1, 2] == null)
            chunks[1, 2] = chunks[0, 2].GetNeighbourChunk(UTIL.EPosition.ERight);

        if (chunks[1, 2] != null && chunks[0, 2] == null)
            chunks[0, 2] = chunks[1, 2].GetNeighbourChunk(UTIL.EPosition.ELeft);

        if (chunks[1, 2] != null && chunks[2, 2] == null)
            chunks[2, 2] = chunks[1, 2].GetNeighbourChunk(UTIL.EPosition.ERight);

        if (chunks[2, 2] != null && chunks[1, 2] == null)
            chunks[1, 2] = chunks[2, 2].GetNeighbourChunk(UTIL.EPosition.ELeft);

        if (chunks[0, 2] == null)
        {
            chunks[0, 2] = gameObject.AddComponent<MapChunk>();
            chunks[0, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 2] != null)
                chunks[0, 2].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.ERight);
            if (chunks[0, 1] != null)
                chunks[0, 2].SetNeighbourChunk(chunks[0, 1], UTIL.EPosition.EAbove);

            chunkStore.Add(chunks[0, 2]);
        }
        

        if (chunks[1, 2] == null)
        {
            chunks[1, 2] = gameObject.AddComponent<MapChunk>();
            chunks[1, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(0, 0, chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[0, 2] != null)
                chunks[1, 2].SetNeighbourChunk(chunks[0, 2], UTIL.EPosition.ELeft);
            if (chunks[2, 2] != null)
                chunks[1, 2].SetNeighbourChunk(chunks[2, 2], UTIL.EPosition.ERight);
            if (chunks[1, 1] != null)
                chunks[1, 2].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.EAbove);

            chunkStore.Add(chunks[1, 2]);
        }
        

        if (chunks[2, 2] == null)
        {
            chunks[2, 2] = gameObject.AddComponent<MapChunk>();
            chunks[2, 2].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 2] != null)
                chunks[2, 2].SetNeighbourChunk(chunks[1, 2], UTIL.EPosition.ELeft);
            if (chunks[2, 1] != null)
                chunks[2, 2].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.EAbove);

            chunkStore.Add(chunks[2, 2]);
        }

        if (!_generateTiles)
            return;

        chunks[0, 2].GenerateMap();
        chunks[1, 2].GenerateMap();
        chunks[2, 2].GenerateMap();
    }

    private void MoveChunksDown(bool _generateTiles)
    {
        //[(0,0) (1,0) (2,0)]
        //[(0,1) (1,1) (2,1)]
        //[(0,2) (1,2) (2,2)]

        //clearing lower row
        chunks[0, 2].Clear();
        chunks[1, 2].Clear();
        chunks[2, 2].Clear();

        //moving chunk references down one step in the array
        chunks[0, 2] = chunks[0, 1];
        chunks[0, 1] = chunks[0, 0];

        chunks[1, 2] = chunks[1, 1];
        chunks[1, 1] = chunks[1, 0];

        chunks[2, 2] = chunks[2, 1];
        chunks[2, 1] = chunks[2, 0];

        //fill empty spots
        chunks[0, 0] = chunks[0, 1].GetNeighbourChunk(UTIL.EPosition.EAbove);
        chunks[1, 0] = chunks[1, 1].GetNeighbourChunk(UTIL.EPosition.EAbove);
        chunks[2, 0] = chunks[2, 1].GetNeighbourChunk(UTIL.EPosition.EAbove);

        if (chunks[0, 0] != null && chunks[1, 0] == null)
            chunks[1, 0] = chunks[0, 0].GetNeighbourChunk(UTIL.EPosition.ERight);

        if (chunks[1, 0] != null && chunks[0, 0] == null)
            chunks[0, 0] = chunks[1, 0].GetNeighbourChunk(UTIL.EPosition.ELeft);

        if (chunks[1, 0] != null && chunks[2, 0] == null)
            chunks[2, 0] = chunks[1, 0].GetNeighbourChunk(UTIL.EPosition.ERight);

        if (chunks[2, 0] != null && chunks[1, 0] == null)
            chunks[1, 0] = chunks[2, 0].GetNeighbourChunk(UTIL.EPosition.ELeft);

        if (chunks[0, 0] == null)
        {
            chunks[0, 0] = gameObject.AddComponent<MapChunk>();
            chunks[0, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(-chunkSizeX, 0, -chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 0] != null)
                chunks[0, 0].SetNeighbourChunk(chunks[1, 0], UTIL.EPosition.ERight);
            if (chunks[0, 1] != null)
                chunks[0, 0].SetNeighbourChunk(chunks[0, 1], UTIL.EPosition.EBelow);

            chunkStore.Add(chunks[0, 0]);
        }
        

        if (chunks[1, 0] == null)
        {
            chunks[1, 0] = gameObject.AddComponent<MapChunk>();
            chunks[1, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(0, 0, -chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[0, 0] != null)
                chunks[1, 0].SetNeighbourChunk(chunks[0, 0], UTIL.EPosition.ELeft);
            if (chunks[2, 0] != null)
                chunks[1, 0].SetNeighbourChunk(chunks[2, 0], UTIL.EPosition.ERight);
            if (chunks[1, 1] != null)
                chunks[1, 0].SetNeighbourChunk(chunks[1, 1], UTIL.EPosition.EBelow);

            chunkStore.Add(chunks[1, 0]);
        }
        
        

        if (chunks[2, 0] == null)
        {
            chunks[2, 0] = gameObject.AddComponent<MapChunk>();
            chunks[2, 0].InitChunk(BIOME.GetRandomBiome(), StaticMaths.MultiplyVector3D(new Vector3(chunkSizeX, 0, -chunkSizeZ), TILES.Offset) + currentCenter, chunkSizeX, chunkSizeY, chunkSizeZ, hexTilePrefab, this);

            if (chunks[1, 0] != null)
                chunks[2, 0].SetNeighbourChunk(chunks[1, 0], UTIL.EPosition.ELeft);
            if (chunks[2, 1] != null)
                chunks[2, 0].SetNeighbourChunk(chunks[2, 1], UTIL.EPosition.EBelow);

            chunkStore.Add(chunks[2, 0]);
        }

        if (!_generateTiles)
            return;

        chunks[0, 0].GenerateMap();
        chunks[1, 0].GenerateMap();
        chunks[2, 0].GenerateMap();
    }

    private void DrawLinesToChunkCenters()
    {
        Vector3 cameraPos = playerCamera.transform.position;
        Debug.DrawLine(cameraPos, chunks[0, 0].GetCenter());
        Debug.DrawLine(cameraPos, chunks[1, 0].GetCenter());
        Debug.DrawLine(cameraPos, chunks[2, 0].GetCenter());
        Debug.DrawLine(cameraPos, chunks[0, 1].GetCenter());
        Debug.DrawLine(cameraPos, chunks[1, 1].GetCenter());
        Debug.DrawLine(cameraPos, chunks[2, 1].GetCenter());
        Debug.DrawLine(cameraPos, chunks[0, 2].GetCenter());
        Debug.DrawLine(cameraPos, chunks[1, 2].GetCenter());
        Debug.DrawLine(cameraPos, chunks[2, 2].GetCenter());
    }

    public MapChunk[,] GetChunks()
    {
        return chunks;
    }
}

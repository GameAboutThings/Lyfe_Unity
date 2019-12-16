using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    const int threadGroupSize = 8;

    private Transform transform;
    private bool editor;

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    ComputeBuffer indexBuffer;
    ComputeBuffer vertexBuffer;

    ComputeShader cs_MarchingCubes;
    ComputeShader cs_UniqueTris;

    Mesh mesh;
    Vector3 resolution;

    public MeshGenerator(Mesh _mesh, Vector3 _resolution, Transform _transform, bool _editor)
    {
        this.mesh = _mesh;
        this.resolution = _resolution;
        this.transform = _transform;
        this.editor = _editor;
        CreateBuffers();
    }

    public void GenerateMesh(float[] _charges)
    {

        if (cs_MarchingCubes == null)
        {
            cs_MarchingCubes = Resources.Load<ComputeShader>("Shaders/cs_MarchingCubes");
        }

        int numThreadsPerX = Mathf.CeilToInt(resolution.x / (float)threadGroupSize);
        int numThreadsPerY = Mathf.CeilToInt(resolution.y / (float)threadGroupSize);
        int numThreadsPerZ = Mathf.CeilToInt(resolution.z / (float)threadGroupSize);
        float isoLevel = Meta_CellEditor.SCULPTING.MARCHING_CUBES.ISOLEVEL;

        int kernel = cs_MarchingCubes.FindKernel("CSMain");

        triangleBuffer.SetCounterValue(0);
        pointsBuffer.SetData(_charges);
        cs_MarchingCubes.SetBuffer(kernel, "points", pointsBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "triangles", triangleBuffer);
        cs_MarchingCubes.SetInt("numPointsPerX", (int)resolution.x);
        cs_MarchingCubes.SetInt("numPointsPerY", (int)resolution.y);
        cs_MarchingCubes.SetInt("numPointsPerZ", (int)resolution.z);
        cs_MarchingCubes.SetFloat("scale", Meta_CellEditor.SCULPTING.GRID.SCALE);
        cs_MarchingCubes.SetVector("center", transform.position);
        cs_MarchingCubes.SetFloat("isoLevel", isoLevel);

        cs_MarchingCubes.Dispatch(kernel, numThreadsPerX, numThreadsPerY, numThreadsPerZ);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        mesh.Clear();
        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        if (editor)
        {
            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j];
                }
            }
        }
        else
        {
            for (int i = 0; i < numTris; i++)
            {
                for (int j = 2; j >= 0; j--)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][2 - j];
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }

    void CreateBuffers()
    {
        int numPoints = (int)(resolution.x * resolution.y * resolution.z);
        int numVoxelsPerX = (int)resolution.x - 1;
        int numVoxelsPerY = (int)resolution.y - 1;
        int numVoxelsPerZ = (int)resolution.z - 1;
        int numVoxels = numVoxelsPerX * numVoxelsPerY * numVoxelsPerZ;
        int maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || (pointsBuffer == null || numPoints != pointsBuffer.count))
        {
            if (Application.isPlaying)
            {
                ReleaseBuffers();
            }
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof(float));
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }
    }

    void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            pointsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    Vector3 GridToLocalPos(Vector3 _pos)
    {
        _pos -= new Vector3(Meta_CellEditor.SCULPTING.GRID.DIMENSION.X,
            Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y,
            Meta_CellEditor.SCULPTING.GRID.DIMENSION.Z) / 2;

        _pos *= Meta_CellEditor.SCULPTING.GRID.SCALE;

        return _pos + transform.position;
    }
}

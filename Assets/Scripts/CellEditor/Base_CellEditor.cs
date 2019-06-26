using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Meta_CellEditor.SCULPTING.NODES;
using System.Runtime;

public class Base_CellEditor : MonoBehaviour
{
    private Node_CellEditor baseNode;
    private ComputeShader cs_Charges;
    private ComputeShader cs_MarchingCubes;
    //remove again. Is just for debugging
    private List<GameObject> cubes;
    private GameObject testCube;
    
    void Start()
    {
        Init();

        InstantiateNew();

        cubes = new List<GameObject>();
    }
    
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A) && Designer_CellEditor.GetEditorInputEnabled())
        //{
            foreach (GameObject g in cubes)
            {
                Destroy(g);
            }
            cubes.Clear();

            int x = Meta_CellEditor.SCULPTING.GRID.DIMENSION.X;
            int y = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y;
            int z = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Z;
            float[] charges = CalculateCharges(new Vector3Int(x, y, z));
            GenerateMeshWithMarchingCubes(charges, new Vector3Int(x, y, z));

            z--;
            y--;
            x--;
            for (; z >= 0; z--)
            {
                for (; y >= 0; y--)
                {
                    for (; x >= 0; x--)
                    {
                        int index = GetGridIndex(new Vector3(x, y, z));

                        if (charges[index] >= 0.8f)
                        {
                            if(GetNumberAdjacentCubes(new Vector3(x, y, z), 0.8f, charges) <= 13)
                                cubes.Add(GameObject.Instantiate(testCube, GridToLocalPos(new Vector3(x, y, z)), Quaternion.identity));
                        }
                    }
                    x = Meta_CellEditor.SCULPTING.GRID.DIMENSION.X - 1;
                }
                y = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y - 1;
            }
        //}
            
    }

    int GetGridIndex(Vector3 _pos)
    {
        int x = (int)_pos.x;
        int y = (int)_pos.y;
        int z = (int)_pos.z;
        return z * (Meta_CellEditor.SCULPTING.GRID.DIMENSION.X * Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y)
            + y * Meta_CellEditor.SCULPTING.GRID.DIMENSION.X
            + x;
    }

    int GetNumberAdjacentCubes(Vector3 _pos, float threshold, float[] charges)
    {
        int num = 0;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y, _pos.z + 1))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y + 1, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y + 1, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y + 1, _pos.z + 1))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y - 1, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y - 1, _pos.z + 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y - 1, _pos.z + 1))] >= threshold)
            num++;


        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y, _pos.z))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y, _pos.z))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y + 1, _pos.z))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y + 1, _pos.z))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y + 1, _pos.z))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y - 1, _pos.z))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y - 1, _pos.z))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y - 1, _pos.z))] >= threshold)
            num++;


        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y, _pos.z - 1))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y + 1, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y + 1, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y + 1, _pos.z - 1))] >= threshold)
            num++;

        if (charges[GetGridIndex(new Vector3(_pos.x, _pos.y - 1, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x + 1, _pos.y - 1, _pos.z - 1))] >= threshold)
            num++;
        if (charges[GetGridIndex(new Vector3(_pos.x - 1, _pos.y - 1, _pos.z - 1))] >= threshold)
            num++;

        return num;
    }

    Vector3 GridToLocalPos(Vector3 _pos)
    {
        _pos -= new Vector3(Meta_CellEditor.SCULPTING.GRID.DIMENSION.X,
            Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y,
            Meta_CellEditor.SCULPTING.GRID.DIMENSION.Z) / 2;

        _pos *= Meta_CellEditor.SCULPTING.GRID.SCALE;

        return _pos + transform.position;
    }

    private void Init()
    {
        cs_Charges = Resources.Load<ComputeShader>("Shaders/cs_Charges");
        cs_MarchingCubes = Resources.Load<ComputeShader>("Shaders/cs_marchingCubes");
        testCube = Resources.Load<GameObject>("Prefab/TEST/test_cube");
    }

    private void InstantiateNew()
    {
        GameObject baseNodeObject = Instantiate(NODE_TEMPLATE, Vector3.zero, Quaternion.identity);
        baseNode = baseNodeObject.GetComponent<Node_CellEditor>();
        baseNodeObject.transform.parent = transform;
        baseNode.PostConstructor(ENodeType.EBase, ENodePosition.EBase, null);
    }

    private void InstantiateFromFile(string _blueprint)
    {

    }

    private float[] CalculateCharges(Vector3Int _resolution)
    {
        float[] charges = new float[_resolution.z * _resolution.y * _resolution.x];

        List<SNode> nodes = baseNode.GetAllChildNodeMetaData(new List<SNode>());

        int kernel = cs_Charges.FindKernel("CSMain");
        ComputeBuffer nodeBuffer = new ComputeBuffer(nodes.Count, SNODE_SIZE);
        nodeBuffer.SetData(nodes.ToArray());

        ComputeBuffer chargeBuffer = new ComputeBuffer(charges.Length, sizeof(float));
        chargeBuffer.SetData(charges);

        cs_Charges.SetBuffer(kernel, "nodes", nodeBuffer);
        cs_Charges.SetBuffer(kernel, "charges", chargeBuffer);
        cs_Charges.SetVector("EDITOR_GRID_DIMENSION", new Vector4(_resolution.x, _resolution.y, _resolution.z, Meta_CellEditor.SCULPTING.GRID.SCALE));
        cs_Charges.SetVector("basePos", transform.position);
        cs_Charges.SetInt("numNodes", nodes.Count);

        cs_Charges.Dispatch(kernel, _resolution.x / 8, _resolution.y / 8, _resolution.z / 8);

        chargeBuffer.GetData(charges);


        chargeBuffer.Dispose();
        nodeBuffer.Dispose();


        return charges;
    }

    private void GenerateMeshWithMarchingCubes(float[] _charges, Vector3 _resolution)
    {
        int kernel = cs_MarchingCubes.FindKernel("CSMain");

        ComputeBuffer chargeBuffer = new ComputeBuffer(_charges.Length, sizeof(float));
        chargeBuffer.SetData(_charges);

        int cubeNum = ((int)_resolution.x - 1) * ((int)_resolution.y - 1) * ((int)_resolution.z - 1);
        ComputeBuffer cubeBuffer = new ComputeBuffer(cubeNum, Meta_CellEditor.SCULPTING.MARCHING_CUBES.SCUBE_MC_SIZE);
        //cubeBuffer.SetData() //no data set. No prior information so this should not be needed

        ComputeBuffer cNodeBuffer = new ComputeBuffer(_charges.Length, Meta_CellEditor.SCULPTING.MARCHING_CUBES.SNODE_SIZE);
        //cubeBuffer.SetData() //no data set. No prior information so this should not be needed


        //8 -> 12
        //12 -> 20
        //16 -> 28
        //18 -> 32
        //20 -> 36
        int edgeNum = 12 + (_charges.Length - 8) * 2;
        ComputeBuffer edgeBuffer = new ComputeBuffer(edgeNum, Meta_CellEditor.SCULPTING.MARCHING_CUBES.SEDGE_SIZE);
        //cubeBuffer.SetData() //no data set. No prior information so this should not be needed

        ComputeBuffer vertexIndexPointer = new ComputeBuffer(2, sizeof(int));
        vertexIndexPointer.SetData(new int[]{ 0, 0});

        ComputeBuffer vertexBuffer = new ComputeBuffer(edgeNum, sizeof(float) * 3);
        //vertexIndexPointer.SetData(new int[] { 0, 0 });

        ComputeBuffer indexBuffer = new ComputeBuffer(edgeNum * 3 * 100, sizeof(int));
        //vertexIndexPointer.SetData(new int[] { 0, 0 });

        cs_MarchingCubes.SetBuffer(kernel, "charges", chargeBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "cubes", cubeBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "controlNodes", cNodeBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "edges", edgeBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "vertexIndexPointer", vertexIndexPointer);
        cs_MarchingCubes.SetBuffer(kernel, "vertexBuffer", vertexBuffer);
        cs_MarchingCubes.SetBuffer(kernel, "indexBuffer", indexBuffer);

        cs_MarchingCubes.SetInt("numCubes", cubeNum);

        cs_MarchingCubes.SetFloat("threshold", Meta_CellEditor.SCULPTING.METABALLS.THRESHOLD);
        cs_MarchingCubes.SetFloat("cubeSize", Meta_CellEditor.SCULPTING.GRID.SCALE);

        cs_MarchingCubes.SetVector("EDITOR_GRID_DIMENSION", new Vector4(_resolution.x, _resolution.y, _resolution.z, Meta_CellEditor.SCULPTING.GRID.SCALE));
        cs_MarchingCubes.SetVector("basePos", transform.position);

        cs_MarchingCubes.Dispatch(kernel, ((int)_resolution.x - 1) / 8, ((int)_resolution.y - 1)/ 8, ((int)_resolution.z - 1)/ 8);

        Vector3[] vertices = new Vector3[edgeNum];
        int[] indices = new int[edgeNum * 3 * 100];

        vertexBuffer.GetData(vertices);
        indexBuffer.GetData(indices);

        chargeBuffer.Dispose();
        cubeBuffer.Dispose();
        cNodeBuffer.Dispose();
        edgeBuffer.Dispose();
        vertexIndexPointer.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();

        Mesh mesh = GetComponent<Mesh>();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;
    }
}

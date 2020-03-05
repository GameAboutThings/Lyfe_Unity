using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Meta_CellEditor.SCULPTING.NODES;
using static Meta_CellEditor.SCULPTING.MISC;
using System.Runtime;

public class Base_CellEditor : MonoBehaviour
{
    private Node_CellEditor baseNode;
    private ComputeShader cs_Charges;
    private ComputeShader cs_MarchingCubes;
    private MeshGenerator meshGenerator;

    private ESymmetry eSymmetryMode;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        Init();

        InstantiateNew();
    }
    
    void Update()
    {
        int x = Meta_CellEditor.SCULPTING.GRID.DIMENSION.X;
        int y = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y;
        int z = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Z;
        float[] charges = CalculateCharges(new Vector3Int(x, y, z));
        meshGenerator.GenerateMesh(charges);
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

    private void Init()
    {
        eSymmetryMode = ESymmetry.EOff;

        cs_Charges = Resources.Load<ComputeShader>("Shaders/cs_Charges");

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }
        int x = Meta_CellEditor.SCULPTING.GRID.DIMENSION.X;
        int y = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Y;
        int z = Meta_CellEditor.SCULPTING.GRID.DIMENSION.Z;
        meshGenerator = new MeshGenerator(mesh, new Vector3Int(x, y, z), transform, false);
        if (meshCollider.sharedMesh == null)
        {
            meshCollider.sharedMesh = mesh;
        }
        // force update
        meshCollider.enabled = false;
        meshCollider.enabled = true;
        meshRenderer.material = Resources.Load<Material>("Material/Shader Graphs_s_cell");
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

    public ESymmetry GetSymmetryMode()
    {
        return eSymmetryMode;
    }
}

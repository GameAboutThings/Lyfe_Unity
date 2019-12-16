using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Meta_CellEditor.SCULPTING.NODES;
using static Meta_CellEditor.SCULPTING.MISC;

public class Node_CellEditor : MonoBehaviour
{
    private bool isMouseOver; //wether the node is hovered
    private bool isSelected; //wether the mouse is selected
    private Base_CellEditor editorBase; //the base of the editor tree
    private Node_CellEditor parentNode; //parent node of the current node
    private Material sphereMaterial;
    private Vector3 distortion = new Vector3(1, 1, 1);
    private float radius = 2; //radius of the node
    private float cubePortion = 0f; //value between 0 and 1
    private bool symmetryNode = false;



    protected ENodeType eType; //If this node is a base node or just a regular one
    /** The position of the node relative to the parent 
	*	  O above
	*	<-O-> 
	*	  O below
	*	  O O right */
    protected ENodePosition ePositionToParent;
    protected Mesh sphereRepresentation; //the mesh to represent this object in the world
    protected Node_CellEditor childAbove;
    protected Node_CellEditor childRight;
    protected Node_CellEditor childBelow;
    protected Node_CellEditor childLeft;
    protected Arrow_CellEditor arrowUp;
    protected Arrow_CellEditor arrowRight;
    protected Arrow_CellEditor arrowDown;
    protected Arrow_CellEditor arrowLeft;
    protected SpringJoint connectionToParent;

    // Start is called before the first frame update
    void Awake()
    {
        sphereMaterial = GetComponent<MeshRenderer>().material;
        sphereMaterial.color = COLOR_BASE;
        if (eType == ENodeType.EBase)
        {
            GetComponent<GB_Dragable>().allowDragging = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    //-------------------------------------------------------------------------------------------//

    /** Call this after creating a new node 
    * Will set all necessary member variables and assign a material
    *
    * @param _eNewType The type of node you created; Most likely normal
    * @param _eNewPositionToParent Where this node is relative to its parent
    * @param _parent The parentNode for this node
    */
    public void PostConstructor(ENodeType _eNewType, ENodePosition _eNewPositionToParent, Node_CellEditor _parent)
    {
        eType = _eNewType;
        ePositionToParent = _eNewPositionToParent;
        parentNode = _parent;
        InstantiateArrows();
        if (eType == ENodeType.EBase)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            connectionToParent = GetComponent<SpringJoint>();
            connectionToParent.connectedBody = parentNode.gameObject.GetComponent<Rigidbody>();
        }
        if (eType != ENodeType.EBase)
        {
            GetComponent<GB_Dragable>().allowDragging = true;
            GetComponent<GB_Dragable>().NodeDrag(_parent.gameObject);
        }
    }

    /** Get whether this node is of type normal or base */
    public ENodeType GetNodeType()
    {
        return eType;
    }

    private void CheckInput()
    {
        if (!Designer_CellEditor.GetEditorInputEnabled())
            return;

        CheckClick();

        if (Input.GetKeyDown(KeyCode.Delete) && isSelected && eType != ENodeType.EBase)
        {
            parentNode.RemoveChild(ePositionToParent);
        }

        if ((Input.GetKeyDown(KeyCode.A) && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))) ||
            (Input.GetKey(KeyCode.A) && (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl))))
        {
            Select();
        }

        Scale();
    }

    /** Returns the mesh component */
    public Mesh GetMesh()
    {
        return sphereRepresentation;
    }

    public void OnArrowClicked(Arrow_CellEditor _arrow)
    {
        if (_arrow == arrowUp)
        {
            CreateAndAttachChildNode(ENodePosition.EAbove);
        }
        else if (_arrow == arrowDown)
        {
            CreateAndAttachChildNode(ENodePosition.EBelow);
        }
        else if (_arrow == arrowRight)
        {
            CreateAndAttachChildNode(ENodePosition.ERight);
        }
        else if (_arrow == arrowLeft)
        {
            CreateAndAttachChildNode(ENodePosition.ELeft);
        }
        else
        {
            Debug.Log("ERROR : OnArrowClicked received arrow that doesn't belong to node");
        }
    }

    public void CreateAndAttachChildNode(ENodePosition _ePosition)
    {
        Vector3 childPos = Vector3.zero;
        Quaternion childRot = Quaternion.Euler(Vector3.zero);
        ENodeType eChildType = ENodeType.ENormal;

        if (eType != ENodeType.EBase)
        {
            List<Node_CellEditor> armNodes = GetAllNodesOfArm();
            //this node is also in there as the last node so we have to take the node at index lenth -2
            Node_CellEditor parentNode = armNodes[armNodes.Count - 2];

            if (parentNode != null)
            {
                Vector3 pos = transform.position;
                Vector3 parPos = parentNode.transform.position;
                Vector3 dir = (pos - parPos).normalized;
                childPos = dir * DISTANCE_AVERAGE;
                float rot = StaticMaths.FindLookAtAngle2D(pos, parPos); //maybe pos and childPos or parPos and childPos
                childRot = Quaternion.Euler(0f, rot, 0f);
                //childRot = gameObject.transform.rotation;

                if ((ePositionToParent == ENodePosition.ERight && _ePosition == ENodePosition.EAbove) ||
                    (ePositionToParent == ENodePosition.ELeft && _ePosition == ENodePosition.EBelow))
                {
                    childPos = new Vector3(-childPos.z, childPos.y, childPos.x);
                }
                else if ((ePositionToParent == ENodePosition.EAbove && _ePosition == ENodePosition.ELeft) ||
                    (ePositionToParent == ENodePosition.EBelow && _ePosition == ENodePosition.ERight))
                {
                    childPos = new Vector3(-childPos.z, childPos.y, childPos.x);
                }
                else if ((ePositionToParent == ENodePosition.ERight && _ePosition == ENodePosition.EBelow) ||
                    (ePositionToParent == ENodePosition.ELeft && _ePosition == ENodePosition.EAbove))
                {
                    childPos = new Vector3(childPos.z, childPos.y, -childPos.x);
                }
                else if ((ePositionToParent == ENodePosition.EAbove && _ePosition == ENodePosition.ERight) ||
                    (ePositionToParent == ENodePosition.EBelow && _ePosition == ENodePosition.ELeft))
                {
                    childPos = new Vector3(childPos.z, childPos.y, -childPos.x);
                }
            }
            else
            {
                Debug.Log("FATAL ERROR : non-base node doesn't have parent node. ");
            }
        }
        else
        {
            childPos = transform.position;
            eChildType = ENodeType.ESingle;
            switch (_ePosition)
            {
                case ENodePosition.EAbove:
                    childPos.z += DISTANCE_AVERAGE;
                    break;
                case ENodePosition.EBelow:
                    childPos.z -= DISTANCE_AVERAGE;
                    break;
                case ENodePosition.ERight:
                    childPos.x += DISTANCE_AVERAGE;
                    break;
                case ENodePosition.ELeft:
                    childPos.x -= DISTANCE_AVERAGE;
                    break;
            }
        }

        childPos += transform.position;

        //These ifs make sure that you can't create a child node on the socket where a parent node is attached
        if (_ePosition == ENodePosition.EAbove && ePositionToParent != ENodePosition.EBelow)
        {
            GameObject chA = Instantiate(NODE_TEMPLATE, childPos, childRot);
            chA.transform.parent = transform; 
            childAbove = chA.GetComponent<Node_CellEditor>();
            childAbove.PostConstructor(eChildType, _ePosition, this);
            if (arrowUp != null)
            {
                Destroy(arrowUp.gameObject);
                arrowUp = null;
            }
        }
        else if (_ePosition == ENodePosition.ERight && ePositionToParent != ENodePosition.ELeft)
        {
            GameObject chR = Instantiate(NODE_TEMPLATE, childPos, childRot);
            chR.transform.parent = transform;
            childRight = chR.GetComponent<Node_CellEditor>();
            childRight.PostConstructor(eChildType, _ePosition, this);

            if (arrowRight != null)
            {
                Destroy(arrowRight.gameObject);
                arrowRight = null;
            }
        }
        else if (_ePosition == ENodePosition.EBelow && ePositionToParent != ENodePosition.EAbove)
        {
            GameObject chB = Instantiate(NODE_TEMPLATE, childPos, childRot);
            chB.transform.parent = transform;
            childBelow = chB.GetComponent<Node_CellEditor>();
            childBelow.PostConstructor(eChildType, _ePosition, this);

            if (arrowDown != null)
            {
                Destroy(arrowDown.gameObject);
                arrowDown = null;
            }
        }
        else if (_ePosition == ENodePosition.ELeft && ePositionToParent != ENodePosition.ERight)
        {
            GameObject chL = Instantiate(NODE_TEMPLATE, childPos, childRot);
            chL.transform.parent = transform;
            childLeft = chL.GetComponent<Node_CellEditor>();
            childLeft.PostConstructor(eChildType, _ePosition, this);

            if (arrowLeft != null)
            {
                Destroy(arrowLeft.gameObject);
                arrowLeft = null;
            }
        }

        if (eType != ENodeType.EBase)
            parentNode.UpdateNodeType();
        else
            UpdateNodeType();
    }

    void OnMouseOver()
    {
        isMouseOver = true;

        if(!isSelected)
            sphereMaterial.color = COLOR_HOVER;
    }

    void OnMouseExit()
    {
        isMouseOver = false;

        if(!isSelected)
            sphereMaterial.color = COLOR_BASE;
    }

    private void CheckClick()
    {
        if (Input.GetMouseButtonUp(0) && eType != ENodeType.EBase)
            GetComponent<Rigidbody>().isKinematic = true;

        if (Input.GetMouseButtonDown(0))
        {
            if (eType != ENodeType.EBase)
                GetComponent<Rigidbody>().isKinematic = false;

            if (isSelected)
            {
                if (!isMouseOver && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    Deselect(false);
                }
                else if (isMouseOver && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    Deselect(true);
                }
            }
            else
            {
                if (isMouseOver)
                {
                    Select();
                }
            }
        }
    }

    private void Select()
    {
        isSelected = true;
        sphereMaterial.color = COLOR_SELECTED;
        Designer_CellEditor.AddSelectedNode(this);
        //GetComponent<Rigidbody>().drag = float.MaxValue;
    }

    private void Deselect(bool _hover)
    {
        isSelected = false;

        if(_hover)
            sphereMaterial.color = COLOR_HOVER;
        else
            sphereMaterial.color = COLOR_BASE;
        Designer_CellEditor.RemoveSelectedNode(this);
        //GetComponent<Rigidbody>().drag = 1f;
    }

    private void Scale()
    {
        if (!isMouseOver)
            return;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            Morph();
            return;
        }

        float scroll = Input.mouseScrollDelta.y;

        if (scroll == 0)
            return;

        bool increase = scroll > 0;

        if (increase)
        {
            if (radius < 10f)
                radius += SCALING_FACTOR;
        }
        else
        {
            if (radius > 3f)
                radius -= SCALING_FACTOR;
        }
    }

    private void Morph()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll == 0)
            return;

        bool increase = scroll > 0;

        if (increase)
        {
            if (cubePortion < 1f)
                cubePortion += SCALING_FACTOR / 3f;
        }
        else
        {
            if (cubePortion > 0f)
                cubePortion -= SCALING_FACTOR / 3f;
        }
    }

    private void InstantiateArrows()
    {
        GameObject arr;
        switch (eType)
        {
            case ENodeType.EBase:
                if (childAbove == null)
                {
                    arr = InstantiateArrow(new Vector3(0,0, 1.5f), Quaternion.Euler(90, 0, 0));
                    arrowUp = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childBelow == null)
                {
                    arr = InstantiateArrow(new Vector3(0, 0, -1.5f), Quaternion.Euler(-90, 0, 0));
                    arrowDown = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childLeft == null)
                {
                    arr = InstantiateArrow(new Vector3(-1.5f, 0, 0), Quaternion.Euler(0, 0, 90));
                    arrowLeft = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childRight == null)
                {
                    arr = InstantiateArrow(new Vector3(1.5f, 0, 0), Quaternion.Euler(0, 0, -90));
                    arrowRight = arr.GetComponent<Arrow_CellEditor>();
                }
                break;
            case ENodeType.ESplit:
            case ENodeType.ENormal:
                if (childAbove == null && ePositionToParent != ENodePosition.EBelow)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(0, 0, 1.5f), Quaternion.Euler(90, 0, 0));
                    arrowUp = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childBelow == null && ePositionToParent != ENodePosition.EAbove)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(0, 0, -1.5f), Quaternion.Euler(-90, 0, 0));
                    arrowDown = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childLeft == null && ePositionToParent != ENodePosition.ERight)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(-1.5f, 0, 0), Quaternion.Euler(0, 0, 90));
                    arrowLeft = arr.GetComponent<Arrow_CellEditor>();
                }
                if (childRight == null && ePositionToParent != ENodePosition.ELeft)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(1.5f, 0, 0), Quaternion.Euler(0, 0, -90));
                    arrowRight = arr.GetComponent<Arrow_CellEditor>();
                }
                break;
            case ENodeType.ESingle:
                if (ePositionToParent == ENodePosition.EAbove && childAbove == null)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(0, 0, 1.5f), Quaternion.Euler(90, 0, 0));
                    arrowUp = arr.GetComponent<Arrow_CellEditor>();
                }
                else if (ePositionToParent == ENodePosition.EBelow && childBelow == null)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(0, 0, -1.5f), Quaternion.Euler(-90, 0, 0));
                    arrowDown = arr.GetComponent<Arrow_CellEditor>();
                }
                else if (childLeft == null && ePositionToParent == ENodePosition.ELeft)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(-1.5f, 0, 0), Quaternion.Euler(0, 0, 90));
                    arrowLeft = arr.GetComponent<Arrow_CellEditor>();
                }
                else if (childRight == null && ePositionToParent == ENodePosition.ERight)
                {
                    arr = InstantiateArrow(transform.position + new Vector3(1.5f, 0, 0), Quaternion.Euler(0, 0, -90));
                    arrowRight = arr.GetComponent<Arrow_CellEditor>();
                }
                break;
            case ENodeType.EEnd:
                break;
            default:
                Debug.Log("FATAL ERROR : Node has unknown type when instantiating arrows");
                break;
        }
    }  

    private GameObject InstantiateArrow(Vector3 _position, Quaternion _rotation)
    {
        GameObject arrowObject = Instantiate(Meta_CellEditor.SCULPTING.ARROW.GetArrowTemplate(), _position, _rotation);
        arrowObject.transform.parent = transform;
        return arrowObject;
    }

    private void ReinstanteArrows()
    {
        int countAllChildren = transform.childCount;
        for (int i = 0; i < countAllChildren; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.tag.Equals("Arrow_CellEditor"))
            {
                Destroy(child);
            }
        }
        InstantiateArrows();
    }

    /*
     * Returns a list of all nodes in the current from the base node to the current one
     */
    private List<Node_CellEditor> GetAllNodesOfArm()
    {
        List<Node_CellEditor> allNodes;
        if (eType == ENodeType.EBase)
        {
            allNodes = new List<Node_CellEditor>();
            allNodes.Add(this);
        }
        else
        {
            allNodes = transform.parent.GetComponent<Node_CellEditor>().GetAllNodesOfArm();
            allNodes.Add(this);
        }

        return allNodes;
    }

    private void UpdateNodeType()
    {
        if (eType == ENodeType.EBase)
        {
            if (childAbove != null)
                childAbove.UpdateNodeType();
            if (childLeft != null)
                childLeft.UpdateNodeType();
            if (childBelow != null)
                childBelow.UpdateNodeType();
            if (childRight != null)
                childRight.UpdateNodeType();
        }
        else
        {
            if (eType != ENodeType.EEnd)
            {
                List<Node_CellEditor> nodesOfArm = GetAllNodesOfArm();

                if (nodesOfArm.Count >= Meta_CellEditor.SCULPTING.NODES.MAXIMUM_PER_ARM)
                {
                    eType = ENodeType.EEnd;
                    GetComponent<Rigidbody>().drag = float.MaxValue;
                }
                else if (parentNode.eType == ENodeType.EBase || parentNode.eType == ENodeType.ESplit)
                {
                    int childCount = 0;
                    eType = ENodeType.ESingle;
                    if (childAbove != null)
                    {
                        childAbove.UpdateNodeType();
                        childCount++;
                    }
                    else if (childLeft != null)
                    {
                        childLeft.UpdateNodeType();
                        childCount++;
                    }

                    else if (childBelow != null)
                    {
                        childBelow.UpdateNodeType();
                        childCount++;
                    }
                    else if (childRight != null)
                    {
                        childRight.UpdateNodeType();
                        childCount++;
                    }

                    if (childCount == 0)
                    {
                        GetComponent<Rigidbody>().drag = float.MaxValue;
                    }
                    else
                    {
                       GetComponent<Rigidbody>().drag = 1f;
                    }

                }
                else if (parentNode.eType == ENodeType.ENormal || parentNode.eType == ENodeType.ESingle)
                {
                    int countChild = 0;

                    if (childAbove != null)
                        countChild++;
                    if (childLeft != null)
                        countChild++;
                    if (childBelow != null)
                        countChild++;
                    if (childRight != null)
                        countChild++;

                    if (countChild == 0)
                    {
                        GetComponent<Rigidbody>().drag = float.MaxValue;
                    }
                    else
                    {
                        GetComponent<Rigidbody>().drag = 1f;
                    }

                    if (countChild > 1)
                    {
                        eType = ENodeType.ESplit;
                        parentNode.eType = ENodeType.ESingle;
                    }
                    else
                        eType = ENodeType.ENormal;

                    if (childAbove != null)
                        childAbove.UpdateNodeType();
                    if (childLeft != null)
                        childLeft.UpdateNodeType();
                    if (childBelow != null)
                        childBelow.UpdateNodeType();
                    if (childRight != null)
                        childRight.UpdateNodeType();
                }
            }
            else
            {
                GetComponent<Rigidbody>().drag = float.MaxValue;
            }
        }
        
        ReinstanteArrows();
    }

    private void RemoveChild(ENodePosition _ePosition)
    {
        switch (_ePosition)
        {
            case ENodePosition.EAbove:
                Destroy(childAbove.gameObject);
                childAbove = null;
                break;
            case ENodePosition.ELeft:
                Destroy(childLeft.gameObject);
                childLeft = null;
                break;
            case ENodePosition.EBelow:
                Destroy(childBelow.gameObject);
                childBelow = null;
                break;
            case ENodePosition.ERight:
                Destroy(childRight.gameObject);
                childRight = null;
                break;
        }
        if (eType == ENodeType.EBase)
            UpdateNodeType();
        else
            parentNode.UpdateNodeType();
    }

    private Node_CellEditor GetChild(ENodePosition _ePosition)
    {
        switch (_ePosition)
        {
            case ENodePosition.EAbove:
                return childAbove;
            case ENodePosition.ELeft:
                return childLeft;
            case ENodePosition.EBelow:
                return childBelow;
            case ENodePosition.ERight:
                return childRight;
            default:
                return null;
        }
    }

    /*
     * Returns all nodes from this one down.
     * 
     * @param _nodes List an empty list that will be filled with nodes an then returned
     * 
     * @return list with all nodes currently in the editor
     */
    public List<Node_CellEditor> GetAllChildNodes(List<Node_CellEditor> _nodesList)
    {
        _nodesList.Add(this);

        if (childAbove != null)
            _nodesList = childAbove.GetAllChildNodes(_nodesList);
        if (childBelow != null)
            _nodesList = childBelow.GetAllChildNodes(_nodesList);
        if (childLeft != null)
            _nodesList = childLeft.GetAllChildNodes(_nodesList);
        if (childRight != null)
            _nodesList = childRight.GetAllChildNodes(_nodesList);

        return _nodesList;
    }

    /*
     * Returns the number of direct children to this node. (Number between 0 and 3; 4 for  base nodes)
     */
    public int GetChildCount()
    {
        int ret = 0;

        if (childAbove != null)
            ret++;
        if (childBelow != null)
            ret++;
        if (childLeft != null)
            ret++;
        if (childRight != null)
            ret++;

        return ret;
    }

    /*
     * Returns all nodes from this one down.
     * 
     * @param _nodes List an empty list that will be filled with nodes an then returned
     * 
     * @return list with all nodes currently in the editor
     */
    public List<SNode> GetAllChildNodeMetaData(List<SNode> _nodesList)
    {
        _nodesList.Add(GetMetaData());

        if (childAbove != null)
            _nodesList = childAbove.GetAllChildNodeMetaData(_nodesList);
        if (childBelow != null)
            _nodesList = childBelow.GetAllChildNodeMetaData(_nodesList);
        if (childLeft != null)
            _nodesList = childLeft.GetAllChildNodeMetaData(_nodesList);
        if (childRight != null)
            _nodesList = childRight.GetAllChildNodeMetaData(_nodesList);

        return _nodesList;
    }

    public SNode GetMetaData()
    {
        SNode x = new SNode();

        x.position = transform.position;
        x.radius = radius;
        x.distortion = distortion;
        x.cubePortion = cubePortion;

        return x;
    }

    private List<Node_CellEditor> GetSymmetryNodes(ESymmetry _eSymmetry)
    {
        List<Node_CellEditor> ret = new List<Node_CellEditor>();
        Stack<ENodePosition> positions = new Stack<ENodePosition>();

        positions.Push(ePositionToParent);
        Node_CellEditor center = parentNode;
        int children = center.GetChildCount();

        //find the center of symmetry
        if (Designer_CellEditor.HasSymmetryNode())
        {
            while (center.eType != ENodeType.EBase && !center.symmetryNode)
            {
                positions.Push(center.ePositionToParent);
                center = center.parentNode;
                children = center.GetChildCount();
            }

            //if the base is reached withou finding the symmetry node, stop
            if (!center.symmetryNode)
                return ret;
        }
        else
        {
            while (center.eType != ENodeType.EBase)
            {
                positions.Push(center.ePositionToParent);
                center = center.parentNode;
                children = center.GetChildCount();
            }
        }
        //if the symmetry node has only one child, stop
        if (children == 1)
            return ret;


        //now we have the center of symmetry
        //iterater over all the steps it took to get here, but backwards

        if (_eSymmetry == ESymmetry.EMirror || _eSymmetry == ESymmetry.EPointMirror)
        {
            ENodePosition curPos;
            curPos = positions.Pop();
            while (positions.Count != 0 && center != null)
            {
                curPos = UTIL_CellEditor.GetOppositePosition(curPos);
                center = center.GetChild(curPos);
            }

            if(center != null)
                ret.Add(center);
        }
        else if (_eSymmetry == ESymmetry.EPoint)
        {
            //FUCK!
        }

        

        return ret;
    }
}

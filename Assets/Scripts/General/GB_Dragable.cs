using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GB_Dragable : MonoBehaviour
{
    private Vector3 mouseOffset;
    private float zCoord;
    public bool allowDragging = true;
    public bool rotateAroundParent = true;
    public bool checkRadius_node = false;
    private float minRadius_node = 0f;
    private float maxRadius_node = 3f;
    private GameObject parent_node;
    private Vector3 startPos;

    public void NodeDrag(GameObject _parentNode)
    {
        checkRadius_node = true;
        minRadius_node = Meta_CellEditor.SCULPTING.NODES.DISTANCE_MIN;
        maxRadius_node = Meta_CellEditor.SCULPTING.NODES.DISTANCE_MAX;
        parent_node = _parentNode;
        startPos = transform.position;
        //I got no idea why, but this line fixes the problem with the nodes suddenly rotating when clicking on them, so LEAVE IT IN THERE
        transform.rotation = StaticMaths.FindQuaternion((startPos - parent_node.transform.position), (startPos - parent_node.transform.position));
    }

    void OnMouseDown()
    {
        if (!allowDragging)
            return;

        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        //store offset = gameobject world pos - mouse world pos
        mouseOffset = gameObject.transform.position - Util_World.GetMouseWorldPosXZ(zCoord);
    }

    void OnMouseDrag()
    {
        if (!allowDragging)
            return;

        Vector3 target = Util_World.GetMouseWorldPosXZ(zCoord) + mouseOffset;
        if (checkRadius_node)
        {
            target = KeepPointInsideZone(target);
        }

        transform.position = target;

        if (rotateAroundParent)
        {
            transform.rotation = StaticMaths.FindQuaternion((startPos - parent_node.transform.position), (target - parent_node.transform.position));
        }

        //Debug.Log(StaticMaths.FindQuaternion((startPos - parent_node.transform.position), (target - parent_node.transform.position)));
        //Debug.Log(StaticMaths.FindQuaternion((startPos), (target)));
        //Vector3 offset = new Vector3(0, 0, 0);
        //Debug.DrawLine(offset, startPos + offset - parent_node.transform.position, Color.blue);
        //Debug.DrawLine(offset, target + offset - parent_node.transform.position, Color.green);
        ////Debug.DrawLine(startPos + offset - parent_node.transform.position, target + offset - parent_node.transform.position, Color.green);
        //Debug.DrawLine(parent_node.transform.position + offset, startPos + offset, Color.yellow);
        //Debug.DrawLine(parent_node.transform.position + offset, target + offset, Color.red);
        ////Debug.DrawLine(startPos + offset, target + offset, Color.red);
    }

    private Vector3 KeepPointInsideZone(Vector3 _point)
    {
        //within the outer sphere
        if (StaticMaths.InsideSphere(maxRadius_node, parent_node.transform.position, _point))
        {
            //within the inner sphere
            if (StaticMaths.InsideSphere(minRadius_node, parent_node.transform.position, _point))
            {
                return StaticMaths.ProjectOntoSphere(minRadius_node, parent_node.transform.position, _point);
            }
            else
            {
                return _point;
            }
        }
        else
        {
            return StaticMaths.ProjectOntoSphere(maxRadius_node, parent_node.transform.position, _point);
        }
    }
}

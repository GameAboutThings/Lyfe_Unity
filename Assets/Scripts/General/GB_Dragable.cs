using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GB_Dragable : MonoBehaviour
{
    private Vector3 mouseOffset;
    private float zCoord;
    public bool allowDragging = true;
    private bool checkRadius_node = false;
    private float minRadius_node = 0f;
    private float maxRadius_node = 3f;
    private GameObject parent_node;

    public void NodeDrag(GameObject _parentNode)
    {
        checkRadius_node = true;
        minRadius_node = Meta_CellEditor.SCULPTING.NODES.DISTANCE_MIN;
        maxRadius_node = Meta_CellEditor.SCULPTING.NODES.DISTANCE_MAX;
        parent_node = _parentNode;
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

            transform.position = KeepPointInsideZone(target);
        }
        else
        {
            transform.position = target;
        }
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

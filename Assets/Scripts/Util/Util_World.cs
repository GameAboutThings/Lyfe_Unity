using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_World : MonoBehaviour
{   
    public static Vector3 GetMouseWorldPosXZ(float zCoord)
    {
        //pixel coordinates (x, y)
        Vector3 mousePoint = Input.mousePosition;

        //z coordinate of game object on screen
        mousePoint.z = zCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBehaviour : MonoBehaviour
{
    bool isMouseOver = false;

    void OnMouseOver()
    {
        isMouseOver = true;
        Player_World.SetCurrentlySelectedHex(gameObject);
    }

    void OnMouseExit()
    {
        isMouseOver = false;
    }
}

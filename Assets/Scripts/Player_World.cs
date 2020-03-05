using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_World : MonoBehaviour
{
    private static GameObject currentlySelectedHex;

    public static void SetCurrentlySelectedHex(GameObject _hex)
    {
        currentlySelectedHex = _hex;
    }

    public static GameObject GetCurrentlySelectedHex()
    {
        return currentlySelectedHex;
    }
}

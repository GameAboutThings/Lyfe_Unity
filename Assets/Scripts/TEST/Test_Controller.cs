using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Controller : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            UTIL.OpenMainMenu();
        }
    }
}

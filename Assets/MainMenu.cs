using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OpenCellEditor()
    {
        UTIL.OpenCellEditor("");
    }

    public void OpenMapShowcase()
    {
        UTIL.OpenScene("MapGenerator_TEST");
    }

    public void Quit()
    {
        Application.Quit();
    }
}

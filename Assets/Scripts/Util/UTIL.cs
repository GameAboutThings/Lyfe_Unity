using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UTIL
{
    public enum EPosition
    {
        EAbove,
        EBelow,
        ERight,
        ELeft,
        ECenter
    }

    public static void OpenCellEditor(string _blueprint)
    {
        if (_blueprint == "")
        {
            OpenScene("Editor_Cell");
        }
        else
        {
            OpenScene("Editor_Cell");
        }
    }

    public static void OpenScene(string _scene)
    {
        SceneManager.LoadScene(_scene);
    }

    public static void OpenMainMenu()
    {
        OpenScene("MainMenu");
    }
}

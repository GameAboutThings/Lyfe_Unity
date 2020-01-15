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

    public static byte[] StringToByteArray(string _str)
    {
        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        return enc.GetBytes(_str);
    }

    public static string ByteArrayToString(byte[] _arr)
    {
        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        return enc.GetString(_arr);
    }

    public static float ByteToFloat(byte x)
    {
        return (x / (float)255.0e7);
    }
}

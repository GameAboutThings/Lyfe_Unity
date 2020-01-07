using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UTIL
{
    public static string allChars = "1234567890ß!§$%&()=?abcdefghijklmnopqrstuvwxyzABZDEFGHIJKLMNOPQRSTUVWXYZ.:,;-_<>|+-*/#'";

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

    /*
     * Generate random string of given length
     */
    public static string GenerateRandomString(int _length)
    {
        string r = "";

        for (int i = 0; i < _length; i++)
        {
            r += allChars[Random.Range(0, allChars.Length)];
        }

        return r;
    }

    public static string GetMapSeed()
    {
        return GenerateRandomString(Meta_MapGenerator.MISC.seedLength);
    }

    public static float ByteToFloat(byte x)
    {
        return (x / (float)255.0e7);
    }

    /*
     * Takes a string and splits it in the middle.
     * Then turns both halfs into floats.
     */
    public static Vector2 GetSeedVectorFromString(string _input)
    {
        string a = _input.Substring(0, _input.Length / 2);
        string b = _input.Substring(_input.Length / 2, _input.Length / 2 - 1);

        byte[] aByte = StringToByteArray(a);
        byte[] bByte = StringToByteArray(b);

        float[] aFloats = new float[aByte.Length];
        float[] bFloats = new float[bByte.Length];

        for (int i = 0; i < aByte.Length; i++)
        {
            aByte[0] ^= aByte[i];
        }
        for (int i = 0; i < bByte.Length; i++)
        {
            bByte[0] ^= bByte[i];
        }

        return new Vector2(
            System.BitConverter.ToInt16(aByte, 0),
            System.BitConverter.ToInt16(bByte, 0)
            );
    }
}

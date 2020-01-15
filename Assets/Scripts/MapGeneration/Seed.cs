using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed
{
    public static string allowedChars = "0123456789ß!§$%&()=?abcdefghijklmnopqrstuvwxyzABZDEFGHIJKLMNOPQRSTUVWXYZ.:,;-_<>|+-*/#'"; //89
    public static int defaultNumber = 0;

    int length = Meta_MapGenerator.MISC.seedLength;
    int[] seed;
    int[,] seed2D;
    string seedString;
    char[,] seed2DChar;
    Vector2 worldNoiseOffset;

    public Seed()
    {
        seed = new int[length];
        seedString = "";
        for (int i = 0; i < length; i++)
        {
            seed[i] = Random.Range(0, allowedChars.Length);
            seedString += GetCharWrapped(seed[i]);
        }
        GenerateSeedVectorFromString();
        Generate2DSeeds();
    }

    public Seed(string _seed)
    {
        seed = new int[length];
        seedString = _seed;
        for(int i = 0; i < length; i++)
        {
            if (i >= _seed.Length)
            {
                seed[i] = defaultNumber;
                seedString += GetCharWrapped(seed[i]);
            }
            else
            {
                seed[i] = allowedChars.IndexOf(_seed[i]);
            }
        }
        GenerateSeedVectorFromString();
        Generate2DSeeds();
    }

    private char GetCharWrapped(int _index)
    {
        return allowedChars[_index % allowedChars.Length];
    }

    /*
     * Takes a string and splits it in the middle.
     * Then turns both halfs into floats.
     */
    private void GenerateSeedVectorFromString()
    {
        string a = seedString.Substring(0, seedString.Length / 2);
        string b = seedString.Substring(seedString.Length / 2, seedString.Length / 2 - 1);

        byte[] aByte = UTIL.StringToByteArray(a);
        byte[] bByte = UTIL.StringToByteArray(b);

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

        worldNoiseOffset =  new Vector2(
                    System.BitConverter.ToInt16(aByte, 0),
                    System.BitConverter.ToInt16(bByte, 0)
                    );
    }

    private void Generate2DSeeds()
    {
        seed2D = new int[length, length];
        seed2DChar = new char[length, length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                seed2D[i, j] = (seed[i] * seed[j]) % allowedChars.Length;
                seed2DChar[i, j] = GetCharWrapped(seed2D[i, j]);
            }
        }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //++++++++++++++++++++++++++++++++ GETTER +++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public string GetSeedString()
    {
        return seedString;
    }

    public int[] GetSeed()
    {
        return seed;
    }

    public int[,] GetSeed2D()
    {
        return seed2D;
    }

    public Vector2 GetWorldOffset()
    {
        return worldNoiseOffset;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionMap<T>
{
    private Dictionary<T, Vector2Int> positionMap;
    private Dictionary<Vector2Int, T> elementMap;

    public PositionMap()
    {
        positionMap = new Dictionary<T, Vector2Int>();
        elementMap = new Dictionary<Vector2Int, T>();
    }

    //public void Add(T _element, Vector2Int _position)
    //{
    //    if (GetIndex(_element) != default(Vector2Int))
    //        ThrowElementAlreadyListed(_element, _position);

    //    if (!GetAt(_position).Equals(null))
    //        ThrowPositionAlreadyTaken(_position);

    //    HardInsert(_element, _position);
    //}

    //public void Replace(T _element, Vector2Int _position)
    //{
    //    if (GetIndex(_element) == default(Vector2Int))
    //        ThrowElementNotFound(_element);

    //    if (GetAt(_position).Equals(null))
    //        ThrowPositionEmpty(_position);

    //    HardInsert(_element, _position);
    //}

    public void HardInsert(T _element, Vector2Int _position)
    {
        positionMap.Add(_element, _position);
        elementMap.Add(_position, _element);
    }

    public T GetAt(Vector2Int _positon)
    {
        try
        {
            return elementMap[_positon];
        }
        catch(KeyNotFoundException ex)
        {
            return default(T);
        }
    }

    public Vector2Int GetIndex(T _element)
    {
        try
        {
            return positionMap[_element];
        }
        catch (KeyNotFoundException ex)
        {
            return default(Vector2Int);
        }
    }

    public void Remove(Vector2Int _position)
    {
        positionMap.Remove(elementMap[_position]);
        elementMap.Remove(_position);
    }

    public void Remove(T _element)
    {
        elementMap.Remove(positionMap[_element]);
        positionMap.Remove(_element);
    }

    /*
     * Goes to the defined location and goes down by _dimensions.y and right by _dimensions.x.
     * The resulting set will be returned as an array.
     * 
     * If a certain spot isn't filled in the mesh this part will be returned as null
     */
    public T[,] Get2DArray(Vector2Int _corner, Vector2Int _dimensions)
    {
        T[,] ret = new T[_dimensions.x, _dimensions.y];

        for (int i = 0; i < _dimensions.x; i++)
        {
            for (int j = 0; j < _dimensions.y; j++)
            {
                T t = GetAt(new Vector2Int(i, j));
                if(t != null)
                    ret[i, j] = t;
            }
        }

        return ret;
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++ EXCEPTIONS ++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private void ThrowElementAlreadyListed(T _element, Vector2Int _position)
    {
        throw new System.Exception("Trying to add " + _element + " to position " + _position + "\n" + _element + " already listed at " + positionMap[_element]);
    }

    private void ThrowPositionAlreadyTaken(Vector2Int _position)
    {
        throw new System.Exception("Position " + _position + " is already taken by " + elementMap[_position]);
    }

    private void ThrowElementNotFound(T _element)
    {
        throw new System.Exception(_element + "is not listed");
    }

    private void ThrowPositionEmpty(Vector2Int _position)
    {
        throw new System.Exception("There is no elemented listed at position " + _position);
    }
}

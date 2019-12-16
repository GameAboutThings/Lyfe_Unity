using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Meta_CellEditor.SCULPTING.NODES;


public class UTIL_CellEditor
{
    public static ENodePosition GetOppositePosition(ENodePosition _ePosition)
    {
        switch (_ePosition)
        {
            case ENodePosition.EAbove:
                return ENodePosition.EBelow;
            case ENodePosition.EBelow:
                return ENodePosition.EAbove;
            case ENodePosition.ELeft:
                return ENodePosition.ERight;
            case ENodePosition.ERight:
                return ENodePosition.ELeft;
            default:
                return ENodePosition.EAbove;
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkTemplate
{
    protected Vector3 center;

    public Vector3 GetCenter()
    {
        return center;
    }
}

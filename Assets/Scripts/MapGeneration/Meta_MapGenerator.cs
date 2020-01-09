using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta_MapGenerator
{
    class PLANET
    {
        public static System.Tuple<int, int> seaLevelVariance = new System.Tuple<int, int>(-5, 10);
    }

    class MISC
    {
        public static int seedLength = 18;
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class CHUNK
    {
        public static Vector3Int tileCount = new Vector3Int(30, 70, 30);
    }

    public class TERRAIN
    {
        public enum ETerrain
        {
            //every terrain type added here has to be added to the GetTerrainMap method, too
            EFlat_1,
            EFlat_2,
            EMountains,
            EExtremeMountain,
            ERoughRocks,
            EGulch,
            EValley,
            EHighPlateau,
            ELowPlatou
        }

        public static float heightVariance = 1f;
        public static float smoothnessVariance = 0.5f;
        public static float colorVariance = 0.3f;
    }
}

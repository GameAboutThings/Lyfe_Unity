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
        public static int seedLength = 36;
        public static int TerrainDimension = 7; //number of terrain chunks loaded at the same time; number needs to be odd so there can be a center
    }

    class TILES
    {
        public static Vector3 Offset = new Vector3(1.73f, 1f, 1.5f);
    }

    class MAP
    {
        public static Vector3Int tileCount = new Vector3Int(40, 70, 44);
        public static int mapDimensions = 3; //number of terrain chunks loaded at the same time; number needs to be odd so there can be a center
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

        public static Vector2 baseSize = new Vector2(MAP.tileCount.x * TILES.Offset.x + 50f, MAP.tileCount.z * TILES.Offset.z + 50f);
        public static Vector2 sizeVariance = new Vector2(10f, 10f);

        public static int maxWeight = 75; //increasing this number makes the terrain type more pronounnced around the center of the terrain chunk
    }
}

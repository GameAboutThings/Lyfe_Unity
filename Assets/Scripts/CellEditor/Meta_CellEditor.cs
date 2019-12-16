using System;
using UnityEngine;

namespace Meta_CellEditor
{
    public class CAMERA
    {
        public class ZOOM
        {
            public static float FACTOR = 15; //how far you zoom in our out with each time you scroll
            public static float SPEED = 0.5f; //how fast you zoom in or out
            public static float DISTANCE_MIN = 30f;
            public static float DISTANCE_MAX = 150f;
        }
        public static float DISTANCE_START = 40f;
    }
	
	public class SCULPTING
	{
		public class NODES
		{
			public static float MAXIMUM_PER_ARM = 5f;
			public static float SIZE_MIN = 50f;
			public static float SIZE_MAX = 150f;
			public static float DISTANCE_MIN = 0.5f;
			public static float DISTANCE_MAX = 3f;
            public static float SCALING_FACTOR = 0.5f;
            public static float DISTANCE_AVERAGE
            {
                get { return (DISTANCE_MAX + DISTANCE_MIN) / 2f; }
            }
            public static Color COLOR_BASE = new Color(210f/255f, 205f/255f, 140f/255f, 0.8f);
            public static Color COLOR_HOVER = new Color(230f/255F, 220f/255f, 110f/255f, 0.8f);
            public static Color COLOR_SELECTED = new Color(250f/255f, 235f/255f, 80f/255f, 0.8f);

            public enum ENodeType : uint
			{
				//Only the Editor Base has the base node. This can't be deleted.
				//Every third node can become a split node
				//Every other node is a normal node
				/*
				* Every node starts out as normal except for the base node obviously
				* When one node is split it becomes a split 
				* it's parent^2 and child^2 on the not split axis become a single
				* base is a special case it stays on that type
				* condition for split:
				* node is a normal not a single
				*/
				EBase,
				ENormal,
				ESplit,
				ESingle,
                EEnd

                
			}
			
			public enum ENodePosition : uint
			{
				EAbove,
				EBelow,
				ERight,
				ELeft,
				EBase
			}

            private static GameObject template;
            public static GameObject NODE_TEMPLATE
            {
                get
                {
                    if (template == null)
                    {
                        template = Resources.Load<GameObject>("Prefab/CellEditor/Node_CellEditor");
                    }
                    return template;
                }

            }

            public struct SNode
            {
                public Vector3 position;
                public Vector3 distortion;
                public float cubePortion;
                public float radius;
            }
            public static int SNODE_SIZE
            {
                get {
                    return sizeof(float) * 3 + 
                        sizeof(float) * 3 + 
                        sizeof(float) + 
                        sizeof(float); }
            }
        }

		public class ARROW
		{
			public static float DISTANCEFROMBASE = 1.5f;
            public static Color COLOR_BASE = new Color(200f / 255f, 200f / 255f, 170f / 255f, 0.8f);
            public static Color COLOR_HOVER = new Color(170f / 255F, 170f / 255f, 120f / 255f, 0.8f);


            private static GameObject template;
            public static GameObject GetArrowTemplate()
            {
                if (template == null)
                {
                    template = Resources.Load<GameObject>("Prefab/CellEditor/Arrow_CellEditor");
                }
                return template;
            }
        }

		public class GRID
		{
			public class DIMENSION
			{
				public static int X = 200;
				public static int Y = 80;
				public static int Z = 200;
                //(x * y * z) % 16 has to be 0
			}
			public static float SCALE = 0.1f; //increasing this makes it blockier
            //making it less blocky also crams the points closer together => grid becomes smaller
		}

        public class MARCHING_CUBES
        {
            public static float ISOLEVEL = 0.1f;

            public struct SEDGE
            {
                bool used;
                Vector3 position;
                int vertexIndex;
            }
            public static int SEDGE_SIZE
            {
                get
                {
                    return sizeof(bool) +
                      sizeof(float) * 3 +
                      sizeof(int);
                }
            }

            public struct SNODE
            {
                bool used;
                Vector3 position;
                bool active;
            }
            public static int SNODE_SIZE
            {
                get
                {
                    return sizeof(bool) +
                      sizeof(float) * 3 +
                      sizeof(bool);
                }
            }

            public struct SCUBE
            {
                SNODE topLeftBack;
                SNODE topRightBack;
                SNODE bottomLeftBack;
                SNODE bottomRightBack;

                SNODE topLeftFront;
                SNODE topRightFront;
                SNODE bottomLeftFront;
                SNODE bottomRightFront;

                SEDGE centerTopBack;
                SEDGE centerRightBack;
                SEDGE centerBottomBack;
                SEDGE centerLeftBack;

                SEDGE centerTopLeft;
                SEDGE centerTopRight;
                SEDGE centerBottomLeft;
                SEDGE centerBottomRight;

                SEDGE centerTopFront;
                SEDGE centerRightFront;
                SEDGE centerBottomFront;
                SEDGE centerLeftFront;
            }

            public static int SCUBE_MC_SIZE
            {
                get
                {
                    return SNODE_SIZE * 8 +
                        SEDGE_SIZE * 12;
                }
            }
        }

        public class MISC
        {
            public enum ESymmetry : uint
            {
                /*
                 * Symmytry always counts for the point where arms split
                 * 
                 * m : relevant node
                 * n : effected node
                 * x : point of symmetry
                 * 
                 *         m
                 *         o
                 *       o x o o
                 *         o
                 *         n
                 *----------------------
                 *         o
                 *         o     m
                 *       o o o o x
                 *         o     n
                 */
                EOff, //no symmetry
                EMirror, //mirror symmetry (left/right) (up/down)
                EPointMirror, //point symmetry for the two opposing arms
                EPoint //point symmetry for all 4 arms
            }
        }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMaths
{
    public enum EPlane
    {
        E_XY,
        E_XZ,
        E_YZ
    };

    /** Returns the delta angle between to angles */
    public static float FindDeltaAngleDegrees(float A1, float A2)
    {
        float delta = A2 - A1;

        if (delta > 180.0f)
        {
            delta = delta - 360.0f;
        }
        else if (delta < 180.0f)
        {
            delta = delta + 360.0f;
        }

        return delta;
    }

    /** Determines Angle between two vectors
	* @param start Starting vector
	* @param target Target vector
	* @param angle The resulting angle
	*
	* @return True if the angle can be calculated
	*/
    public static float FindLookAtAngle2D(Vector3 start, Vector3 target)
    {
        Vector3 normal = (target - start);
        normal.Normalize();
        float angle = 0f;
        if (!IsNearlyZero(normal))
        {
            angle = Mathf.Atan2(start.y, start.x) - Mathf.Atan2(target.y, target.x);
            angle = angle + Mathf.Rad2Deg;
        }

        return angle;
    }

    public static bool IsNearlyZero(Vector3 a)
    {
        return Vector3.Magnitude(a) <= 0.0001f;
    }

    public static Vector2 ThreeDTo2D(Vector3 vector, EPlane plane)
    {
        if (plane == EPlane.E_XY)
        {
            return new Vector2(vector.x, vector.y);
        }
        else if (plane == EPlane.E_XZ)
        {
            return new Vector2(vector.x, vector.z);
        }
        else if (plane == EPlane.E_YZ)
        {
            return new Vector2(vector.y, vector.z);
        }

        return Vector2.zero;
    }

    public static float Distance2D(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2)));
    }

    public static float Distance2D(Vector3 a, Vector3 b)
    {
        return Distance2D(ThreeDTo2D(a, EPlane.E_XY), ThreeDTo2D(b, EPlane.E_XY));
    }

    /** Returns whether the given value lies between the other two */
    public static bool Between(float value, float a, float b)
    {
        if ((value >= a && value <= b) || (value >= b && value <= a))
            return true;

        return false;
    }

    /** Return whether a point lies within or on a sphere or not */
    public static bool InsideSphere(float _radius, Vector3 _sphereCenter, Vector3 _point)
    {
        float leftSideOfEquation = Mathf.Pow(_point.x - _sphereCenter.x, 2)
                                + Mathf.Pow(_point.y - _sphereCenter.y, 2)
                                + Mathf.Pow(_point.z - _sphereCenter.z, 2);
        float rightSideOfEquation = Mathf.Pow(_radius, 2);

        return (leftSideOfEquation < rightSideOfEquation || leftSideOfEquation == rightSideOfEquation);
    }

    public static Vector3 ProjectOntoSphere(float _radius, Vector3 _sphereCenter, Vector3 _point)
    {
        return _sphereCenter + ((_point - _sphereCenter).normalized * _radius);
    }
}

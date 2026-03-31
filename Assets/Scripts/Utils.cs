using UnityEngine;

public static class Utils
{
    public static float RandomInRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }

    public static float GetMidPoint(Vector2 v)
    {
        return (v.y + v.x) / 2;
    }

    public static Vector3 GetDomainMidPoint(Vector2 x, Vector2 y, Vector2 z)
    {
        return new(GetMidPoint(x), GetMidPoint(y), GetMidPoint(z));
    }
}

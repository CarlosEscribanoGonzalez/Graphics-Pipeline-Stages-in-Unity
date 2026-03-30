using UnityEngine;

public static class Utils
{
    public static float RandomInRange(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }
}

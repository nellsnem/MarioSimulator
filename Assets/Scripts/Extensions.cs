using UnityEngine;

public static class Extensions
{
    private static LayerMask layerMask = LayerMask.GetMask("Default");

    public static bool Raycast(this Rigidbody2D rb, Vector2 direction)
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic) {
            return false;
        }

        float radius = 0.25f;
        float distance = 0.375f;

        RaycastHit2D hit = Physics2D.CircleCast(rb.position, radius, direction.normalized, distance, layerMask);
        return hit.collider != null && hit.rigidbody != rb;
    }

    public static bool DotTest(this Transform transform, Transform other, float dotThreshold)
    {
        Vector2 direction = transform.position - other.position;
        return Vector2.Dot(direction.normalized, other.up) > dotThreshold;
    }
}
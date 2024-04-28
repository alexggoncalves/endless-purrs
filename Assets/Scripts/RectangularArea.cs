using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class RectangularArea : MonoBehaviour
{
    [SerializeField]
    Vector2 size;
    [SerializeField]
    Vector2 offset;
    [SerializeField]
    Color wireColor;

    void OnDrawGizmos()
    {
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(transform.position + new Vector3(offset.x,0,offset.y), new Vector3(size.x,4,size.y));
    }

    public void Initialize(float width, float height, Vector2 offset, Color wireColor)
    {
        this.size = new Vector2(width, height);
        this.offset = offset;
        this.wireColor = wireColor;
    }

    public bool Contains(Vector2 point)
    {
        float minX = transform.position.x + offset.x - size.x / 2;
        float maxX = transform.position.x + offset.x + size.x / 2;
        float minZ = transform.position.z + offset.y - size.y / 2;
        float maxZ = transform.position.z + offset.y + size.y / 2;

        return (point.x >= minX && point.x <= maxX && point.y >= minZ && point.y <= maxZ);
    }

    public bool CollidesWith(RectangularArea other)
    {
        bool overlapX = Mathf.Abs(transform.position.x - other.transform.position.x) * 2 < (size.x + other.size.x);
        bool overlapY = Mathf.Abs(transform.position.y - other.transform.position.y) * 2 < (size.y + other.size.y);

        return overlapX && overlapY;
    }

    public bool CollidesWith(float x, float y, float width, float height)
    {
        bool overlapX = Mathf.Abs(transform.position.x - x) * 2 < (size.x + width);
        bool overlapY = Mathf.Abs(transform.position.y - y) * 2 < (size.y + height);

        return overlapX && overlapY;
    }

    public Vector2 GetOffset()
    {
        return this.offset;
    }

    public float GetCellArea(float scale)
    {
        return size.x/scale * size.y/scale;
    }
}

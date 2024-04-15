using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class RectangularArea : MonoBehaviour
{
    [SerializeField]
    Vector2 size;
    [SerializeField]
    Vector2 offset;
    [SerializeField]
    UnityEngine.Color wireColor;

    void OnDrawGizmos()
    {
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(transform.position + new Vector3(offset.x,0,offset.y), new Vector3(size.x,4,size.y));
    }

    public void Initialize(float width, float height, Vector2 offset, UnityEngine.Color wireColor)
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

    public Vector2 GetOffset()
    {
        return this.offset;
    }

    public float GetArea()
    {
        return size.x * size.y;
    }
}

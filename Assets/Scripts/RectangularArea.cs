using UnityEngine;

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
        return Contains(point.x, point.y);
    }

    public bool Contains(float x, float z)
    {
        float minX = transform.position.x + offset.x - size.x / 2;
        float maxX = transform.position.x + offset.x + size.x / 2;
        float minZ = transform.position.z + offset.y - size.y / 2;
        float maxZ = transform.position.z + offset.y + size.y / 2;

        return (x >= minX && x <= maxX && z >= minZ && z <= maxZ);
    }

    public bool CollidesWith(float x, float y, float width, float height, float margin)
    {
        Rect placeRect = new(
            transform.position.x + offset.x - size.x / 2 - margin / 2,
            transform.position.z + offset.y - size.y / 2 - margin / 2,
            size.x + margin * 2,
            size.y + margin * 2
        );

        Rect checkRect = new(
            x - width / 2,
            y - height / 2,
            width,
            height
        );

        return placeRect.Overlaps(checkRect);
    }

    public bool CollidesWithOrigin(float x, float y, float width, float height, float margin)
    {
        Rect areaRect = new(
            offset.x - size.x / 2 - margin / 2,
            offset.y - size.y / 2 - margin / 2,
            size.x + margin * 2,
            size.y + margin * 2
        );

        Rect checkRect = new(
            x - width / 2,
            y - height / 2,
            width,
            height
        );

        return areaRect.Overlaps(checkRect);
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

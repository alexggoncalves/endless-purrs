using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class RectangularArea : MonoBehaviour
{
    [SerializeField]
    float width, height;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(width,4,height));
    }

    public void Initialize(float width, float height)
    {
        this.width = width;
        this.height = height;
    }

    public bool Contains(Vector2 point)
    {
        float minX = transform.position.x - width / 2;
        float maxX = transform.position.x + width / 2;
        float minZ = transform.position.z - height / 2;
        float maxZ = transform.position.z + height / 2;

        return (point.x >= minX && point.x <= maxX && point.y >= minZ && point.y <= maxZ);
    }
}

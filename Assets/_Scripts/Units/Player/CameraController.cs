using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new(0, 15, -13);
    public float smoothTime = 0.65f;

    private Vector3 currentVelocity;

    private bool smoothingPaused = false;

    private void LateUpdate()
    {
        if (target)
        {
            if (!smoothingPaused)
            {
                transform.position = Vector3.SmoothDamp(
                     transform.position,
                     target.position + offset,
                     ref currentVelocity,
                     smoothTime
                     );
            }
            else transform.position = target.position + offset;

        }
    }

    public void PauseSmoothing() => smoothingPaused = true;

    public void ResumeSmoothing() => smoothingPaused = false;
}

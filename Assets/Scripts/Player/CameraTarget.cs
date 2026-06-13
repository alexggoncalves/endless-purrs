using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float lookAheadDistance = 2f;
    [SerializeField] float smoothTime = 0.2f;

    private Vector3 currentVelocity;
    private PlayerController playerController;

    void Start()
    {
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetPos = player.position;

        // If the player is moving, shift the target position ahead
        if (playerController != null && playerController.IsMoving())
        {
            // Use the movement direction from the player controller
            // We can also use CharacterController.velocity for more accuracy
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null && cc.velocity.magnitude > 0.1f)
            {
                Vector3 velocity = cc.velocity;
                velocity.y = 0; // Focus on horizontal movement
                targetPos += velocity.normalized * lookAheadDistance;
            }
        }

        // Smoothly move the CameraTarget (PointAhead) towards the calculated target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }
}

using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] float lookAheadDistance = 2f;
    [SerializeField] float smoothTime = 0.2f;

    private Vector3 currentVelocity;
    private PlayerController player;

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    private void Update()
    {
        if (player == null) return;

        // If the player is moving, shift the target position ahead
        if (player.IsMoving())
        {
            Vector3 targetPos = new(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector2 movementDirection = player.GetMovementDirection();

            // Use the movement direction from the player controller
            if (movementDirection.magnitude > 0.1f)
            {
                Vector3 direction = new(movementDirection.x, 0, movementDirection.y);
                targetPos += direction * lookAheadDistance;
            }

            // Smoothly move the CameraTarget (PointAhead) towards the calculated target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }
        // Center the target on the player he's not free to move (teleporting, locked or acting)
        else if (player.State != PlayerState.Free)
        {
            Vector3 targetPos = new(player.transform.position.x, transform.position.y, player.transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Walking : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer = true;

    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    float playerSpeed = 3.5f;
    [SerializeField]
    Animator animator;

    Vector2 gridDimensions = new Vector2(2,2);
    RectangularArea innerPlayerArea;
    RectangularArea outterPlayerArea;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
          
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        controller.Move(move.normalized * Time.deltaTime * playerSpeed);

        animator.SetFloat("Velocity", move.magnitude);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void SetMapDetails(int gridWidth, int gridHeight, float cellScale)
    {
        this.gridDimensions = new Vector2(gridWidth, gridHeight);

        innerPlayerArea = GetComponent<RectangularArea>();
        outterPlayerArea = this.AddComponent<RectangularArea>();
        outterPlayerArea.Initialize(gridDimensions.x * cellScale, gridDimensions.y * cellScale, innerPlayerArea.GetOffset(), UnityEngine.Color.magenta);
    }

    public Vector2 GetPlayerGridCoordinates()
    {
        return new Vector2(Mathf.RoundToInt((transform.position.x / 2 + (gridDimensions.x / 2))), Mathf.RoundToInt((transform.position.z / 2 + (gridDimensions.y / 2))));
    }

    public RectangularArea GetInnerPlayerArea()
    {
        return innerPlayerArea;
    }

    public RectangularArea GetOutterPlayerArea()
    {
        return outterPlayerArea;
    }
}
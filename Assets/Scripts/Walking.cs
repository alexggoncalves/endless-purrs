using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
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

    float walkedDistance;

    Boolean locked;

    private AudioSource footStep;

    private void Start()
    {
        locked = true;
        controller = GetComponent<CharacterController>();
        walkedDistance = 0;
        footStep = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!locked)
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
                footStep.enabled = true;
            } else
            {
                footStep.enabled = false;
            }

            // Changes the height position of the player..
            if (Input.GetButtonDown("Jump") && groundedPlayer)
            {
                footStep.enabled = false;
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            walkedDistance += (move.normalized * Time.deltaTime * playerSpeed).magnitude;
        }
    }

    public void SetMapDetails(int gridWidth, int gridHeight, float cellScale, Vector2 mapOffset, Vector2 edgeSize)
    {
        this.gridDimensions = new Vector2(gridWidth, gridHeight);

        innerPlayerArea = this.AddComponent<RectangularArea>();
        innerPlayerArea.Initialize((gridDimensions.x - edgeSize.x * cellScale) * cellScale, (gridDimensions.y - edgeSize.y * cellScale) * cellScale, mapOffset, UnityEngine.Color.green);
        outterPlayerArea = this.AddComponent<RectangularArea>();
        outterPlayerArea.Initialize(gridDimensions.x * cellScale, gridDimensions.y * cellScale, mapOffset, UnityEngine.Color.magenta);
    }

    public Vector2 GetPlayerWorldCoordinates()
    {
        int x = Mathf.RoundToInt((transform.position.x / 2 + (gridDimensions.x / 2)) + innerPlayerArea.GetOffset().x); 
        int y = Mathf.RoundToInt((transform.position.z / 2 + (gridDimensions.y / 2)) + innerPlayerArea.GetOffset().y);
        return new Vector2( x,y);
    }

    public RectangularArea GetInnerPlayerArea()
    {
        return innerPlayerArea;
    }

    public RectangularArea GetOutterPlayerArea()
    {
        return outterPlayerArea;
    }

    public float GetWalkedDistance()
    {
        return walkedDistance;
    }

    public void LockMovement()
    {
        locked = true;
    }
    public void UnlockMovement()
    {
        locked = false;
    }

    public void ResetVelocity()
    {
        playerVelocity = Vector3.zero;
    }
}
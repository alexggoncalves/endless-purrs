using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float jumpHeight = 1.0f;

    [SerializeField]
    private float rotationSpeed = 400;


    [SerializeField]
    float maxSpeed = 6f;
    
    [SerializeField]
    float jumpDebouncePeriod = 0.2f;

    /*[SerializeField]
    float jumpHorizontalSpeed = 2f;*/

    [SerializeField]
    Animator animator;

    private CharacterController controller;

    float ySpeed = 0;
    private float? jumpButtonPressedTime = 0;
    private float? lastGroundedTime = 0;
    private bool isGrounded;
    private bool isJumping;
    private bool isTeleporting = false;
    
    bool locked;
    bool isInsideHouse = true;
    float walkedDistance;

    private RoofController roofController;

    private AudioSource[] footStep;

    Vector2 gridDimensions = new(2, 2);

    public TileType currentTileType;
    public MapGenerator mapGenerator;

    private void Start()
    {
        locked = true;
        controller = GetComponent<CharacterController>();
        footStep = gameObject.GetComponents<AudioSource>();
        roofController = GameObject.Find("HouseTop").GetComponent<RoofController>();
        walkedDistance = 0;
    }

    void Update()
    {
       

        if (!locked)
        {
            Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            
            float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);
            if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                inputMagnitude *= 0.66f;
            }
            animator.SetFloat("Velocity", inputMagnitude, 0.05f, Time.deltaTime);
            movementDirection.Normalize();

            ySpeed += Physics.gravity.y * Time.deltaTime;

            
            if (controller.isGrounded)
            {
                lastGroundedTime = Time.time;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpButtonPressedTime = Time.time;
            }

            if (Time.time - lastGroundedTime <= jumpDebouncePeriod)
            {
                ySpeed = -0.5f;
                animator.SetBool("IsGrounded", true);
                isGrounded = true;
                animator.SetBool("IsJumping", false);
                isJumping = false;
                animator.SetBool("IsFalling", false);

                if (Time.time - jumpButtonPressedTime <= jumpDebouncePeriod)
                {
                    ySpeed += Mathf.Sqrt(jumpHeight * -3.0f * Physics.gravity.y);
                    lastGroundedTime = null;
                    jumpButtonPressedTime = null;
                    animator.SetBool("IsJumping", true);
                    isJumping = true;
                }
            }
            else
            {
                animator.SetBool("IsGrounded", false);
                isGrounded = false;
                footStep[0].enabled = false;
                footStep[1].enabled = false;
                footStep[2].enabled = false;

                if ((isJumping && ySpeed < 0) || ySpeed < -3f)
                {
                    animator.SetBool("IsFalling", true);
                }
            }

               
            if (movementDirection != Vector3.zero)
            {
                animator.SetBool("IsMoving", true);

                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

                if (roofController != null)
                {
                    bool isPlayerInside = roofController.IsPlayerInsideCheck;

                    if (isPlayerInside)
                    {
                        footStep[1].enabled = true;
                        footStep[0].enabled = false;
                    }
                    else
                    {
                        footStep[0].enabled = true;
                        footStep[1].enabled = false;
                    }
                }

                if (controller.transform.position.y <= -0.3)
                {
                    footStep[0].enabled = false;
                    footStep[2].enabled = true;
                    footStep[2].Play();
                }
            } else
            {
                animator.SetBool("IsMoving", false);
                footStep[0].enabled = false;
                footStep[1].enabled = false;
                footStep[2].enabled = false;
            }

          
            Vector3 velocity = movementDirection * inputMagnitude * maxSpeed;
            velocity.y = ySpeed;

            controller.Move(velocity * Time.deltaTime);


            walkedDistance += inputMagnitude * maxSpeed * Time.deltaTime;
            UpdateCurrentTile();
        }
    }

    public void UpdateCurrentTile() {
        if (!mapGenerator.GetWFC().initialLoading)
        {
            Vector2 gridCoordinates = mapGenerator.GetWFC().CalculateGridCoordinates(transform.position.x, transform.position.z);
            int tileID = mapGenerator.GetWFC().grid[(int)gridCoordinates.x, (int)gridCoordinates.y].tileOptions[0];
            currentTileType = mapGenerator.GetWFC().tileLoader.GetTileByID(tileID).tileType;
            Debug.Log(currentTileType);
        }
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

    public void EnterHouse()
    {
        isInsideHouse = true;
    }
    
    public void LeaveHouse()
    {
        isInsideHouse = false;
    }

    public bool IsInsideHouse()
    {
        return isInsideHouse;
    }

    public bool IsTeleporting()
    {
        return isTeleporting;
    }

    public void SetTeleporting(bool isTeleporting)
    {
        this.isTeleporting = isTeleporting;
    }

    public bool IsGrounded() { return isGrounded; } 
}
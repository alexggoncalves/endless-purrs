using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    // Movement
    private CharacterController controller;

    Vector3 movementDirection;
    float inputMagnitude;
    [SerializeField]
    float maxSpeed = 6f;
    [SerializeField]
    private float rotationSpeed = 400;
    [SerializeField]
    float waterY = -0.3f;

    bool locked;
    public bool isMoving;
    bool isInsideHouse = true;
    private bool isTeleporting = false;
    float walkedDistance;


    // Jump
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    float jumpDebouncePeriod = 0.2f;
    float ySpeed = 0;
    private float? jumpButtonPressedTime = 0;
    private float? lastGroundedTime = 0;
    private bool isGrounded;
    private bool isJumping;


    // Animations
    [SerializeField]
    Animator animator;
    private float actionTime;
    private bool busy = false;

    public bool isOpeningPortal;
    private float openPortalDuration = 1.7f;
    public bool isPettingCat;
    public float pettingCatDuration = 6.7f;
    public Vector3 catTarget;

    // Sound
    private AudioSource[] footStep;

    //Other
    private RoofController roofController;
    public MapGenerator mapGenerator;
    private GameObject activePortalInstance;
    public GameObject portalObj;
    public Game game;

   
    private void Start()
    {
        locked = true;
        controller = GetComponent<CharacterController>();
        footStep = gameObject.GetComponents<AudioSource>();
        game = GameObject.Find("Game").GetComponent<Game>();
        roofController = GameObject.Find("HouseTop").GetComponent<RoofController>();
        walkedDistance = 0;
    }

    void Update()
    {
        if (!locked)
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

            // If leftShift is not being pressed limit the player speed
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                inputMagnitude *= 0.66f;
            }

            // Set velocity parameter for the animator
            animator.SetFloat("Velocity", inputMagnitude, 0.05f, Time.deltaTime);
            movementDirection.Normalize();

            // Add gravity to movement
            ySpeed += Physics.gravity.y * Time.deltaTime;

            // Jump
            HandleJump();

            // Apply movement
            Move();

            // Spawn Portal
            HandlePortalSpawn();

            // Pet Cat
            HandlePetCat();
        }
        else if (isOpeningPortal)
        {
            // Rotate towards portal
            Vector3 targetPosition = new Vector3(activePortalInstance.transform.position.x, 0, activePortalInstance.transform.position.z);
            Quaternion toRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            if (Time.time - actionTime > openPortalDuration)
            {
                isOpeningPortal = false;
                locked = false;
                busy = false;
            }
        }
        else if (isPettingCat)
        {
            // Rotate towards cat
            Vector3 targetPosition = new Vector3(catTarget.x, 0, catTarget.z);
            Quaternion toRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            if (Time.time - actionTime > pettingCatDuration)
            {
                isPettingCat = false;
                locked = false;
                busy = false;
            }
        }


    }

    private void Move()
    {
        if (movementDirection != Vector3.zero)
        {
            isMoving = true;
            animator.SetBool("IsMoving", isMoving);

            // Apply Rotation
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Apply velocity
            Vector3 velocity = movementDirection * inputMagnitude * maxSpeed;
            velocity.y = ySpeed;
            controller.Move(velocity * Time.deltaTime);


            //Play Novement Sound
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

            // Play water sound when under water height
            if (controller.transform.position.y <= waterY)
            {
                footStep[0].enabled = false;
                footStep[2].enabled = true;
                footStep[2].Play();
            }

            // Update walked distance and the tile the player is walking on
            walkedDistance += inputMagnitude * maxSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("IsMoving", false);
            isMoving = false;

            // Stop step sounds when there's no movement
            footStep[0].enabled = false;
            footStep[1].enabled = false;
            footStep[2].enabled = false;
        }
    }

    public void HandleJump()
    {
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
    }

    private void HandlePortalSpawn()
    {
        if (isGrounded && !busy)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Perform a raycast to detet hits on tiles, 
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tiles"))
                    {
                        if (activePortalInstance != null) { Destroy(activePortalInstance);}
                        /*Debug.Log("SpawnPortalAt: " + hit.point);*/
                        activePortalInstance = Instantiate(portalObj, hit.point + Vector3.up * 2, Quaternion.identity);

                        
                        animator.SetBool("IsMoving", false);
                        animator.SetTrigger("OpenPortal");

                        actionTime = Time.time;

                        isMoving = false;
                        isOpeningPortal = true;
                        locked = true;
                        busy = true;
                    }
                }
            }
        }
    }

    private void HandlePetCat()
    {
        if (isGrounded && !busy)
        {
            if (Input.GetKey(KeyCode.E)){
                if(game.followers != null)
                {

                    // Find the closest cat and if he is in reach pet him
                    CatController closest = null;
                    float distance = Mathf.Infinity;
                    Vector3 position = transform.position;
                    
                    foreach (GameObject go in game.followers)
                    {
                        Vector3 diff = go.transform.position - position;
                        float curDistance = diff.sqrMagnitude;
                        if (curDistance < distance
                            && !go.GetComponent<CatIdentity>().behaviour.Equals(BehaviourType.Scaredy)
                            )
                        {
                            if (curDistance < 3)
                            {
                                closest = go.GetComponent<CatController>();
                                distance = curDistance;
                            }
                           
                        }
                    }

                    if (closest != null)
                    {
                        catTarget = closest.transform.position;

                        animator.SetBool("IsMoving", false);
                        animator.SetTrigger("PetCat");

                        actionTime = Time.time;

                        isMoving = false;
                        isPettingCat = true;
                        locked = true;
                        busy = true;

                        //Lock cat's movement??
                    }
                }
            }
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
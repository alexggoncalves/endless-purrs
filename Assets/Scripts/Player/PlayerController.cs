using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Free,
    Acting,
    Teleporting,
    Locked
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --- Inspector -------------------------------------
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float rotationSpeed = 400;
    [SerializeField] private float waterY = -0.3f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float jumpDebouncePeriod = 0.2f;
    [SerializeField] private float distancePerStep = 1.8f;
    [SerializeField] private Animator animator;

    // --- References ------------------------------------
    public MapGenerator mapGenerator;
    public Game game;
    private RoofController roofController;

    // --- State -----------------------------------------
    public PlayerState State { get; private set; } = PlayerState.Free;
    public bool IsFree => State == PlayerState.Free;
    public bool IsActing => State == PlayerState.Acting;
    public bool IsTeleporting => State == PlayerState.Teleporting;
    public bool IsLocked => State == PlayerState.Locked;

    // --- Movement --------------------------------------
    private CharacterController controller;
    private Vector2 movementDirection;
    private float inputMagnitude;
    private bool isMoving;
    private bool isInsideHouse;

    // --- Jump -----------------------------------------
    private float ySpeed;
    private float? jumpButtonPressedTime = null;
    private float? lastGroundedTime = null;
    private bool isGrounded;
    private bool isJumping;

    // --- Sound ----------------------------------------
    private enum FootstepSurface { Outdoor = 0, Indoor = 1, Water = 2 }
    private AudioSource[] footstepSources;
    private float distanceSinceLastStep = 0f;

    // --- Animation Hashes -----------------------------
    private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int VelocityHash = Animator.StringToHash("Velocity");

    // --- Input ----------------------------------------
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        footstepSources = gameObject.GetComponents<AudioSource>();
        game = Object.FindAnyObjectByType<Game>();

        // Find input actions
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");

        // Find house top
        GameObject houseTop = GameObject.Find("HouseTop");
        if (houseTop != null)
            roofController = houseTop.GetComponent<RoofController>();

        // Set up sound sources and step distance
        distanceSinceLastStep = distancePerStep;
        foreach (AudioSource source in footstepSources)
        {
            if (source == null) continue;
            source.loop = false;
            source.playOnAwake = false;
            source.enabled = true;
        }
    }

    void Update()
    {
        if (!IsFree) return;

        movementDirection = moveAction.ReadValue<Vector2>();
        inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        // If leftShift is not being pressed limit the player speed
        if (!sprintAction.IsPressed())
        {
            inputMagnitude *= 0.66f;
        }

        // Set velocity parameter for the animator
        animator.SetFloat(VelocityHash, inputMagnitude, 0.05f, Time.deltaTime);
        movementDirection.Normalize();

        // Add gravity to movement
        ySpeed += Physics.gravity.y * Time.deltaTime;

        HandleJump();
        Move();
    }

    private void Move()
    {
        Vector3 velocity = Vector3.zero;

        if (movementDirection != Vector2.zero)
        {
            isMoving = true;
            animator.SetBool(IsMovingHash, true);

            // Apply Rotation
            Quaternion toRotation = Quaternion.LookRotation(new Vector3(movementDirection.x, 0f, movementDirection.y), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Apply velocity
            velocity = inputMagnitude * maxSpeed * new Vector3(movementDirection.x, 0f, movementDirection.y);

            // Update walked distance and the tile the player is walking on
            float deltaDist = inputMagnitude * maxSpeed * Time.deltaTime;

            // Trigger timed footstep sounds when grounded
            if (isGrounded)
            {
                distanceSinceLastStep += deltaDist;
                if (distanceSinceLastStep >= distancePerStep)
                {
                    PlayFootstepSound();
                    distanceSinceLastStep = 0f;
                }
            }
        }
        else
        {
            isMoving = false;
            animator.SetBool(IsMovingHash, false);
            distanceSinceLastStep = distancePerStep; // Ready to step instantly on next move
        }

        // Apply gravity
        velocity.y = ySpeed;

        // Move player
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && ySpeed > 0f)
            ySpeed = 0f;

        if (controller.isGrounded)
            lastGroundedTime = Time.time;

        if (jumpAction.IsPressed())
        {
            jumpButtonPressedTime = Time.time;
        }

        bool recentlyGrounded = lastGroundedTime.HasValue && Time.time - lastGroundedTime.Value <= jumpDebouncePeriod;

        if (recentlyGrounded)
        {
            ySpeed = -0.5f;
            isGrounded = true;
            isJumping = false;
            animator.SetBool(IsGroundedHash, true);
            animator.SetBool(IsJumpingHash, false);
            animator.SetBool(IsFallingHash, false);

            bool recentJumpPress = jumpButtonPressedTime.HasValue && Time.time - jumpButtonPressedTime.Value <= jumpDebouncePeriod;

            if (recentJumpPress)
            {
                ySpeed += Mathf.Sqrt(jumpHeight * -3.0f * Physics.gravity.y);
                lastGroundedTime = null;
                jumpButtonPressedTime = null;
                isJumping = true;
                animator.SetBool(IsJumpingHash, true);
            }
        }
        else
        {
            isGrounded = false;
            animator.SetBool(IsGroundedHash, false);

            if ((isJumping && ySpeed < 0f) || ySpeed < -3f)
            {
                animator.SetBool(IsFallingHash, true);
            }
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepSources == null) return;

        FootstepSurface surface;

        // Play water sound when under water height
        if (controller.transform.position.y <= waterY)
        {
            surface = FootstepSurface.Water;
        }
        // Play wood sound when inside the house
        else if (roofController != null && roofController.IsPlayerInsideCheck)
        {
            surface = FootstepSurface.Indoor;
        }
        // Play grass sound when outside
        else
        {
            surface = FootstepSurface.Outdoor;
        }

        int index = (int)surface;
        if (index < footstepSources.Length && footstepSources[index] != null)
            footstepSources[index].Play();
    }

    /// <summary>Transitions the player to a new state.</summary>
    public void SetState(PlayerState newState) => State = newState;

    /// <summary>Returns true if the player is currently grounded.</summary>
    public bool IsGrounded() => isGrounded;

    /// <summary>Returns true if the player is currently moving.</summary>
    public bool IsMoving() => isMoving;

    /// <summary>Returns true if the player is inside a house.</summary>
    public bool IsInsideHouse() => isInsideHouse;
}
using System.Collections;
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
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float jumpDebouncePeriod = 0.2f;
    [SerializeField] private Animator animator;
    [SerializeField] private CameraController cameraController;
    //[SerializeField] private float waterY = -0.3f;

    // --- References ------------------------------------
    private Cloth playerCape;
    private Vector3 spawnPoint;

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

    // --- Jump -----------------------------------------
    private float ySpeed;
    private float? jumpButtonPressedTime = null;
    private float? lastGroundedTime = null;
    private bool isGrounded;
    private bool isJumping;

    // --- Sound ----------------------------------------
    private enum FootstepSurface { Outdoor = 0, Indoor = 1, Water = 2 }

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

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
        else
        {
            Debug.LogError("PlayerController.Awake: GameManager.Instance is null. Check Script Execution Order.");
        }
    }

    private void OnEnable()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCape = GameObject.FindGameObjectWithTag("Cape").GetComponent<Cloth>();
    }

    void Update()
    {
        TryFindSpawnPoint();

        if (!IsFree) return;
        if (moveAction == null) return;

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

    private void TryFindSpawnPoint()
    {
        GameObject spawnPointObject = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPointObject != null)
            spawnPoint = spawnPointObject.transform.position;
        else spawnPoint = Vector3.zero;
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
        }
        else
        {
            isMoving = false;
            animator.SetBool(IsMovingHash, false);
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

    public IEnumerator TeleportToSpawnPoint()
    {
        cameraController.PauseSmoothing();

        if (playerCape != null)
            playerCape.enabled = false;

        transform.SetPositionAndRotation(spawnPoint, Quaternion.Euler(0, 180, 0));

        if (playerCape != null)
            playerCape.enabled = true;

        yield return null;

        cameraController.ResumeSmoothing();
    }

    public void TeleportTo(Vector3 point)
    {
        cameraController.PauseSmoothing();

        if (playerCape != null)
            playerCape.enabled = false;

        transform.position = point;

        if (playerCape != null)
            playerCape.enabled = true;

        cameraController.ResumeSmoothing();
    }

    /// <summary>Transitions the player to a new state.</summary>
    public void SetState(PlayerState newState) => State = newState;

    /// <summary>Returns true if the player is currently grounded.</summary>
    public bool IsGrounded() => isGrounded;

    /// <summary>Returns true if the player is currently moving.</summary>
    public bool IsMoving() => isMoving;

    public Vector2 GetMovementDirection() { return movementDirection; }
}
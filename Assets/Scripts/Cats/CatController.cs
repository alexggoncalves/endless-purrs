using CAC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CatState { None, Following, Wandering, Fleeing, AtHome }

[RequireComponent(typeof(CreateACatGenerator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CatWanderScript))]
public class CatController : MonoBehaviour
{
    private static readonly int SpeedMultiplierHash = Animator.StringToHash("speedMultiplier");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    // --- Global cat registry -------------------------------
    public static List<CatController> AllCats = new();
    void OnEnable() { if (!AllCats.Contains(this)) AllCats.Add(this); }
    void OnDisable() { AllCats.Remove(this); }

    // --- Inspector attributes ------------------------------
    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float fleeDistance = 30f;

    [Header("Senses")]
    [SerializeField] private float visionRange = 10f;

    [Header("Navigation")]
    [SerializeField] private LayerMask navMeshLayerMask;

    // --- References -----------------------------------------
    private MapGenerator mapGenerator;
    private Game game;
    private Transform player;

    // --- Components ------------------------------------------
    private CatIdentity identity;
    private CatWanderScript wander;
    private Animator animator;
    private NavMeshAgent agent;

    // --- Runtime state ---------------------------------------
    private CatState currentState = CatState.None;
    private Place homePlace;
    private bool playerSearchFailed = false;
    private bool isAlertOfPlayer = false;

    // --- Timers ------------------------------------------------
    private float navMeshPollTimer = 0f;
    private float homeCheckTimer = 0f;
    private const float POLL_INTERVAL = 0.2f;

    // --- Nav mesh links --------------------------------------------
    private Vector3 offMeshLinkStartPos;
    private Vector3 offMeshLinkEndPos;
    private bool isTraversingOffMeshLink = false;
    private float offMeshLinkProgression = -1f;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        wander = GetComponent<CatWanderScript>();
        navMeshLayerMask = LayerMask.GetMask("Tiles");

        // Randomize cat and create it's identity
        GetComponent<CreateACatGenerator>().RandomizeCat();
        identity = gameObject.AddComponent<CatIdentity>();

        // Find Map Generator
        var mapGenObj = GameObject.Find("MapGenerator");
        if (mapGenObj != null) mapGenerator = mapGenObj.GetComponent<MapGenerator>();

        // Find Game Manager
        var gameObj = GameObject.Find("GameManager");
        if (gameObj != null) game = gameObj.GetComponent<Game>();

        // Find Player
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Disable nav agent before finding nav surface
        agent.updatePosition = false;
        agent.enabled = false;

        // Enable wandering
        SetWandering();
    }

    void Start()
    {
        TryCacheHomePlace();
    }

    void Update()
    {
        TryFindPlayer();
        TryCacheHomePlace();

        if (!agent.enabled)
        {
            LookForNavMesh();
            return;
        }

        if (player == null) return;

        UpdateSenses();
        UpdateStateTransitions();
        UpdateMovement();
        UpdateHomeCheck();
        HandleOffMeshLinks();
        UpdateAtHomeDeactivation();
    }

    // =========================================================================
    // Senses
    // =========================================================================

    private void TryFindPlayer()
    {
        if (player != null || playerSearchFailed) return;

        var obj = GameObject.FindWithTag("Player");
        if (obj != null)
            player = obj.transform;
        else
            playerSearchFailed = true;
    }

    private void UpdateSenses()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        isAlertOfPlayer = dist < visionRange && !IsAtHome();
    }

    // =========================================================================
    // State machine
    // =========================================================================

    private void UpdateStateTransitions()
    {
        bool isReacting = IsFollowing() || IsFleeing();

        if (isAlertOfPlayer && !isReacting && !IsAtHome()) // If cat is alert of player make it react
            EnterReactiveState();
        else if (isReacting && (!isAlertOfPlayer || IsAtHome())) // Exit reactive state iff cat stops being alert of player or gets home
            ExitReactiveState();
    }

    private void EnterReactiveState()
    {
        wander.enabled = false;
        var behaviour = identity.GetBehaviourType();

        if (behaviour == BehaviourType.Friendly) // If cat is friendly make it follow the player
        {
            SetState(CatState.Following);
            game.AddToFollowers(this);
        }
        else if (behaviour == BehaviourType.Scaredy) // If cat is scaredy makeit flee the player
        {
            SetState(CatState.Fleeing);
        }
    }

    private void ExitReactiveState()
    {
        if (IsFollowing())
            game.RemoveFromFollowers(this);

        SetWandering();
    }

    // =========================================================================
    // Movement
    // =========================================================================

    private void UpdateMovement()
    {
        if (isAlertOfPlayer && !IsAtHome())
        {
            UpdateDestination();
            DriveAnimatorAndPosition();
        }
        else if (IsFollowing() || IsFleeing())
        {
            SetWandering();
        }

        if ((IsWandering() || IsAtHome()) && wander.enabled)
        {
            if (IsAtHome() && homePlace != null)
            {
                // Wander around the house center with a radius based on house dimensions
                Vector2 dims = homePlace.GetDimensions();
                float radius = Mathf.Max(dims.x, dims.y) * 4f; // cellScale is roughly 2-4, so 4f is safe
                wander.UpdateWanderScript(homePlace.transform.position, radius);
            }
            else
            {
                wander.UpdateWanderScript();
            }
        }
    }

    private void UpdateDestination()
    {
        agent.updatePosition = false;

        if (IsFollowing())
        {
            agent.speed = speed;
            agent.destination = player.position;
        }
        else if (IsFleeing())
        {
            agent.speed = fleeSpeed;
            Vector3 fleeDir = (transform.position - player.position).normalized;
            Vector3 fleeDest = transform.position + fleeDir * fleeDistance;

            if (NavMesh.SamplePosition(fleeDest, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
                agent.destination = hit.position;
        }
    }

    private void DriveAnimatorAndPosition()
    {
        bool shouldMove = agent.velocity.magnitude > 0.5f
                       && agent.remainingDistance > agent.stoppingDistance;

        animator.SetBool(IsMovingHash, shouldMove);
        animator.SetFloat(SpeedMultiplierHash, shouldMove ? agent.velocity.magnitude / speed : 0f);

        Vector3 worldDelta = agent.nextPosition - transform.position;
        if (worldDelta.magnitude > agent.radius)
            transform.position = agent.nextPosition - 0.9f * worldDelta;
    }

    void OnAnimatorMove()
    {
        // Only use root motion if we are moving to a destination (Following/Fleeing)
        // Wandering handles its own position update via navMeshAgent.updatePosition = true
        if (!agent.updatePosition && (agent.isOnNavMesh || agent.isOnOffMeshLink))
        {
            Vector3 pos = animator.rootPosition;
            pos.y = agent.nextPosition.y;
            transform.position = pos;
        }
    }

    // =========================================================================
    // NavMesh management
    // =========================================================================

    private void TryCacheHomePlace()
    {
        if (homePlace != null || mapGenerator == null) return;
        var wfc = mapGenerator.GetWFC();
        if (wfc == null || wfc.homeInstance == null) return;
        homePlace = wfc.homeInstance.GetComponent<Place>();
    }

    private void LookForNavMesh()
    {
        navMeshPollTimer -= Time.deltaTime;
        if (navMeshPollTimer > 0) return;
        navMeshPollTimer = POLL_INTERVAL;

        if (!Physics.Raycast(transform.position + Vector3.up * 6, Vector3.down, out _, 7f, navMeshLayerMask))
            return;

        if (!IsAtHome())
        {
            TryEnableAgent();
        }
        else if (player != null && Vector3.Distance(transform.position, player.position) < 40f)
        {
            if (TryEnableAgent())
                SetWandering();
        }
    }

    private bool TryEnableAgent()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            return false;

        agent.enabled = true;
        agent.Warp(navHit.position);
        agent.ResetPath();
        return true;
    }

    private void UpdateHomeCheck()
    {
        if (IsAtHome() || homePlace == null) return;

        homeCheckTimer -= Time.deltaTime;
        if (homeCheckTimer > 0) return;
        homeCheckTimer = POLL_INTERVAL;

        if (homePlace.GetExtents().CollidesWith(transform.position.x, transform.position.z, 1, 1, -5))
        {
            SetState(CatState.AtHome);
            game.RemoveFromFollowers(this);
            game.AddToHome(this);
        }
    }

    private void UpdateAtHomeDeactivation()
    {
        if (!IsAtHome() || Vector3.Distance(transform.position, player.position) <= 40f) return;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.ResetPath();

        agent.enabled = false;
        wander.enabled = false;
    }

    private void HandleOffMeshLinks()
    {
        if (agent.isOnOffMeshLink && !isTraversingOffMeshLink)
        {
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;
            offMeshLinkStartPos = agent.transform.position;
            offMeshLinkEndPos = linkData.endPos + Vector3.up * agent.baseOffset;
            isTraversingOffMeshLink = true;
            agent.speed = 1f;
            agent.autoTraverseOffMeshLink = true;
        }

        if (isTraversingOffMeshLink)
        {
            float total = Vector3.Distance(offMeshLinkStartPos, offMeshLinkEndPos);
            float current = Vector3.Distance(agent.transform.position, offMeshLinkEndPos);
            offMeshLinkProgression = 1f - (current / total);
        }

        if (offMeshLinkProgression >= 1f)
        {
            isTraversingOffMeshLink = false;
            agent.autoTraverseOffMeshLink = false;
            offMeshLinkProgression = -1f;
        }
    }

    public void SetState(CatState state) => currentState = state;
    public void SetWandering()
    {
        if (currentState != CatState.AtHome)
            SetState(CatState.Wandering);

        wander.enabled = true;
        wander.InitializeWanderScript(animator, agent);
    }

    public bool IsAtHome() => currentState == CatState.AtHome;
    public bool IsFollowing() => currentState == CatState.Following;
    public bool IsFleeing() => currentState == CatState.Fleeing;
    public bool IsWandering() => currentState == CatState.Wandering;
    public CatState GetCatState() => currentState;
    public CatIdentity GetIdentity() => identity;
}
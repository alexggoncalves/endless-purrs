using CAC;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum CatState
{
    None,
    Following,
    Wandering,
    Fleeing
}

public class CatController : MonoBehaviour
{
    public static List<CatController> AllCats = new List<CatController>();

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    public Transform target = null;
    
    // Cached components and references
    public CatIdentity identity;
    public AudioSource[] audioSources;
    private CatWanderScript wander;
    private Place homePlace;
    
    private float raycastTimer = 0f;
    private const float RAYCAST_INTERVAL = 0.2f;

    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    public float speed;
    
    public bool randomizeCat = false;

    private Vector3 offMeshLinkStartPos;
    private Vector3 offMeshLinkEndPos;
    private bool isTraversingOffMeshLink = false;
    private float offMeshLinkProgression = -1;

    public float visionRange = 10;
    public float fleeSpeed = 5;
    public float fleeDistance = 30;
    private float stoppingDistance;
    private bool isAlertOfPlayer;
    CatState currentState = CatState.None;
    private bool isAtHome = false;

    public bool owned;
    public string catName = "Nameless";
    public string gender = "Neutral";
    public GameObject identityDisplay;

    public MapGenerator mapGenerator;
    public Game game;

    public LayerMask navMeshLayerMask;

    void OnEnable()
    {
        if (!AllCats.Contains(this))
        {
            AllCats.Add(this);
        }
    }

    void OnDisable()
    {
        AllCats.Remove(this);
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        identity = GetComponent<CatIdentity>();
        if (identity == null) identity = gameObject.AddComponent<CatIdentity>();

        audioSources = GetComponents<AudioSource>();

        GameObject mapGenObj = GameObject.Find("MapGenerator");
        if (mapGenObj != null) mapGenerator = mapGenObj.GetComponent<MapGenerator>();

        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj != null) game = gameManagerObj.GetComponent<Game>();

        stoppingDistance = agent.stoppingDistance;

        if (navMeshLayerMask == 0)
        {
            navMeshLayerMask = LayerMask.GetMask("Tiles");
        }
        
        wander = GetComponent<CatWanderScript>();
if (wander == null) wander = gameObject.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
        // Ensure wander is enabled if starting in Wandering state
        wander.enabled = true;

        currentState = CatState.Wandering;
// Don't update position automatically
        agent.updatePosition = false;
        agent.enabled = false;

        if (randomizeCat && !owned) 
        {
            GetComponent<CreateACatGenerator>().RandomizeCat();
            identity.SetIdentity(identityDisplay);
        } 
        
        if(owned) {
            identity.SetIdentity(catName, gender, BehaviourType.Owned, identityDisplay);
        }
    }

    void Start()
    {
        if (mapGenerator != null && mapGenerator.GetWFC() != null && mapGenerator.GetWFC().homeInstance != null)
        {
            homePlace = mapGenerator.GetWFC().homeInstance.GetComponent<Place>();
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        // Try to find player if not already found
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Try to cache homePlace if not already cached
        if (homePlace == null && mapGenerator != null && mapGenerator.GetWFC() != null && mapGenerator.GetWFC().homeInstance != null)
{
            homePlace = mapGenerator.GetWFC().homeInstance.GetComponent<Place>();
        }

        if (!agent.enabled)
        {
            raycastTimer -= Time.deltaTime;
            if (raycastTimer <= 0)
            {
                raycastTimer = RAYCAST_INTERVAL;
                // Perform a raycast downwards to check for NavMesh
                RaycastHit hit;
                float raycastDistance = 7;

                if (Physics.Raycast(transform.position + Vector3.up * 6, Vector3.down, out hit, raycastDistance, navMeshLayerMask))
                {
                    // Enable the NavMeshAgent if there is NavMesh directly below
                    if (!isAtHome)
                    {
                        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
                        {
                            agent.enabled = true;
                            agent.Warp(navHit.position);
                            agent.ResetPath();
                        }
                    }
                    if (isAtHome && player != null && Vector3.Distance(transform.position, player.position) < 15)
                    {
                        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
{
                            agent.enabled = true;
                            agent.Warp(navHit.position);
                            agent.ResetPath();
                            SetWandering();
                        }
                    }
                }
            }
        }
        else
        {
            if (player == null) return; // Wait for player to be found

            float distToPlayer = Vector3.Distance(player.position, transform.position);
if (distToPlayer < visionRange && !isAtHome)
            {
                isAlertOfPlayer = true;
            }
            else isAlertOfPlayer = false;

            if (isAlertOfPlayer && (currentState != CatState.Fleeing && currentState != CatState.Following) && !isAtHome)
            {
                target = player;

                if (wander != null)
                {
                    wander.enabled = false;
                }
                if (identity.GetBehaviourType() == BehaviourType.Owned || identity.GetBehaviourType() == BehaviourType.Friendly)
                {
                    currentState = CatState.Following;
                    if (game != null) game.AddToFollowers(this);
                }
if (identity.GetBehaviourType() == BehaviourType.Scaredy)
                {
                    currentState = CatState.Fleeing;
                }
            }
            else if (!isAlertOfPlayer || isAtHome)
            {
                if (currentState == CatState.Following)
                {
                    if (game != null) game.RemoveFromFollowers(this);
                }
                target = null;
            }

            // If the cat sees the player and is not at home, follow or flee
            // Else start wandering
            if (isAlertOfPlayer && !isAtHome)
            {
                SetTarget();
                MoveToTarget();
            }
            else if (currentState == CatState.Fleeing || currentState == CatState.Following)
            {
                SetWandering();
            }

            if (currentState == CatState.Wandering && wander != null && wander.enabled)
            {
                wander.UpdateWanderScript();
            }

            // When the cat gets home
            if (!isAtHome && homePlace != null)
            {
                // Only check collision occasionally when not home
                raycastTimer -= Time.deltaTime;
                if (raycastTimer <= 0)
                {
                    raycastTimer = RAYCAST_INTERVAL;
                    if (homePlace.GetExtents().CollidesWith(transform.position.x, transform.position.z, 1, 1, -5))
                    {
                        target = null;
                        isAtHome = true;
                        if (game != null) 
                        {
                            game.RemoveFromFollowers(this);
                            game.AddToHome(this);
                        }
                    }
}
            }

            HandleOffMeshLinks();

            if (isAtHome && distToPlayer > 15)
            {
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }
                agent.enabled = false;
                if (wander != null) wander.enabled = false;
            }
        }
    }

    public void SetWandering()
    {
        currentState = CatState.Wandering;
        if (wander != null)
        {
            wander.enabled = true;
            wander.InitializeWanderScript(animator, agent);
        }
    }

    void HandleOffMeshLinks()
    {
        if (agent.isOnOffMeshLink && !isTraversingOffMeshLink)
        {
            // Initialize the off-mesh link traversal
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;
            offMeshLinkStartPos = agent.transform.position;
            offMeshLinkEndPos = linkData.endPos + Vector3.up * agent.baseOffset;

            isTraversingOffMeshLink = true;
            agent.speed = 1f;
            agent.autoTraverseOffMeshLink = true;
        }

        if (isTraversingOffMeshLink)
        {
            float distanceTotal = Vector3.Distance(offMeshLinkStartPos, offMeshLinkEndPos);
            float distanceCurrent = Vector3.Distance(agent.transform.position, offMeshLinkEndPos);
            offMeshLinkProgression = 1f - (distanceCurrent / distanceTotal);
        }

        if (offMeshLinkProgression >= 1)
        {
            isTraversingOffMeshLink = false;
            agent.autoTraverseOffMeshLink = false;
            offMeshLinkProgression = -1;
        }
    }

    void SetTarget()
    {
        agent.stoppingDistance = stoppingDistance;
        agent.updatePosition = false;
        if (currentState == CatState.Following)
        {
            agent.speed = speed;
            agent.destination = target.position;
        }
        else if (currentState == CatState.Fleeing)
        {
            agent.speed = fleeSpeed;
            Vector3 fleeDirection = (transform.position - target.position).normalized;

            Vector3 fleeDest = transform.position + fleeDirection * fleeDistance;

            if (NavMesh.SamplePosition(fleeDest, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                // Set the NavMeshAgent's destination to the closest valid NavMesh point
                agent.destination = hit.position;
            }
        }
    }

    public CatState GetCatState()
    {
        return currentState;
    }

    public void MoveToTarget()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = agent.velocity.magnitude > 0.5f && agent.remainingDistance > agent.stoppingDistance;

        // Update animation parameters
        animator.SetBool("isMoving", shouldMove);
        if (shouldMove)
        {
            animator.SetFloat("speedMultiplier", agent.velocity.magnitude / agent.speed);
        }
        else
        {
            animator.SetFloat("speedMultiplier", 0);
        }

        // Pull character towards agent
        if (worldDeltaPosition.magnitude > agent.radius)
        {
            transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
        }
    }

    void OnAnimatorMove()
    {
        if ((agent.isOnNavMesh || agent.isOnOffMeshLink))
        {
            Vector3 position = animator.rootPosition;
            position.y = agent.nextPosition.y;
           
            transform.position = position;
        }
    }

    public bool IsAtHome { get {  return isAtHome; } }
    public void SetIsAtHome(bool value)
    {
        this.isAtHome = true;
    }
}

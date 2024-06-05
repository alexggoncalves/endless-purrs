using CAC;
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
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    public Transform target = null;

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

    private CatWanderScript wander;
    
    private CatIdentity identity;
    public bool owned;
    public string catName = "Nameless";
    public string gender = "Neutral";
    public GameObject identityDisplay;

    public MapGenerator mapGenerator;
    public Game game;

    public LayerMask navMeshLayerMask;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        identity = this.AddComponent<CatIdentity>();
        mapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
        game = GameObject.Find("Game").GetComponent<Game>();

        stoppingDistance = agent.stoppingDistance;
        wander = this.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
        currentState = CatState.Wandering;
        // Don’t update position automatically
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

    public void SetTarget(Transform target)
    {
        this.target = target;
    }


    void Update()
    {

        if (!agent.enabled)
        {
            // Perform a raycast downwards to check for NavMesh
            RaycastHit hit;
            float raycastDistance = 7;
            

            if (Physics.Raycast(transform.position + Vector3.up * 6, Vector3.down, out hit, raycastDistance, navMeshLayerMask))
            {
                // Enable the NavMeshAgent if there is NavMesh directly below
                if(!isAtHome)
                {
                    agent.enabled = true;
                    agent.ResetPath();
                    agent.Warp(transform.position); // Adjust the agent's position to stick to the NavMesh
                }
                if(isAtHome && Vector3.Distance(transform.position, player.transform.position) < 15) {
                    agent.enabled = true;
                    agent.ResetPath();
                    agent.Warp(transform.position);
                    SetWandering(); 

                }
                
            }
        }
        else
        {
            if (Vector3.Distance(player.position, transform.position) < visionRange && !isAtHome)
            {
                isAlertOfPlayer = true;
            }
            else isAlertOfPlayer = false;

            if (isAlertOfPlayer && (currentState != CatState.Fleeing || currentState != CatState.Following) && !isAtHome)
            {
                target = player;

                if (wander != null)
                {
                    Destroy(wander);
                    wander = null;
                }
                if (identity.GetBehaviourType() == BehaviourType.Owned || identity.GetBehaviourType() == BehaviourType.Friendly)
                {
                    currentState = CatState.Following;
                    game.AddToFollowers(gameObject);
                }
                if (identity.GetBehaviourType() == BehaviourType.Scaredy)
                {
                    currentState = CatState.Fleeing;
                }
            }
            else
            {
                game.RemoveFromFollowers(gameObject);
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

            if (currentState == CatState.Wandering && wander != null)
            {
                wander.UpdateWanderScript();
            }

            // When the cat get's home
            if (mapGenerator.GetWFC().homeInstance.GetComponent<Place>().GetExtents().CollidesWith(transform.position.x, transform.position.z, 1, 1, -5) && !isAtHome)
            {
                target = null;
                isAtHome = true;
                game.RemoveFromFollowers(gameObject);
                game.AddToHome(gameObject);
            }

            HandleOffMeshLinks();

            if (isAtHome && Vector3.Distance(transform.position, player.transform.position) > 15)
            {
                agent.ResetPath();
                agent.enabled = false;
                wander = null;
            }
        }

        
    }

    public void SetWandering()
    {
        currentState = CatState.Wandering;
        wander = this.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
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

using CAC;

using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CatHouseSystem))]
public class CatMovement : MonoBehaviour
{
    private static readonly int SpeedMultiplierHash = Animator.StringToHash("speedMultiplier");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    [SerializeField] private float speed = 3f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float fleeDistance = 30f;
    [SerializeField] private float followStopDistance = 4f;
    [SerializeField] private float walkAnimBaseSpeed = 3f;

    // --- Timers ------------------------------------------------
    private float navMeshPollTimer = 0f;
    private const float POLL_INTERVAL = 0.2f;

    // --- Nav mesh links --------------------------------------------

    private Vector3 offMeshLinkStartPos;
    private Vector3 offMeshLinkEndPos;
    private bool isTraversingOffMeshLink = false;
    private float offMeshLinkProgression = -1f;

    public NavMeshAgent agent;
    private CatWanderScript wander;
    private Animator animator;
    private CatHouseSystem houseSystem;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        houseSystem = GetComponent<CatHouseSystem>();

        // Add wander script and initialize it 
        wander = gameObject.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
        agent.updatePosition = true;

        // Disable agent before finding navmesh
        agent.enabled = false;
    }

    public bool Tick()
    {
        if (!agent.enabled)
        {
            LookForNavMesh();
            return false;
        }

        if (ShouldDisableAgent())
        {
            Stop();
            agent.enabled = false;
            return false;
        }

        HandleOffMeshLinks();
        UpdateAnimator();

        return true;
    }


    private void UpdateAnimator()
    {
        bool shouldMove =
        agent.velocity.magnitude > 0.1f &&
        agent.remainingDistance > agent.stoppingDistance;

        animator.SetBool(IsMovingHash, shouldMove);

        float normalizedSpeed = shouldMove
            ? agent.velocity.magnitude / walkAnimBaseSpeed
            : 0f;

        // optional: smooth instead of snapping, see below
        animator.SetFloat(SpeedMultiplierHash, normalizedSpeed, 0.15f, Time.deltaTime);
    }

    private bool CanUseAgent()
    {
        return agent.enabled && agent.isOnNavMesh;
    }

    public void Follow(Vector3 target)
    {
        EnableAgent();
        if (!CanUseAgent()) return;

        wander.enabled = false;
        agent.speed = speed;
        agent.stoppingDistance = followStopDistance;

        if (Vector3.Distance(transform.position, target) > followStopDistance)
            agent.destination = target;
        else
            agent.ResetPath();
    }

    public void Flee(Vector3 from)
    {
        EnableAgent();
        if (!CanUseAgent()) return;

        wander.enabled = false;
        Vector3 dir = (transform.position - from).normalized;
        Vector3 target = transform.position + dir * fleeDistance;

        agent.speed = fleeSpeed;
        agent.stoppingDistance = 0f;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            agent.destination = hit.position;
    }

    public void Wander()
    {
        EnableAgent();
        if (!CanUseAgent()) return;

        if (!wander.enabled)
            wander.enabled = true;

        agent.speed = speed;
        agent.stoppingDistance = 0f;

        if (houseSystem.IsAtHome())
        {
            Vector2 dims = houseSystem.GetHouseDimensions();
            float radius = Mathf.Max(dims.x, dims.y) / 2;
            wander.UpdateWanderScript(houseSystem.GetHouseCenter(), radius);
        }
        else
        {
            wander.UpdateWanderScript();
        }
    }

    public void TeleportTo(Vector3 worldPos)
    {
        // Find nearest valid navmesh point near the target
        if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            if (agent.enabled)
            {
                agent.Warp(hit.position);   // correct way to move an active agent
                agent.ResetPath();
            }
            else
            {
                transform.position = hit.position;
            }
        }
        else
        {
            // No navmesh near target yet — move transform and let LookForNavMesh re-acquire
            if (agent.enabled) agent.enabled = false;
            transform.position = worldPos;
        }
    }

    private bool TryEnableAgent()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            return false;

        agent.enabled = true;
        agent.Warp(navHit.position);
        agent.ResetPath();

        wander.enabled = true;
        wander.UpdateWanderScript();

        return true;
    }

    private void LookForNavMesh()
    {
        navMeshPollTimer -= Time.deltaTime;
        if (navMeshPollTimer > 0) return;
        navMeshPollTimer = POLL_INTERVAL;

        TryEnableAgent();
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

    private bool IsOnValidNavMesh()
    {
        return agent.isOnNavMesh;
    }

    private bool IsNearNavMeshEdge(float radius = 2.5f)
    {
        return !NavMesh.SamplePosition(transform.position, out _, radius, NavMesh.AllAreas);
    }

    private bool ShouldDisableAgent()
    {
        return !IsOnValidNavMesh() || IsNearNavMeshEdge(3f);
    }

    public void Stop()
    {
        if (agent.enabled)
            agent.ResetPath();

        if (wander != null)
            wander.enabled = false;
    }

    void EnableAgent()
    {
        if (agent.enabled) return;

        TryEnableAgent();
    }
}

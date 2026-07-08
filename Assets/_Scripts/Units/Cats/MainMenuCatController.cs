using CAC;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CreateACatGenerator))]
public class MainMenuCatController : MonoBehaviour
{
    private static readonly int SpeedMultiplierHash = Animator.StringToHash("speedMultiplier");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    [SerializeField] private float walkAnimBaseSpeed = 3f;

    private CatWanderScript wander;
    private Animator animator;
    private NavMeshAgent agent;

    private void Start()
    {
        GetComponent<CreateACatGenerator>().RandomizeCat();

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;

        wander = gameObject.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
        wander.enabled = true;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            return;
        }

        UpdateAnimator();
        wander.UpdateWanderScript();
    }

    private void UpdateAnimator()
    {
        bool shouldMove =
        agent.velocity.magnitude > 0.1f &&
        !agent.pathPending &&
        agent.remainingDistance > 0.1f;

        animator.SetBool(IsMovingHash, shouldMove);

        float normalizedSpeed = agent.velocity.magnitude / walkAnimBaseSpeed;
        if(normalizedSpeed < 0.0001f) normalizedSpeed = 0f;

        animator.SetFloat(SpeedMultiplierHash, normalizedSpeed, 0.15f, Time.deltaTime);
    }
}
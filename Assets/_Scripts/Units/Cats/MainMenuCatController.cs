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

    private CatWanderScript wander;
    private Animator animator;
    private NavMeshAgent agent;

    private void Start()
    {
        GetComponent<CreateACatGenerator>().RandomizeCat();

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        agent.updatePosition = true;

        wander = gameObject.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);
        wander.enabled = true;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        UpdateAnimator();
        wander.UpdateWanderScript();
    }

    private void UpdateAnimator()
    {
        bool shouldMove =
            agent.velocity.magnitude > 0.1f;

        animator.SetBool(IsMovingHash, shouldMove);

        float normalizedSpeed = shouldMove
            ? agent.velocity.magnitude / agent.speed
            : 0f;

        animator.SetFloat(SpeedMultiplierHash, normalizedSpeed);
    }

    // Kept so that if "Apply Root Motion" is checked, root motion is swallowed
    // and the agent stays the sole driver of position. With updatePosition=true
    // the body never runs — it just prevents root motion fighting the agent.
    void OnAnimatorMove() { }
}
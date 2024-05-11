using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

enum CatState
{
    None,
    Following,
    Wandering,
    Fleeing
}

public class CatController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;

    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    /*private CatState movementState;*/

    public float minDistanceToPlayer = 0.5f;
    Vector3 dest;

    public float speed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        /*agent.isStopped = true;*/
        /*movementState = CatState.None;*/

        // Don’t update position automatically
        agent.updatePosition = false;
    }

    void Update()
    {
        if (agent.isOnNavMesh)
        {
            agent.speed = speed;
            dest = player.position;
            agent.destination = dest;

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

            bool shouldMove = agent.velocity.magnitude > 0f && agent.remainingDistance > agent.radius;

            // Update animation parameters
            animator.SetBool("isMoving", shouldMove);
            animator.SetFloat("speedMultiplier", agent.velocity.magnitude/agent.speed);

            // Pull character towards agent
            if (worldDeltaPosition.magnitude > agent.radius && shouldMove)
                transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
        }

        if (agent.isOnOffMeshLink)
        {
            agent.autoTraverseOffMeshLink = true;
            agent.speed = 1f;
        }
        else agent.autoTraverseOffMeshLink = false;
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = animator.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
    }
}

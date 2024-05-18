using CAC;
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
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    public Transform target = null;

    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    /*private CatState movementState;*/
    Vector3 dest;

    public float speed;
    
    public bool randomizeCat = false;
    public bool randomizeBehaviour;

    private Vector3 offMeshLinkStartPos;
    private Vector3 offMeshLinkEndPos;
    private bool isTraversingOffMeshLink = false;
    private float offMeshLinkProgression = -1;

    public float catVisionRange = 10;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();

        /*movementState = CatState.None;*/

        // Don’t update position automatically
        agent.updatePosition = false;

        if (randomizeCat) 
        {
            this.GetComponent<CreateACatGenerator>().RandomizeCat();
        }
        
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }


    void Update()
    {
        if (agent.isOnNavMesh)
        {
            if(Vector3.Distance(player.position,transform.position) > catVisionRange)
            {
                target = player;
            }

            if (target != null)
            {
                FollowTarget();
            }          

        }

        HandleOffMeshLinks();
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

    void FollowTarget()
    {
        agent.speed = speed;
        dest = target.position;
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

        bool shouldMove = agent.velocity.magnitude > 0.5f && agent.remainingDistance > agent.stoppingDistance;

        // Update animation parameters
        animator.SetBool("isMoving", shouldMove);
        if(shouldMove)
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
        if (agent.isOnNavMesh || agent.isOnOffMeshLink)
        {
            Vector3 position = animator.rootPosition;
            position.y = agent.nextPosition.y;
           
            transform.position = position;
        }
    }
}

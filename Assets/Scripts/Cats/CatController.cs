using CAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;
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

    private CatWanderScript wander;
    private bool isWandering;
    CatIdentity identity;

    public bool owned;
    public string catName = "Nameless";
    public string gender = "Neutral";
    public GameObject identityDisplay;

    

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        identity = this.AddComponent<CatIdentity>();

        stoppingDistance = agent.stoppingDistance;

        isWandering = true;
        wander = this.AddComponent<CatWanderScript>();
        wander.InitializeWanderScript(animator, agent);

        // Don’t update position automatically
        agent.updatePosition = false;

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
        if (agent.isOnNavMesh)
        {
            if(Vector3.Distance(player.position,transform.position) < visionRange)
            {
                target = player;
                isWandering = false;
                if (wander != null)
                {
                    Destroy(wander);
                    wander = null;
                }
            }

            if (target != null)
            {
                SetTarget();
                MoveToTarget();
            } 
            else if (!isWandering)
            {
                target = null;
                isWandering = true;
                wander = this.AddComponent<CatWanderScript>();
                wander.InitializeWanderScript(animator,agent);
            }

            if (isWandering && wander != null)
            {
                wander.UpdateWanderScript();
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

    void SetTarget()
    {
        agent.stoppingDistance = stoppingDistance;
        agent.updatePosition = false;
        if (identity.GetBehaviourType() == BehaviourType.Owned || identity.GetBehaviourType() == BehaviourType.Friendly)
        {
            agent.speed = speed;
            agent.destination = target.position;
        }
        else if (identity.GetBehaviourType() == BehaviourType.Scaredy)
        {
            agent.speed = fleeSpeed;
            Vector3 fleeDirection = (transform.position - target.position).normalized;

            Vector3 fleeDest = transform.position + fleeDirection * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeDest, out hit, fleeDistance, NavMesh.AllAreas))
            {
                // Set the NavMeshAgent's destination to the closest valid NavMesh point
                Debug.Log(hit.position);
                agent.destination = hit.position;

            }


        }
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
}

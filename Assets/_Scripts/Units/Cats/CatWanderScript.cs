using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CAC
{
    /// A runtime class for providing random wander behaviour to cats in scene
    public class CatWanderScript : MonoBehaviour
    {
        private static readonly int LoafHash = Animator.StringToHash("loaf");
        private static readonly int LickPawHash = Animator.StringToHash("lickPaw");
        private static readonly int StretchHash = Animator.StringToHash("stretch");
        private static readonly int SitHash = Animator.StringToHash("sit");

        // How long the agent waits upon arriving at a new location
        private const float WAIT_TIME = 4;
        // The radius within which the agent can find a new destination
        private const float WALK_RADIUS = 6f;

        private Animator animator; // Reference to the cat's animator
        private Coroutine coroutine; // Object representing current move to destination coroutine, if any
        private NavMeshAgent navMeshAgent; // Reference to the cat's nav mesh agent
        private float timer; // Timer count used to perform actions in sequence

        /// Static method returning the random destination a cat should move to
        /// <param name="pos">The starting position</param>
        /// <returns>Vector3 The random destination to be moved to</returns>
        private static bool TryGetRandomDestination(Vector3 center, float radius, out Vector3 result)
        {
            Vector3 randomDirection = center + Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }

        public void InitializeWanderScript(Animator animator, NavMeshAgent navMeshAgent)
        {
            this.animator = animator;
            this.navMeshAgent = navMeshAgent;

            // Random time until first action 
            timer = Random.Range(0, WAIT_TIME);
        }

        private void OnDisable()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public void UpdateWanderScript()
        {
            UpdateWanderScript(transform.position, WALK_RADIUS);
        }

        public void UpdateWanderScript(Vector3 center, float radius)
        {
            if (coroutine == null)
            {
                if (timer >= WAIT_TIME)
                {
                    if (TryGetRandomDestination(center, radius, out Vector3 dest))
                        coroutine = StartCoroutine(MoveToDestinationCoroutine(dest));

                    timer = 0; 
                }
                else
                {
                    timer += Time.deltaTime;
                }
            }
        }

        private IEnumerator MoveToDestinationCoroutine(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);

            float timeout = 15f;
            while (timeout > 0f)
            {
                timeout -= Time.deltaTime;

                if (!navMeshAgent.pathPending &&
                    navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
                    break;

                yield return null;
            }

            while (navMeshAgent.velocity.magnitude > 0.1f)
                yield return null;
            yield return null;

            // Randomly choose an idle animation state, if any
            switch (Random.Range(0, 11))
            {
                case 0:
                    animator.SetTrigger(SitHash);
                    timer -= 3.5f;
                    break;
                case 1:
                    animator.SetTrigger(StretchHash);
                    timer -= 2.5f;
                    break;
                case 2:
                    animator.SetTrigger(LickPawHash);
                    timer -= 3.5f;
                    break;
                case 3:
                    animator.SetTrigger(LoafHash);
                    timer -= 8f;
                    break;
            }

            // Release the stored coroutine
            coroutine = null;
        }
    }
}

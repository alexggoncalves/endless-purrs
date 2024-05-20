using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CAC
{
    /// A runtime class for providing random wander behaviour to cats in scene
    public class CatWanderScript : MonoBehaviour
    {
        // How long the agent waits upon arriving at a new location
        private const float WAIT_TIME = 4;
        // The radius within which the agent can find a new destination
        private const float WALK_RADIUS = 4f;

        private Animator animator; // Reference to the cat's animator
        private Coroutine coroutine; // Object representing current move to destination coroutine, if any
        private NavMeshAgent navMeshAgent; // Reference to the cat's nav mesh agent
        private float timer; // Timer count used to perform actions in sequence

        /// Static method returning the random destination a cat should move to
        /// <param name="pos">The starting position</param>
        /// <returns>Vector3 The random destination to be moved to</returns>
        private static Vector3 GetRandomDestination(Vector3 pos)
        {
            // Get random point within a sphere of walk radius size
            Vector3 randomDirection = Random.insideUnitSphere * WALK_RADIUS;

            // Use vector as a direction added onto the current position
            randomDirection += pos;
            // Find the closest point the nav mesh agent can actually move to
            NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WALK_RADIUS, 1);

            return hit.position;
        }

        public void InitializeWanderScript(Animator animator, NavMeshAgent navMeshAgent)
        {
            this.animator = animator;
            this.navMeshAgent = navMeshAgent;

            // Random time until first action 
            timer = Random.Range(0, WAIT_TIME);
        }

        public void UpdateWanderScript()
        {
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.updatePosition = true;
            if (timer >= WAIT_TIME)
            {
                // Start move to destination coroutine
                if (coroutine == null)
                    coroutine = StartCoroutine(MoveToDestinationCoroutine(GetRandomDestination(transform.position)));

                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        private IEnumerator MoveToDestinationCoroutine(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);

            // Set animator moving bool while sufficiently far enough from end position
            while (Vector3.Distance(transform.position, destination) > 0.25f)
            {
                animator.SetBool("isMoving", true);

                yield return null;
            }

            animator.SetFloat("speedMultiplier", navMeshAgent.velocity.magnitude / navMeshAgent.speed);

            // Randomly choose an idle animation state, if any
            switch (Random.Range(0, 11))
            {
                case 0:
                    animator.SetTrigger("sit");
                    timer -= 3.5f;
                    break;
                case 1:
                    animator.SetTrigger("stretch");
                    timer -= 2.5f;
                    break;
                case 2:
                    animator.SetTrigger("lickPaw");
                    timer -= 3.5f;
                    break;
                case 3:
                    animator.SetTrigger("loaf");
                    timer -= 8f;
                    break;
            }


            

            // Reset the moving bool and release the stored coroutine
            animator.SetBool("isMoving", false);
            coroutine = null;
        }
    }
}

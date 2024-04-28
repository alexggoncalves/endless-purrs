using UnityEngine;

namespace CAC
{
    /// <summary>
    /// Simple class to rotate a cat gameobject at a given speed, and randomize the cat settings every so often
    /// </summary>
    public class CreateACatViewer : MonoBehaviour
    {
        // How often the cat settings will be randomized
        public float randomizeTime = 1.5f;
        // How quickly the cat gameobject will be rotated
        public float rotationSpeed = -0.5f;

        private CreateACatGenerator createACatGenerator;
        private float timer = 0;

        private void Start()
        {
            // Find the cat generator on this GameObject, if there isn't one, destroy
            if (!TryGetComponent(out createACatGenerator))
                Destroy(this);
        }

        private void Update()
        {
            // Ensure the user doesn't set negative time threshold
            if (randomizeTime < 0) { randomizeTime = 0; }

            // Rotating this gameobject by the speed amount
            transform.Rotate(new Vector3(0, rotationSpeed, 0));

            // Updating timer and randomizing if applicable
            timer += Time.deltaTime;
            if (timer >= randomizeTime)
            {
                createACatGenerator.RandomizeCat();
                timer = 0;
            }
        }
    }
}

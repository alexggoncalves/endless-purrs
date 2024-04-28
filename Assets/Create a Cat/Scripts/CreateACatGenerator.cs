using UnityEngine;

namespace CAC
{
    /// <summary>
    /// Main entry class into all cat generation functionality, and stores references to required cat resources
    /// </summary>
    public class CreateACatGenerator : MonoBehaviour
    {
        // Reference to the currently operated upon settings, assigned through custom editor class
        [HideInInspector] public CreateACatSettings settings;
        // Reference to the main material using the custom cat shader, assigned through custom editor class
        [HideInInspector] public Material createACatMaterial;
        // Reference to the cat SMR so it doesn't get lost, assigned through custom editor class
        [HideInInspector] public SkinnedMeshRenderer catSMR;

        // A reference to the current accessory, if it exists
        private GameObject accessoryInstance;

        /// <summary>
        /// The main cat creation method
        /// </summary>
        public void CreateCat()
        {
            // Avoid trying to create with invalid settings
            if (settings == null) { return; }

            // Clean up the previous accessory if it exists
            if (accessoryInstance != null)
                DestroyImmediate(accessoryInstance);

            if (catSMR) // If the cat SMR has been found and assigned
            {
                // Initiate the RNG with the assigned seed
                Random.InitState(settings.seed);

                RandomizeCatBlendshapes(catSMR);

                if (createACatMaterial) // Only perform material operations if it exists
                {
                    Material catMaterialInstance = Instantiate(createACatMaterial);
                    catSMR.sharedMaterial = catMaterialInstance;
                    RandomizeCatMaterial
                    (
                        settings.GetRandomCoatColour(),
                        settings.GetRandomEyeColour(),
                        settings.GetRandomSkinColour(),
                        catMaterialInstance
                    );
                }

                // Only create a random accessory if it succeeds the random roll
                if (Random.Range(1, 101) <= settings.accessoryChance)
                    CreateAccessory(settings.GetRandomAccessory(), catSMR);
            }
        }

        /// <summary>
        /// Method to perform the cat creation operations with a randomized seed, and without changing the cat settings
        /// </summary>
        public void RandomizeCat()
        {
            // Avoid trying to create with invalid settings
            if (settings == null) { return; }

            // Getting a random seed to use rather than the pre-defined seed
            int randomSeed = Random.Range(0, int.MaxValue);

            // Clean up the previous accessory if it exists
            if (accessoryInstance != null)
                DestroyImmediate(accessoryInstance);

            if (catSMR)
            {
                Random.InitState(randomSeed); // Initiate the RNG with the randomized seed

                RandomizeCatBlendshapes(catSMR);

                if (createACatMaterial) // Only perform material operations if it exists
                {
                    Material catMaterialInstance = Instantiate(createACatMaterial);
                    catSMR.sharedMaterial = catMaterialInstance;
                    RandomizeCatMaterial
                    (
                        settings.GetRandomCoatColour(),
                        settings.GetRandomEyeColour(),
                        settings.GetRandomSkinColour(),
                        catMaterialInstance
                    );
                }

                // Only create a random accessory if it succeeds the random roll
                if (Random.Range(1, 101) <= settings.accessoryChance)
                    CreateAccessory(settings.GetRandomAccessory(), catSMR);
            }
        }

        /// <summary>
        /// Creates the given random accessory prefab, and assigns its required skinned data
        /// </summary>
        /// <param name="accessory">The prefab accessory gameobject to be created</param>
        /// <param name="smr">The cat skinned mesh renderer to use the armature of</param>
        private void CreateAccessory(GameObject accessory, SkinnedMeshRenderer smr)
        {
            // Protect against invalid accessory creation
            if (accessory == null) { return; }
            else if (smr == null || smr.bones == null || smr.rootBone == null) { return; }

            // Creating the accessory instance at the proper location
            accessoryInstance = Instantiate(accessory);
            accessoryInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
            accessoryInstance.transform.SetParent(transform);

            // Assigning the cat armature to the new accessory instance
            SkinnedMeshRenderer accessorySMR = accessoryInstance.GetComponent<SkinnedMeshRenderer>();
            if (accessorySMR)
            {
                accessorySMR.bones = smr.bones;
                accessorySMR.rootBone = smr.rootBone;
            }
        }

        /// <summary>
        /// Randomizes all of the blendshape weights on the given skinned mesh renderer's mesh
        /// </summary>
        /// <param name="smr">The skinned mesh renderer containing the mesh with the blendshapes to randomize</param>
        private void RandomizeCatBlendshapes(SkinnedMeshRenderer smr)
        {
            // Protect against invalid blendshape conditions
            if (smr == null || smr.sharedMesh == null || smr.sharedMesh.blendShapeCount == 0) { return; }

            // Setting each blendshape weight in the mesh to a value between 0 and 100
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                smr.SetBlendShapeWeight(i, Random.Range(0, 101));
            }
        }

        /// <summary>
        /// The method for randomizing and assigning all material properties used in the cat shader
        /// </summary>
        /// <param name="coatColour">The cat's coat colour, also used to determine pattern colour</param>
        /// <param name="eyeColour">The cat's eye colour</param>
        /// <param name="skinColour">The cat's skin colour</param>
        /// <param name="material">The material to apply these properties to</param>
        private void RandomizeCatMaterial(Color coatColour, Color eyeColour, Color skinColour, Material material)
        {
            // Protect against invalid material
            if (material == null) { return; }

            // Coat Parameters
            material.SetColor("_Coat_Colour", coatColour);

            // Eye Parameters
            material.SetColor("_Eye_Colour", eyeColour);

            // Pattern Parameters
            Color patternColour = coatColour * Random.Range(0.5f, 2f);
            material.SetColor("_Pattern_Colour", patternColour);
            material.SetVector("_Pattern_Offset", new Vector4(Random.Range(0, 2048), Random.Range(0, 2048), 0, 0));
            material.SetFloat("_Pattern_Threshold", Random.Range(0, 1.01f));
            material.SetInt("_Spots_Pattern", Random.Range(0, 2));

            // Skin Parameters
            material.SetColor("_Skin_Colour", skinColour);

            // Undercoat Parameters
            material.SetFloat("_Undercoat_Leg_Coverage", Random.Range(0, 1.01f));
            material.SetFloat("_Undercoat_Tail_Coverage", Random.Range(0, 1.01f));
            material.SetFloat("_Undercoat_Threshold", Random.Range(0, 1.01f));
        }
    }
}

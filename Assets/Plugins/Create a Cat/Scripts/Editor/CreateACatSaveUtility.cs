using UnityEngine;
using UnityEditor;

namespace CAC.EditorTools
{
    /// <summary>
    /// Convenience class for performing save operations related to cat generation
    /// </summary>
    public static class CreateACatSaveUtility
    {
        // Path to the exports folder, independent from the CreateACat package folder
        private const string ASSETS_PATH = "Assets/CreateACat Exports";

        /// <summary>
        /// Method that saves a prefab from the given cat object and material
        /// </summary>
        /// <param name="catGO">The root cat GameObject</param>
        /// <param name="catMaterial">The cat SkinnedMeshRenderer's material instance</param>
        public static void SaveCatPrefab(GameObject catGO, Material catMaterial)
        {
            #if UNITY_EDITOR
            // Check if this cat gameobject is already a prefab instance, if it is, break the prefab connection
            if (PrefabUtility.IsAnyPrefabInstanceRoot(catGO))
                PrefabUtility.UnpackPrefabInstance(catGO, PrefabUnpackMode.Completely, InteractionMode.UserAction);

            // Setting the cat gameobject name to its instance ID
            catGO.name = $"Cat {catGO.GetInstanceID()}";

            // Ensuring the exports folder exists
            if (!AssetDatabase.IsValidFolder(ASSETS_PATH))
                AssetDatabase.CreateFolder("Assets", "CreateACat Exports");
            // Ensuring the folder for this specific cat instance exists
            if (!AssetDatabase.IsValidFolder($"{ASSETS_PATH}/{catGO.name}"))
                AssetDatabase.CreateFolder("Assets/CreateACat Exports", catGO.name);

            // Creating the path to this cat's folder
            string path = $"{ASSETS_PATH}/{catGO.name}";

            // Checking if the given material is already a material asset, if it is, create a copy of it
            if (AssetDatabase.GetAssetPath(catMaterial) != string.Empty)
                catMaterial = Object.Instantiate(catMaterial);

            // Creating the material asset
            AssetDatabase.CreateAsset(catMaterial, $"{path}/{catGO.name} Material.mat");

            // Creating and connecting the prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(catGO, $"{path}/{catGO.name}.prefab", InteractionMode.UserAction, out bool saveSuccess);

            // Refresh the asset database to reflect changes
            AssetDatabase.Refresh();

            if (saveSuccess)
                Debug.Log($"CreateACat: Sucessfilly saved {catGO.name} at {path}!");
            else
                Debug.LogError($"CreateACat: Failed saving prefab {catGO}! {saveSuccess}");
            #endif
        }

        /// <summary>
        /// Method that saves the cat settings ScriptableObject to file
        /// </summary>
        /// <param name="settings"></param>
        public static void SaveCatSettings(CreateACatSettings settings)
        {
            #if UNITY_EDITOR
            // If this settings ScriptableObject is already an asset, no saving needs to be done
            if (AssetDatabase.GetAssetPath(settings) != string.Empty)
                return;

            // Ensuring the exports folder exists
            if (!AssetDatabase.IsValidFolder(ASSETS_PATH))
                AssetDatabase.CreateFolder("Assets", "CreateACat Exports");
            // Ensuring the settings folder exists in exports
            if (!AssetDatabase.IsValidFolder($"{ASSETS_PATH}/Settings"))
                AssetDatabase.CreateFolder("Assets/CreateACat Exports", "Settings");

            // Setting the settings asset name to match the instance ID
            settings.name = $"Cat Settings{settings.GetInstanceID()}";
            string path = ASSETS_PATH + "/Settings" + $"/{settings.name}.asset";

            // Creating the settings asset as a file
            AssetDatabase.CreateAsset(settings, path);

            // Refresh the asset database to reflect changes
            AssetDatabase.Refresh();
            #endif
        }
    }
}

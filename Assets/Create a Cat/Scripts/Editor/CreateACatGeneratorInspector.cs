using UnityEngine;
using UnityEditor;

namespace CAC.EditorTools
{
    /// <summary>
    /// Custom editor class for inspecting CreateACat generator components
    /// </summary>
    [CustomEditor(typeof(CreateACatGenerator))]
    public class CreateACatGeneratorInspector : Editor
    {
        // Stored reference to the CAC generator being targeted
        private CreateACatGenerator createACatGenerator;

        // The override of the inspector GUI will draw this instead of the default CAC generator component inspector
        public override void OnInspectorGUI()
        {
            if (createACatGenerator == null) { return; }

            // Changing CAC generator's public fields
            EditorGUI.BeginChangeCheck();
            createACatGenerator.createACatMaterial = (Material)EditorGUILayout.ObjectField
            (
                createACatGenerator.createACatMaterial,
                typeof(Material),
                false
            );
            createACatGenerator.catSMR = (SkinnedMeshRenderer)EditorGUILayout.ObjectField
            (
                createACatGenerator.catSMR,
                typeof(SkinnedMeshRenderer),
                true
            );

            EditorGUILayout.Space(10);

            createACatGenerator.settings = (CreateACatSettings)EditorGUILayout.ObjectField
            (
                createACatGenerator.settings,
                typeof(CreateACatSettings),
                false
            );

            // Creating and assigning a default cat settings object if 
            if (createACatGenerator.settings == null)
                createACatGenerator.settings = (CreateACatSettings)CreateInstance(typeof(CreateACatSettings));

            // Instructing Unity that the CAC generator needs to be serialized since changes were made
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(createACatGenerator);

            EditorGUILayout.Space(10);

            // Creating the custom sttings inspector to show its fields
            if (createACatGenerator.settings != null)
            {
                Editor settingsEditor = CreateEditor(createACatGenerator.settings);
                if (settingsEditor != null)
                {
                    EditorGUILayout.LabelField("Cat Settings", EditorStyles.boldLabel);
                    settingsEditor.OnInspectorGUI();
                }
            }

            EditorGUILayout.Space(10);

            // Triggering the cat settings saving to file
            if (GUILayout.Button("Save Settings"))
            {
                if (createACatGenerator.settings != null)
                    CreateACatSaveUtility.SaveCatSettings(createACatGenerator.settings);
                else
                    Debug.LogWarning("CreateACat: Cat settings are null!");
            }

            EditorGUILayout.Space(20);

            // Disable attempting to create, randomize or save cat if these things aren't assigned
            GUI.enabled =
                createACatGenerator != null &&
                createACatGenerator.createACatMaterial != null &&
                createACatGenerator.settings != null;

            // Access to CAC generator's functionalities
            if (GUILayout.Button("Create Cat"))
            {
                if (createACatGenerator != null)
                    createACatGenerator.CreateCat();
                else
                    Debug.Log("CreateACat: Cat Generator is null!");
            }
            else if (GUILayout.Button("Randomize Cat"))
            {
                if (createACatGenerator != null)
                    createACatGenerator.RandomizeCat();
                else
                    Debug.Log("CreateACat: Cat Generator is null!");
            }

            EditorGUILayout.Space(10);

            // Trigerring cat prefab creation
            if (GUILayout.Button("Save Cat Prefab") && createACatGenerator != null && createACatGenerator.catSMR != null)
            {
                CreateACatSaveUtility.SaveCatPrefab(createACatGenerator.gameObject, createACatGenerator.catSMR.sharedMaterial);
            }

            // Ensure GUI is enabled at the end to avoid messing up other GUI elements
            GUI.enabled = true;
        }

        private void OnEnable()
        {
            // Acquiring the CAC generator reference
            createACatGenerator = (CreateACatGenerator)target;
        }
    }
}

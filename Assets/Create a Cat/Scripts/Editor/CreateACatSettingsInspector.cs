using UnityEditor;

namespace CAC.EditorTools
{
    /// <summary>
    /// Custom inspector class for displaying all the fields of the CAC settings scriptable object
    /// </summary>
    [CustomEditor(typeof(CreateACatSettings))]
    public class CreateACatSettingsInspector : Editor
    {
        // Overrides the default ScriptableObject inspector GUI
        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Colour Settings", EditorStyles.label);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coatColours"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eyeColours"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skinColours"));

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Accessory Settings", EditorStyles.label);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("catAccessories"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accessoryChance"));

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));

            // Ensuring any and all modifications are applied back to the ScriptableObject asset
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
    }
}

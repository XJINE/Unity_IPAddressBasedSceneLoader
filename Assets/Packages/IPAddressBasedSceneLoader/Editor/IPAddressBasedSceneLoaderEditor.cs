using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IPAddressBasedSceneLoader.Setting))]
public class IPAddressBasedSceneLoaderSettingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var height = position.height;
        var width  = (position.width - 20) / 4 ;

        var addressLabelRect = new Rect(position.xMin,                  position.y, width, height);
        var addressFieldRect = new Rect(position.xMin + width * 1,      position.y, width, height);
        var sceneLabelRect   = new Rect(position.xMin + width * 2 + 10, position.y, width, height);
        var sceneFieldRect   = new Rect(position.xMin + width * 3 + 10, position.y, width, height);

        var prevIndentLevel = EditorGUI.indentLevel;

        EditorGUI.indentLevel = 0;

        EditorGUI.LabelField    (addressLabelRect, "Address");
        EditorGUI.PropertyField (addressFieldRect, property.FindPropertyRelative(nameof(IPAddressBasedSceneLoader.Setting.ipAddress)), GUIContent.none);
        EditorGUI.LabelField    (sceneLabelRect, "Scene");
        EditorGUI.PropertyField (sceneFieldRect, property.FindPropertyRelative(nameof(IPAddressBasedSceneLoader.Setting.scene)), GUIContent.none);

        EditorGUI.indentLevel = prevIndentLevel;

        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(IPAddressBasedSceneLoader))]
public class IPAddressBasedSceneLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var sceneLoader = (IPAddressBasedSceneLoader)target;

        if (GUILayout.Button("Setup into Build Settings"))
        {
            var loaderScenePath = sceneLoader.gameObject.scene.path;

            if (string.IsNullOrEmpty(loaderScenePath))
            {
                Debug.LogWarning("Save the scene that contains " + nameof(IPAddressBasedSceneLoader) + " before setting up Build Settings.");
            }
            else
            {
                var scenePaths = new List<string> { loaderScenePath };
                    scenePaths.AddRange(sceneLoader.Settings
                              .Select(setting => setting.scene?.Path)
                              .Where(path => !string.IsNullOrEmpty(path)));

                EditorBuildSettings.scenes = scenePaths.Distinct()
                                                       .Select(path => new EditorBuildSettingsScene(path, true))
                                                       .ToArray();
            }
        }

        base.OnInspectorGUI();
    }
}

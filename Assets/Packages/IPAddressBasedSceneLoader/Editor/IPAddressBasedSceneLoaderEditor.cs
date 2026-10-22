using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IPAddressBasedSceneLoader.Setting))]
public class IPAddressBasedSceneLoaderSettingDrawer : PropertyDrawer
{
    private const float LabelWidth    = 55f;
    private const float ColumnSpacing = 10f;

    private static readonly GUIContent AddressLabel = new GUIContent("Address");
    private static readonly GUIContent SceneLabel   = new GUIContent("Scene");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var prevIndentLevel = EditorGUI.indentLevel;
        var prevLabelWidth  = EditorGUIUtility.labelWidth;

        EditorGUI.indentLevel       = 0;
        EditorGUIUtility.labelWidth = LabelWidth;

        var columnWidth = (position.width - ColumnSpacing) / 2f;
        var addressRect = new Rect(position.x,                               position.y, columnWidth, position.height);
        var sceneRect   = new Rect(addressRect.xMax + ColumnSpacing, position.y, columnWidth, position.height);

        EditorGUI.PropertyField(addressRect, property.FindPropertyRelative(nameof(IPAddressBasedSceneLoader.Setting.ipAddress)), AddressLabel);
        EditorGUI.PropertyField(sceneRect,   property.FindPropertyRelative(nameof(IPAddressBasedSceneLoader.Setting.scene)),     SceneLabel);

        EditorGUIUtility.labelWidth = prevLabelWidth;
        EditorGUI.indentLevel       = prevIndentLevel;

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

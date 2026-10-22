using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

public class IPAddressBasedSceneLoader : SingletonMonoBehaviour<IPAddressBasedSceneLoader>
{
    [System.Serializable]
    public class Setting
    {
        public string      ipAddress;
        public SceneObject scene;
    }

    [SerializeField] private float               loadDelayTimeSec = 10f;
    [SerializeField] private LoadSceneParameters loadSceneParameters;
    [SerializeField] private Setting[]           settings;

    [SerializeField] private float guiScale      = 1.25f;
    [SerializeField] private bool  showSettings  = true;
    [SerializeField] private bool  showAddresses = true;

    private Setting _loadSetting;
    private float   _loadTimeSec;
    private bool    _isLoaded;

    private float RemainingTimeSec => _loadTimeSec - Time.timeSinceLevelLoad;

    private void Start()
    {
        _loadSetting = settings.FirstOrDefault(setting => IPAddressUtil.HasAddress(setting.ipAddress));

        if (_loadSetting == null)
        {
            return;
        }

        _loadTimeSec = Time.timeSinceLevelLoad + loadDelayTimeSec;

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(loadDelayTimeSec);

        _isLoaded = true;
        SceneManager.LoadScene(_loadSetting.scene.Path, loadSceneParameters);
    }

    private void OnGUI()
    {
        GUI.matrix *= Matrix4x4.Scale(Vector3.one * guiScale);

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10));
        GUILayout.Label(nameof(IPAddressBasedSceneLoader));

        if (_loadSetting == null)
        {
            GUILayout.Label("Setting Not Found.");
        }
        else if (_isLoaded)
        {
            GUILayout.Label(_loadSetting.ipAddress + " : " + _loadSetting.scene.Path);
            GUILayout.Label("Loaded.");
        }
        else
        {
            var remaining = RemainingTimeSec.ToString("F1");
            GUILayout.Label(_loadSetting.ipAddress + " : " + _loadSetting.scene.Path);
            GUILayout.Label("This will be loading after " + remaining + " seconds.");
        }

        if (showSettings)
        {
            GUILayout.Label("Settings");
            if(settings.Length == 0)
            {
                GUILayout.Label(" - No Definitions");
            }
            foreach (var setting in settings)
            {
                GUILayout.Label(" - " + setting.ipAddress + " : " + setting.scene.Path);
            }
        }

        if (showAddresses)
        {
            GUILayout.Label("Addresses");
            if(IPAddressUtil.LocalAddresses.Length == 0)
            {
                GUILayout.Label(" - No Definitions");
            }
            foreach (var address in IPAddressUtil.LocalAddresses)
            {
                GUILayout.Label(" - " + address);
            }
        }

        GUILayout.EndArea();
    }

    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(Setting))]
    public class SettingDrawer : PropertyDrawer
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
            EditorGUI.PropertyField (addressFieldRect, property.FindPropertyRelative(nameof(Setting.ipAddress)), GUIContent.none);
            EditorGUI.LabelField    (sceneLabelRect, "Scene");
            EditorGUI.PropertyField (sceneFieldRect, property.FindPropertyRelative(nameof(Setting.scene)), GUIContent.none);

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
                        scenePaths.AddRange(sceneLoader.settings
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

    #endif // UNITY_EDITOR
}
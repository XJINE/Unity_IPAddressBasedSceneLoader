using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IPAddressBasedSceneLoader : SingletonMonoBehaviour<IPAddressBasedSceneLoader>
{
    [System.Serializable]
    public class Setting
    {
        public string      ipAddress;
        public SceneObject scene;
    }

    #region Field

    [SerializeField] private float               loadDelayTimeSec = 10f;
    [SerializeField] private LoadSceneParameters loadSceneParameters;
    [SerializeField] private Setting[]           settings;

    [SerializeField] private float guiScale      = 1.25f;
    [SerializeField] private bool  showSettings  = true;
    [SerializeField] private bool  showAddresses = true;

    private float    _startTimeSec = -1;
    private Setting  _loadSetting  = null;

    #endregion Field

    #region Method

    private void Start()
    {
        foreach (var setting in settings)
        {
            if (IPAddressUtil.HasAddress(setting.ipAddress))
            {
                _startTimeSec = Time.timeSinceLevelLoad;
                _loadSetting  = setting;
                break;
            }
        }
    }

    private void Update()
    {
        if(0 <= _startTimeSec && loadDelayTimeSec < Time.timeSinceLevelLoad - _startTimeSec)
        {
            SceneManager.LoadScene(_loadSetting.scene.Path, loadSceneParameters);
        }
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
        else
        {
            var remaining = (loadDelayTimeSec - (Time.timeSinceLevelLoad - _startTimeSec)).ToString("F1");
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

    #endregion Method

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
                var scenesToSetup = new List<EditorBuildSettingsScene>();

                foreach (var setting in sceneLoader.settings)
                {
                    if(setting.scene == null || scenesToSetup.Any(scene => scene.path == setting.scene.Path))
                    {
                        continue;
                    }

                    scenesToSetup.Add(new EditorBuildSettingsScene(setting.scene.Path, true));
                }

                scenesToSetup.Insert(0, new EditorBuildSettingsScene(SceneManager.GetActiveScene().path, true));

                EditorBuildSettings.scenes = scenesToSetup.ToArray();

                EditorUtility.SetDirty(target);
            }

            base.OnInspectorGUI();
        }
    }

    #endif // UNITY_EDITOR
}
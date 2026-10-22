using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private float               loadDelayTimeSec = 10f;
    [SerializeField] private LoadSceneParameters loadSceneParameters;
    [SerializeField] private Setting[]           settings;

    [SerializeField] private float guiScale      = 1.25f;
    [SerializeField] private bool  showSettings  = true;
    [SerializeField] private bool  showAddresses = true;

    private Setting _loadSetting;
    private float   _loadTimeSec;
    private bool    _isLoaded;

    public IReadOnlyList<Setting> Settings => settings;

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
}
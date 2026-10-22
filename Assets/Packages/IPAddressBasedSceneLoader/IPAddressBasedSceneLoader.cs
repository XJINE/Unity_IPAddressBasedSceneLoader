using System.Collections;
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

    [field: SerializeField] public float               LoadDelayTimeSec    { get; private set; } = 10f;
    [field: SerializeField] public LoadSceneParameters LoadSceneParameters { get; private set; }
    [field: SerializeField] public Setting[]           Settings            { get; private set; }
                            public Setting             LoadSetting         { get; private set; }
                            public bool                IsLoaded            { get; private set; }
                            public float               RemainingTimeSec => _loadTimeSec - Time.timeSinceLevelLoad;

    private float _loadTimeSec;

    private void Start()
    {
        LoadSetting = Settings.FirstOrDefault(setting => IPAddressUtil.HasAddress(setting.ipAddress));

        if (LoadSetting == null)
        {
            return;
        }

        _loadTimeSec = Time.timeSinceLevelLoad + LoadDelayTimeSec;

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(LoadDelayTimeSec);

        IsLoaded = true;

        SceneManager.LoadScene(LoadSetting.scene.Path, LoadSceneParameters);
    }
}
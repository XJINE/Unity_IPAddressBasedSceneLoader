using UnityEngine;

[RequireComponent(typeof(IPAddressBasedSceneLoader))]
public class IPAddressBasedSceneLoaderView : SingletonMonoBehaviour<IPAddressBasedSceneLoaderView>
{
    private void OnGUI()
    {
        var loader = GetComponent<IPAddressBasedSceneLoader>();

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10));
        GUILayout.Label(nameof(IPAddressBasedSceneLoader));

        if (loader.LoadSetting == null)
        {
            GUILayout.Label("Setting Not Found.");
        }
        else if (loader.IsLoaded)
        {
            GUILayout.Label(loader.LoadSetting.ipAddress + " : " + loader.LoadSetting.scene.Path);
            GUILayout.Label("Loaded.");
        }
        else
        {
            var remaining = loader.RemainingTimeSec.ToString("F1");
            GUILayout.Label(loader.LoadSetting.ipAddress + " : " + loader.LoadSetting.scene.Path);
            GUILayout.Label("This will be loading after " + remaining + " seconds.");
        }

        GUILayout.Label("Settings");

        if(loader.Settings.Length == 0)
        {
            GUILayout.Label(" - No Definitions");
        }

        foreach (var setting in loader.Settings)
        {
            GUILayout.Label(" - " + setting.ipAddress + " : " + setting.scene.Path);
        }

        GUILayout.Label("Addresses");

        if(IPAddressUtil.LocalAddresses.Length == 0)
        {
            GUILayout.Label(" - No Definitions");
        }

        foreach (var address in IPAddressUtil.LocalAddresses)
        {
            GUILayout.Label(" - " + address);
        }

        GUILayout.EndArea();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingButton : MonoBehaviour
{
    public Dropdown ResolutionDropdown;

    public void SetResolution()
    {
        switch (ResolutionDropdown.value)
        {
            case 0:
                Screen.SetResolution(2560, 1600, true);
                break;
            case 1:
                Screen.SetResolution(2560, 1440, true);
                break;
            case 2:
                Screen.SetResolution(1920, 1080, true);
                break;
        }
    }

    /// <summary>
    /// 重置存档：清空 SaveManager 内存数据并立即写盘，然后重新加载当前场景。
    /// 绑定到 SettingsCanvas/Panel/RestartButton 的 onClick。
    /// </summary>
    public void ResetSave()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetSave();

        // 重新加载当前场景，让所有脚本用全新存档重新初始化
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.isPlaying = false;
    }
}

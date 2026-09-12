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
                Screen.SetResolution(2560, 1600,true);
                break;
            case 1:
                Screen.SetResolution(2560, 1440,true);
                break;
            case 2:
                Screen.SetResolution(1920, 1080,true);
                break;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.isPlaying = false;
    }
}

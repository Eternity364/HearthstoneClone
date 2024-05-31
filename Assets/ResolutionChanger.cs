using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionChanger : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown dropdown;

    public void SetResolution(int i)
    {
        if (dropdown.value == 0)
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed, new RefreshRate());
        else if (dropdown.value == 1)
            Screen.SetResolution(1600, 900, FullScreenMode.Windowed, new RefreshRate());
        else if (dropdown.value == 2)
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed, new RefreshRate());
        else if (dropdown.value == 3)
            Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen, new RefreshRate());
    }
}

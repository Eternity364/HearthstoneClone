using System.Collections.Generic;
using UnityEngine;

public class AdjustWindow : MonoBehaviour
{
    private int lastWidth = 0;
    private int lastHeight = 0;
    private int currentDisplay = -1;


    void Update()
    {
        // var width = Screen.width; var height = Screen.height;
        

        // if (lastWidth != width) // if the user is changing the width
        // {
        //     print("width = " + width);
        //     print("height = " + height);
        //     print("lastWidth = " + lastWidth);
        //     print("lastHeight = " + lastHeight);
        //     // update the height
        //     lastHeight = (int)(width / 16.0 * 9.0);
        //     Screen.SetResolution(width, lastHeight, FullScreenMode.Windowed, new RefreshRate());
        // }
        // else if (lastHeight != height) // if the user is changing the height
        // {
        //     print("width = " + width);
        //     print("height = " + height);
        //     print("lastWidth = " + lastWidth);
        //     print("lastHeight = " + lastHeight);
        //     // update the width
        //     lastWidth = (int)(height / 9.0 * 16.0);
        //     Screen.SetResolution(lastWidth, height, FullScreenMode.Windowed, new RefreshRate());
        // }
        
        // //print("display = " + GetCurrentDisplayNumber());
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    public int GetCurrentDisplayNumber()
    {
        List<DisplayInfo> displayLayout = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displayLayout);
        return displayLayout.IndexOf(Screen.mainWindowDisplayInfo);
    }
}
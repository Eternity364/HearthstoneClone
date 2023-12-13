using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotkeys : MonoBehaviour
{
    [SerializeField] GameObject quantumConsole;
    
    void Update()
    {
        if (Input.GetKeyDown("`"))
        {
            quantumConsole.SetActive(!quantumConsole.activeSelf);
        }
    }
}

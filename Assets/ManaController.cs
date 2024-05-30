using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManaController : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    private int value, maxValue;
    
    public void Set(int value, int maxValue)
    {
        this.value = value;
        this.maxValue = maxValue;
        UpdateText();
    }

    void UpdateText()
    {
        text.SetText(value + "/" + maxValue);
    }
}

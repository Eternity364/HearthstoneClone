using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class EndTurnTimer : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    int timerValue;

    private float value = 0;

    public void Begin()
    {
        value = timerValue;
    }

    void Update()
    {
        value -= Time.deltaTime;
        text.SetText(Math.Round(value).ToString());
        if (value <= 0)
            gameObject.SetActive(false);
    }
}

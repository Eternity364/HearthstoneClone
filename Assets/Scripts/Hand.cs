using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField]
    Card[] cards;
    [SerializeField]
    Vector3 positionShift;

    void Start()
    {
        Sort();
    }

    public void Sort()
    {
        int lenght = cards.Length - 1;
        Vector3 startPosition = (-lenght / 2 + 1) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= 0.5f * positionShift;
            
        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.position += startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderOrder(i);
        }
    }
}

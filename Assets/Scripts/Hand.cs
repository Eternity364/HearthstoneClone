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
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].transform.position += positionShift * i;
            cards[i].cardDisplay.SetRenderOrder(i);
        }
    }
}

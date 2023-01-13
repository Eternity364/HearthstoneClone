using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InHandClickHandler : MonoBehaviour
{
    [SerializeField]
    public GameObject card;

    void OnMouseDown()
    {
        Debug.Log(card.name);
    }
}

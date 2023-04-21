using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InHandClickHandler : MonoBehaviour
{
    [SerializeField]
    public GameObject card;

    private bool clicked = false;

    void OnMouseDown()
    {
        clicked = true;
    }

    void Update()
    {
        if (clicked)
            card.transform.localPosition = PositionGetter.GetPosition();
    }
}

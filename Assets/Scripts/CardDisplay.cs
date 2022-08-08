using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject cardInHand;
    [SerializeField]
    private GameObject textCanvas;


    void Update()
    {
        if (cardInHand != null)
            textCanvas.SetActive(cardInHand.transform.rotation.y > -0.7 && cardInHand.transform.rotation.y < 0.7);
    }
}

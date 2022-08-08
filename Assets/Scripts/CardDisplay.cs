using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject cardInHand;
    [SerializeField]
    private GameObject textCanvasGO;
    [SerializeField]
    private Canvas textCanvas;


    void Update()
    {
        TextVisibilityWorkaround();
    }

    // Workaround to fix card text visibility after rotating card
    void TextVisibilityWorkaround()
    {
        if (cardInHand != null) {
            Debug.Log(cardInHand.transform.rotation.y);
            textCanvasGO.SetActive(cardInHand.transform.rotation.y > -0.7 && cardInHand.transform.rotation.y < 0.7);
            if (cardInHand.transform.rotation.y != 0)
                textCanvas.sortingLayerName = "1";
            else
                textCanvas.sortingLayerName = "Default";
        }
    }
}

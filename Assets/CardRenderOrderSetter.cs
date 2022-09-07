using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CardRenderOrderSetter : MonoBehaviour
{
    [SerializeField]
    Canvas mainCanvas;
    [SerializeField]
    Canvas textCanvas;
    [SerializeField]
    SortingGroup sortGroup;
    [SerializeField]
    SpriteRenderer inHandImage;
    [SerializeField]
    SpriteRenderer inHandFront;
    [SerializeField]
    SpriteRenderer inHandBack;
    [SerializeField]
    SpriteRenderer onFieldImage;
    [SerializeField]
    SpriteRenderer onFieldFront;

    public void Set(int layer) {
        mainCanvas.sortingLayerName = "InHandCard" + layer.ToString();
        textCanvas.sortingLayerName = "InHandCard" + layer.ToString();
        sortGroup.sortingLayerName = "InHandCard" + layer.ToString();
        inHandImage.sortingLayerName = "InHandCard" + layer.ToString();
        inHandFront.sortingLayerName = "InHandCard" + layer.ToString();
        inHandBack.sortingLayerName = "InHandCard" + layer.ToString();
        onFieldImage.sortingLayerName = "InHandCard" + layer.ToString();
        onFieldFront.sortingLayerName = "InHandCard" + layer.ToString();
    }
}

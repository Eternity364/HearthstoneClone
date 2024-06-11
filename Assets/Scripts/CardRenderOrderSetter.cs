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
    [SerializeField]
    SpriteRenderer divineShield;

    public void Set(string layer) {
        mainCanvas.sortingLayerName = layer;
        textCanvas.sortingLayerName = layer;
        sortGroup.sortingLayerName = layer;
        inHandImage.sortingLayerName = layer;
        inHandFront.sortingLayerName = layer;
        inHandBack.sortingLayerName = layer;
        onFieldImage.sortingLayerName = layer;
        onFieldFront.sortingLayerName = layer;
        divineShield.sortingLayerName = layer;
    }
}

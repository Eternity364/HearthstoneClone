using DG.Tweening;
using TMPro;
using UnityEngine;

public class SleepMode : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI textObj;
    [SerializeField]
    CanvasGroup group;
    
    public void Activate()
    {
        textObj.transform.localPosition = Vector3.zero;
        AlphaSetter(1f);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(textObj.transform.DOLocalMove(new Vector3(0.9f, 0.9f, 0), 2f));
        mySequence.Insert(1f, DOTween.To(AlphaSetter, 1f, 0, 1f));
        mySequence.OnComplete(Activate);       
        
        void AlphaSetter(float alpha) {
            group.alpha = alpha;
        }
    }
}

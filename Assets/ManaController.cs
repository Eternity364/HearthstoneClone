using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ManaController : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;  
    [SerializeField]
    bool crystalsDisplay;  
    [SerializeField]
    GameObject crystalPrefab;
    [SerializeField]
    Transform crystalParent;
    [SerializeField]
    Material grayScale;
    [SerializeField]
    Material original;

    private int value = -1;
    private int maxValue = -1;
    List<GameObject> crystals = new List<GameObject>();
    
    public void Set(int value, int maxValue)
    {
        bool appear = false;
        if (this.value == -1)
            appear = true;
        this.value = value;
        this.maxValue = maxValue;
        if (appear)
            StartAppearAnimation();
        UpdateValue();
    }

    void UpdateValue()
    {
        text.SetText(value + "/" + maxValue);
        if (crystals.Count == maxValue) {
            for (int i = 0; i < maxValue; i++)
            {
                if (i >= value) {
                    crystals[i].GetComponent<SpriteRenderer>().material = grayScale;
                }
            }
        }
    }

    public void StartAppearAnimation()
    {
        if (crystalsDisplay) {
            Sequence mySequence = DOTween.Sequence();
            for (int i = 0; i < maxValue; i++)
            {
                GameObject crystal;
                if (i >= crystals.Count) {
                    crystal = Instantiate(crystalPrefab, crystalParent);
                    crystals.Add(crystal);
                    crystal.SetActive(true);
                }
                else
                    crystal = crystals[i];

                crystal.GetComponent<SpriteRenderer>().material = original;
                Vector2 oldScale = new Vector2(crystal.transform.localScale.x, crystal.transform.localScale.y);
                crystal.transform.localPosition = new Vector3(i * 0.05f, 0, 0);
                crystal.transform.localScale = new Vector2(oldScale.x / 10f, oldScale.y / 10f);
                mySequence.Insert(0.1f * i, crystal.transform.DOScale(new Vector2(0.07f, 0.07f), 0.5f).SetEase(Ease.OutBounce));
                
            }
        }
    }
}

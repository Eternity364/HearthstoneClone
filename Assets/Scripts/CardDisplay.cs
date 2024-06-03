using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private Card card;
    [SerializeField]
    private GameObject textCanvasGO;
    [SerializeField]
    private Text inHandAttack;
    [SerializeField]
    private Text inHandHealth;
    [SerializeField]
    private Text inHandCost;
    [SerializeField]
    private Text onFieldAttack;
    [SerializeField]
    private Text onFieldHealth;
    [SerializeField]
    private GameObject cardBack;
    [SerializeField]
    private CardData data;
    [SerializeField]
    private GameObject shadowInHand;
    [SerializeField]
    private GameObject shadowOnBoard;
    [SerializeField]
    private CardChangingStateAnimation changingStateAnimation;
    [SerializeField]
    private CardRenderOrderSetter cardRenderOrderSetter;
    public Transform mainObjectsTransform; 
    [SerializeField]
    public GameObject grayScale; 
    [SerializeField]
    public Transform intermediateObjectsTransform; 
    [SerializeField]
    Color lackHealth; 
    [SerializeField]
    GameObject activeStatusInHand; 
    [SerializeField]
    GameObject activeStatusOnField; 
    [SerializeField]
    GameObject pickedCardParticles; 
    [SerializeField]
    GameObject placingCardParticles;  
    [SerializeField]
    GameObject deathParticles; 
    [SerializeField]
    AttackParticle attackParticle; 
    [SerializeField]
    DisplayStates currentState;

    public enum DisplayStates
    {
        InHand = 1,
        OnField = 2
    }

    public CardData Data
    {
        get { return data; }
    }
    bool shadowsActive = false;

    void Awake()
    {
        UpdateDisplay();
    }

    public void SetData(CardData data)
    {
        //if (this.data == null)
            this.data = data;
        UpdateDisplay();
    }

    public void ChangeState(DisplayStates state) {
        changingStateAnimation.Do(state);
        currentState = state;
        SetShadowActive(shadowsActive);
        card.clickHandler.SetRectAreaClickable(state == DisplayStates.InHand);
    }

    public void SetPickedCardParticlesActive(bool active) {
        pickedCardParticles.SetActive(true);
    }

    public void SetPlacingParticlesActive(bool active) {
        placingCardParticles.SetActive(true);
    }

    public void ResetTransform() {
        mainObjectsTransform.localScale = Vector3.one;
        mainObjectsTransform.localRotation = Quaternion.Euler(Vector3.zero);
        mainObjectsTransform.localPosition = Vector3.zero;
        intermediateObjectsTransform.localScale = Vector3.one;
        intermediateObjectsTransform.localRotation = Quaternion.Euler(Vector3.zero);
        intermediateObjectsTransform.localPosition = Vector3.zero;
    }

    public void SetRenderLayer(string layer)
    {
        cardRenderOrderSetter.Set(layer);
    }

    public void SetCardFrontActive(bool active)
    {
        cardBack.SetActive(!active);
        textCanvasGO.SetActive(active);
    }

    public void SetActiveStatus(bool active)
    {
        if (currentState == DisplayStates.InHand)
            activeStatusInHand.SetActive(active);
        else
            activeStatusOnField.SetActive(active);
    }

    public void SetAttackParticleAngle(Vector2 attackerPos)
    {
        attackParticle.SetAngle(attackerPos);
    }

    public void SetAttackParticleActive()
    {
        attackParticle.gameObject.SetActive(false);
        attackParticle.gameObject.SetActive(true);
    }

    public void StartDeathAnimation()
    {
        grayScale.SetActive(true);
        float duration = 0.7f;
        float goDownDuration = 0.8f;
        int shakesCount = 25;
        int divisionRate = 500;
        Sequence mySequence = DOTween.Sequence();
        void DeathParticles() {
            deathParticles.SetActive(true);
        }           

        for (int i = 0; i < shakesCount; i++)
        {
            mySequence.Append(mainObjectsTransform.DOLocalMove(new Vector3(
                UnityEngine.Random.Range(-10.0f, 10.0f) / divisionRate, UnityEngine.Random.Range(-10.0f, 10.0f) / divisionRate, 0), 
                duration / shakesCount).SetEase(Ease.OutCubic));
            
        } 
        mySequence.AppendCallback(DeathParticles);
        mySequence.Append(mainObjectsTransform.DOLocalMove(new Vector3(0, 0, 0.2f), 
                0.8f).SetEase(Ease.OutCubic));
        mySequence.InsertCallback(duration + goDownDuration, DeathParticles);
    }

    public void DestroyCard() {
        Destroy(gameObject);
    }

    public void UpdateDisplay() {
        if (this.data != null) {
            inHandAttack.text = data.Attack.ToString();
            inHandHealth.text = data.Health.ToString();
            inHandCost.text = data.Cost.ToString();
            onFieldAttack.text = data.Attack.ToString();
            onFieldHealth.text = data.Health.ToString();
            if (data.Health < data.MaxHealth)
                onFieldHealth.color = lackHealth;
        }
    }

    // void OnCardTurningVisibility()
    // {
    //     bool back = card.gameObject.transform.eulerAngles.y > 93.61 && card.gameObject.transform.eulerAngles.y < 273.75f;
    //     cardBack.SetActive(back);
    //     textCanvasGO.SetActive(!back);
    // }

    public void SetShadowActive(bool value)
    {
        shadowInHand.SetActive(value && currentState == DisplayStates.InHand);
        shadowOnBoard.SetActive(value && currentState != DisplayStates.InHand);
        shadowsActive = value;
    }
}

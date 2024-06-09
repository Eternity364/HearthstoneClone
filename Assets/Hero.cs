using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Hero : MonoBehaviour
{
    [SerializeField]
    PolygonCollider2D area;
    [SerializeField]
    public HeroData data;
    [SerializeField]
    TextMeshProUGUI healthText;
    [SerializeField]
    Color lackHealth;
    [SerializeField]
    GameObject playerPicture;
    [SerializeField]
    GameObject enemyPicture;
    [SerializeField]
    Transform intermediate;
    [SerializeField]
    GameObject deathParticles;
    [SerializeField]
    Material grayScale;
    public TweenCallback OnDeath;
    public UnityAction OnMouseUpEvents;
    public UnityAction<Card> OnMouseEnterCallbacks;
    public UnityAction<Card> OnMouseLeaveCallbacks;

    bool mouseEntered = false;
    bool dead = false;
    GameObject picture;

    void Start()
    {
        UpdateDisplay();
    }

    public void SetClickable(bool active)
    {
        area.enabled = active;
    }

    public bool DealDamage(int damage)
    {
        data.Health -= damage;
        UpdateDisplay();
        bool dead = data.Health <= 0;
        if (dead) StartDeathAnimation();
        return dead;
    }

    
    public void SetState(PlayerState state)
    {
        playerPicture.SetActive(state == PlayerState.Player);
        enemyPicture.SetActive(state != PlayerState.Player);
        picture = playerPicture;
        if (state == PlayerState.Enemy)
            picture = enemyPicture;
    }

    void UpdateDisplay()
    {
        healthText.text = data.Health.ToString();
        if (data.Health < data.maxHealth)
            healthText.color = lackHealth;
    }

    public void StartDeathAnimation()
    {
        if (!dead) {
            dead = true;
            InputBlockerInstace.Instance.AddBlock();
            picture.GetComponent<SpriteRenderer>().material = grayScale;
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
                mySequence.Append(intermediate.DOLocalMove(new Vector3(
                    UnityEngine.Random.Range(-10.0f, 10.0f) / divisionRate, UnityEngine.Random.Range(-10.0f, 10.0f) / divisionRate, 0), 
                    duration / shakesCount).SetEase(Ease.OutCubic));
                
            } 
            mySequence.AppendCallback(DeathParticles);
            mySequence.Append(intermediate.DOLocalMove(new Vector3(0, 0, 0.2f), 
                    0.8f).SetEase(Ease.OutCubic));
            mySequence.InsertCallback(duration + goDownDuration, DeathParticles);
            mySequence.AppendInterval(1f);
            mySequence.OnComplete(OnDeath);
        }
    }

    void OnMouseEnter()
    {
        mouseEntered = true;
        if (OnMouseEnterCallbacks != null)
            OnMouseEnterCallbacks.Invoke(null);
    }

    void OnMouseExit()
    {
        if (mouseEntered && OnMouseLeaveCallbacks != null) {
            OnMouseLeaveCallbacks.Invoke(null);
        }
        mouseEntered = false;
    }

    void Update() {
        if (Input.GetMouseButtonUp (0) && mouseEntered) {
            if (OnMouseUpEvents != null)
                OnMouseUpEvents.Invoke();
        }
    }
}

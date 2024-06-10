using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SplashScreen : MonoBehaviour
{
    [SerializeField]
    Transform newTurnScreen;    
    [SerializeField]
    Transform victoryScreen; 
    [SerializeField]
    Transform defeatScreen;
    [SerializeField]
    GameObject playerVictory;
    [SerializeField]
    GameObject enemyVictory;
    [SerializeField]
    GameObject playerDefeat;
    [SerializeField]
    GameObject enemyDefeat;
    [SerializeField]
    BoxCollider2D areaRect;

    Transform currentScreen;
    TweenCallback Action;

    public void Clear()
    {
        areaRect.enabled = false;
        victoryScreen.gameObject.SetActive(false);
        defeatScreen.gameObject.SetActive(false);
        newTurnScreen.gameObject.SetActive(false);
    }  

    public void ShowNewTurnMessage()
    {
        Vector2 startScale = new Vector2(0.025f, 0.025f);
        newTurnScreen.localScale = new Vector2(0.025f, 0.025f);
        newTurnScreen.gameObject.SetActive(true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(newTurnScreen.DOScale(new Vector2(0.25f, 0.25f), 0.5f).SetEase(Ease.OutQuad));
        mySequence.AppendInterval(1f);
        mySequence.Append(newTurnScreen.DOScale(startScale, 0.2f).SetEase(Ease.InQuad));
        mySequence.OnComplete(Disable);
        
        void Disable() {
            newTurnScreen.gameObject.SetActive(false);
        }
    }

    public void ShowVictoryMessage(TweenCallback Action, PlayerState state)
    {
        areaRect.enabled = true;
        if (state == PlayerState.Player)
            playerVictory.SetActive(true);
        else
            enemyVictory.SetActive(true);
        victoryScreen.gameObject.SetActive(true);
        victoryScreen.localScale = new Vector2(0.1f, 0.1f);
        currentScreen = victoryScreen;
        victoryScreen.DOScale(Vector2.one, 0.5f).SetEase(Ease.OutQuad);
        this.Action = Action;
    }    
    
    public void ShowDefeatMessage(TweenCallback Action, PlayerState state)
    {
        areaRect.enabled = true;
        if (state == PlayerState.Player)
            playerDefeat.SetActive(true);
        else
            enemyDefeat.SetActive(true);
        defeatScreen.gameObject.SetActive(true);
        defeatScreen.localScale = new Vector2(0.1f, 0.1f);
        currentScreen = defeatScreen;
        defeatScreen.DOScale(Vector2.one, 0.5f).SetEase(Ease.OutQuad);
        this.Action = Action;
    }

    void OnMouseDown()
    {
        areaRect.enabled = false;
        victoryScreen.DOScale(new Vector2(0.1f, 0.1f), 0.4f).SetEase(Ease.InQuad).OnComplete(Action);
    }
}

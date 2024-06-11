using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameInstance
{
    public UnityAction<GameInstance> OnTimerRunOut;
    public UnityAction<GameInstance> OnTimerThresholdReached;
    public Func<CardData> GenerateNewData;

    PlayerPair pair;
    GameState state;
    float turnDuration;
    float timerThreshold;
    bool timerThresholdReached = false;
    
    public float currentTimer;
    public int maxCardsInHand;
    public int cardIndexGeneratedThisTurn;
    public PlayerState currentTurn = PlayerState.Player;

    public PlayerPair Pair
    {
        get { return pair; }
    }
    public GameState GameState
    {
        get { return state; }
    }

    public GameInstance(PlayerPair pair, float turnDuration, float timerThreshold, int maxCardsInHand, GameState state)
    {
        this.pair = pair;
        this.turnDuration = turnDuration;
        this.timerThreshold = timerThreshold;
        this.state = state;
        this.maxCardsInHand = maxCardsInHand;
        currentTimer = 0;
    }

    public void SetTurn(PlayerState turn)
    {
        currentTurn = turn;
        timerThresholdReached = false;
        currentTimer = 0;
        state.SetCardsActive(turn);
        state.ProgressMana(turn);
        List<CardData> list = state.GetHandListByState(turn);
        if (list.Count < maxCardsInHand) {
            list.Add(GenerateNewData());
            cardIndexGeneratedThisTurn = list[list.Count - 1].Index;
        } 
        else
            cardIndexGeneratedThisTurn = -1;
    }

    public void Clear()
    {
        OnTimerRunOut = null;
        OnTimerThresholdReached = null;
    }

    public void OnUpdate(float dt)
    {
        currentTimer += dt;
        if (currentTimer >= turnDuration)
        {
            OnTimerRunOut.Invoke(this);
        }
        if (currentTimer >= timerThreshold && !timerThresholdReached)
        {
            OnTimerThresholdReached.Invoke(this);
            timerThresholdReached = true;
        }
    }
}

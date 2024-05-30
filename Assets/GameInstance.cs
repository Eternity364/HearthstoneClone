using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameInstance
{
    public UnityAction<GameInstance> OnTimerRunOut;
    public UnityAction<GameInstance> OnTimerThresholdReached;

    PlayerPair pair;
    GameState state;
    float turnDuration;
    float timerThreshold;
    bool timerThresholdReached = false;
    public float currentTimer;
    public PlayerState currentTurn;

    public PlayerPair Pair
    {
        get { return pair; }
    }
    public GameState GameState
    {
        get { return state; }
    }

    public GameInstance(PlayerPair pair, float turnDuration, float timerThreshold, GameState state)
    {
        this.pair = pair;
        this.turnDuration = turnDuration;
        this.timerThreshold = timerThreshold;
        this.state = state;
        currentTimer = 0;
    }

    public void SetTurn(PlayerState turn)
    {
        currentTurn = turn;
        timerThresholdReached = false;
        currentTimer = 0;
        state.SetCardsActive(turn);
        state.ProgressMana(turn);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameInstance
{
    public UnityAction<GameInstance> OnTimerRunOut;

    PlayerPair pair;
    GameState state;
    float turnDuration;
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

    public GameInstance(PlayerPair pair, float turnDuration, GameState state)
    {
        this.pair = pair;
        this.turnDuration = turnDuration;
        this.state = state;
        currentTimer = 0;
    }

    public void SetTurn(PlayerState turn)
    {
        currentTurn = turn;
    }

    public void Clear()
    {
        OnTimerRunOut = null;
    }

    public void OnUpdate(float dt)
    {
        currentTimer += dt;
        if (currentTimer >= turnDuration)
        {
            OnTimerRunOut.Invoke(this);
        }
    }
}

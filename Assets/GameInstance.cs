using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance
{
    PlayerPair pair;
    GameState state;
    PlayerState currentTurn;
    float turnDuration;
    float currentTimer;

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
    }

    public void SetTurn(PlayerState turn)
    {
        currentTurn = turn;
    }

    void OnUpdate(float dt)
    {
        
    }
}

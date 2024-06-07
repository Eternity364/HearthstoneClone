using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameInstanceManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private CardGenerator cardGenerator;

    List<GameInstance> instances = new List<GameInstance>();

    public GameInstance Create(PlayerPair pair, UnityAction<GameInstance> OnTimerRunOut, UnityAction<GameInstance> OnTimerThresholdReached)
    {
        GameState gameState = new GameState(cardGenerator.GetRandomDataList(3), cardGenerator.GetRandomDataList(4),
            cardGenerator.GetRandomDataList(9), cardGenerator.GetRandomDataList(9), 
            1, 0, 1, 0, 10, 10,
            30, 30, 30, 30,
            OnCardDead, OnManaChangeEmpty, OnHeroDead);
        GameInstance newInstance = new GameInstance(pair, 30, 10, 10, gameState);
        newInstance.OnTimerRunOut += OnTimerRunOut;
        newInstance.OnTimerThresholdReached += OnTimerThresholdReached;
        newInstance.GenerateNewData = cardGenerator.GetRandomData;
        newInstance.GameState.PrintCounts();
        instances.Add(newInstance);
        return newInstance;
    }

    public GameInstance GetInstanceByPlayerID(ulong playerID) {
        GameInstance instance = null;
        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null && instances[i].Pair.IsClientIdInPair(playerID)) {
                instance = instances[i];
                break;
            }
        }
        return instance;
    }    
    
    public int GetInstanceID(GameInstance instance) {
        return instances.IndexOf(instance);
    }

    public void Remove(GameInstance instance) {
        instances[instances.IndexOf(instance)] = null;
    }
    
    private void OnCardDead(PlayerState state, int index) {
    }

    private void OnHeroDead(PlayerState state) {
    }

    private void OnManaChangeEmpty(PlayerState state, int empty, int empty2) {
    }

    void Update()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null)
                instances[i].OnUpdate(Time.deltaTime);
        }
    }
}

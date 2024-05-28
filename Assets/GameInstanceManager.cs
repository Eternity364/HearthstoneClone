using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstanceManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;

    List<GameInstance> instances = new List<GameInstance>();

    public GameInstance Create(PlayerPair pair)
    {
        GameState gameState = new GameState(boardManager.playerCardsSet, boardManager.enemyCardsSet, playerHand.cards, opponentHand.cards, OnCardDead);
        GameInstance newInstance = new GameInstance(pair, 60, gameState);
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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

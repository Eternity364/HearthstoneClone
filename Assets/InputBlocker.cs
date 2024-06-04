using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InputBlockerInstace {
    private static InputBlocker instance;
 
    public static InputBlocker Instance
    {
        get { return instance; }
    }
 
    public static void SetInstance(InputBlocker instance)
    {
        if (InputBlockerInstace.instance == null)
           InputBlockerInstace.instance =  instance;
    }
}

public class InputBlocker : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Hand playerHand;
    [SerializeField] private ActiveCardController activeCardController;

    private List<InputBlock> blocks = new List<InputBlock>();
    private List<InputBlock> handBlocks = new List<InputBlock>();
    private Dictionary<InputBlock, Card> cardBlocks = new Dictionary<InputBlock, Card>();

    void Awake()
    {
        InputBlockerInstace.SetInstance(this);
    }

    public InputBlock AddBlock()
    {
        InputBlock block = new InputBlock();
        blocks.Add(block);
        UpdateValues();
        return block;
    }  

    public InputBlock AddCardBlock(Card card) {
        InputBlock block = new InputBlock();
        cardBlocks[block] = card;
        UpdateValues();
        return block;
    }

    public InputBlock AddHandBlock()
    {
        InputBlock block = new InputBlock();
        handBlocks.Add(block);
        UpdateValues();
        return block;
    }  

    public void RemoveBlock(InputBlock block)
    {
        blocks.Remove(block);
        handBlocks.Remove(block);
        if (cardBlocks.ContainsKey(block)) {
            cardBlocks.Remove(block);
        }
        UpdateValues();
    }

    public void RemoveCardBlock(Card card)
    {
        while(cardBlocks.ContainsValue(card)) {
            var item = cardBlocks.First(kvp => kvp.Value == card);
            cardBlocks.Remove(item.Key);
        }
        UpdateValues();
    }

    public void UpdateValues()
    {
        //boardManager.SetInputActive(blocks.Count == 0);
        if (blocks.Count > 0)
            boardManager.DisableAttack();
        boardManager.SetCardsActive(true);
        playerHand.SetCardsActive(true);
        boardManager.SetCardsActive(blocks.Count == 0);
        playerHand.SetCardsActive(blocks.Count == 0 && handBlocks.Count == 0);
        if (activeCardController.pickedCard)
            playerHand.SetCardActive(activeCardController.pickedCard, blocks.Count == 0 && handBlocks.Count == 0);
        print("cardBlocks = " + cardBlocks.Count);
        foreach(var item in cardBlocks)
        {
            Card card = item.Value;
            if (playerHand.cards.Contains(card))
                playerHand.SetCardActive(card, false);
            if (boardManager.PlayerCardsOnBoard.Contains(card))
                boardManager.SetCardActive(card, false);
        }
    }
}

public class InputBlock {}

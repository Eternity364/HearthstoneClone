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
    private Dictionary<Card, InputBlock> cardBlocks = new Dictionary<Card, InputBlock>();

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
        cardBlocks[card] = new InputBlock();
        UpdateValues();
        return cardBlocks[card];
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
        if (cardBlocks.ContainsValue(block)) {
            var item = cardBlocks.First(kvp => kvp.Value == block);
            cardBlocks.Remove(item.Key);
        }
        UpdateValues();
    }

    public void RemoveCardBlock(Card card)
    {
        if (cardBlocks.ContainsKey(card)) {
            card.clickHandler.SetClickable(true);
            cardBlocks.Remove(card);
            UpdateValues();
        }
    }

    public void UpdateValues()
    {
        boardManager.SetInputActive(blocks.Count == 0);
        if (blocks.Count > 0)
            boardManager.DisableAttack();
        playerHand.SetCardsActive(true);
        playerHand.SetCardsActive(blocks.Count == 0 && handBlocks.Count == 0);
        if (activeCardController.pickedCard)
            playerHand.SetCardActive(activeCardController.pickedCard, blocks.Count == 0 && handBlocks.Count == 0);
        foreach(var item in cardBlocks)
        {
            Card card = item.Key;
            playerHand.SetCardActive(card, false);
        }
    }
}

public class InputBlock {}

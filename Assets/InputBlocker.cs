using System.Collections;
using System.Collections.Generic;
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
        UpdateValues();
    }

    public void UpdateValues()
    {
        if (blocks.Count > 0) {
            activeCardController.ReturnCardToHand();
        }
        boardManager.SetInputActive(blocks.Count == 0);
        if (blocks.Count > 0)
            boardManager.DisableAttack();
        playerHand.SetCardsClickable(blocks.Count == 0 && handBlocks.Count == 0);
    }
}

public class InputBlock {}

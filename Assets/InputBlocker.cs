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

    private List<InputBlock> blocks = new List<InputBlock>();

    void Awake()
    {
        InputBlockerInstace.SetInstance(this);
    }

    public InputBlock AddBlock()
    {
        InputBlock block = new InputBlock();
        blocks.Add(block);
        Update();
        return block;
    }  

    public void RemoveBlock(InputBlock block)
    {
        blocks.Remove(block);
        Update();
    }

    public void Update()
    {
        boardManager.SetInputActive(blocks.Count == 0);
        playerHand.SetCardsClickable(blocks.Count == 0);
    }
}

public class InputBlock {}

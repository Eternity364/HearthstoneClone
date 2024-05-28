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

    void Start()
    {
        InputBlockerInstace.SetInstance(this);
    }

    public void SetActive(bool value)
    {
        boardManager.SetInputActive(value);
        playerHand.SetCardsClickable(value);
    }
}

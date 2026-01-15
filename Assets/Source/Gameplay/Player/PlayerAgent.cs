using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAgent : Agent
{
    public static PlayerAgent instance;

    bool isYourTurn = false;

    public static event Action<Card> playerGotCard = (_) => { };
    public static event Action playerTurnOver = () => { };

    // Singleton. Do not allow more than one
    public PlayerAgent()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Debug.LogWarning("Did not create a second PlayerAgent.");
        }
    }

    public override void markAsReady()
    {
        isYourTurn = true;
        Debug.Log("It's the player's turn.");
    }

    public override void acquireCard(Card card)
    {
        inventory.Add(card);
        playerGotCard.Invoke(card);
        Debug.Log("The player acquires card: " + card);

        playerTurnOver.Invoke();
    }

    public override void acquireCards(List<Card> cards)
    {
        inventory.AddRange(cards);
    }

    public override void playCard(Card card)
    {

    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {

    }

    public override void useAbility()
    {

    }
}

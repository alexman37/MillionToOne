using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Agent
{
    public int id;
    public string agentName;
    public Sprite portrait;

    public int maxCardCount = 5;
    public List<Card> inventory = new List<Card>();
    public AgentAbility ability;

    public RosterConstraints rosterConstraints;

    public bool isPlayer = false;


    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    /// <summary>
    /// Give the agent a card in the initial deal
    /// </summary>
    public abstract int startingDealtCard(Card card);

    /// <summary>
    /// Give the agent a card.
    /// Return the number of cards in hand afterwards
    /// </summary>
    public abstract int acquireCard(Card card);

    /// <summary>
    /// Give the agent several cards.
    /// Return the number of cards in hand afterwards
    /// </summary>
    public abstract int acquireCards(List<Card> cards);

    /// <summary>
    /// When a clue card is declassified (shown to everyone),
    /// Update your own information automatically
    /// </summary>
    public abstract void onClueCardDeclassified(ClueCard cc);

    /// <summary>
    /// Agent uses a card
    /// </summary>
    public abstract void playCard(Card card);

    /// <summary>
    /// Agent asks another agent for information
    /// </summary>
    public abstract void askAgent(Agent asking, List<(CPD_Type, string)> inquiry);

    /// <summary>
    /// Guess one of the target's characteristics for rewards
    /// </summary>
    public virtual void guessTargetCharacteristic(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        if (wasCorrect)
        {
            rosterConstraints.onlyConstraint(cpdType, cat);
        }
        else
        {
            rosterConstraints.addConstraint(cpdType, cat, true);
        }
    }

    /// <summary>
    /// Guess one of the target's characteristics for rewards
    /// </summary>
    public abstract void guessTarget(int characterId, bool correct);

    /// <summary>
    /// Agent uses their special ability
    /// </summary>
    public abstract void useAbility();

    public virtual void clearConstraints()
    {
        // "Clear" also serves as initialization for the constraints lists if need be
        rosterConstraints = new RosterConstraints();
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            rosterConstraints.clearConstraints(cpd, true);
        }
    }

    public int getCardsCount()
    {
        return inventory.Count;
    }
}

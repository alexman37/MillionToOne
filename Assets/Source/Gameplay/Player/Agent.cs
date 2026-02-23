using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Agent
{
    public int id;
    public string agentName;
    public Sprite portrait;

    public int maxActionCardCount = 5;
    public List<ClueCard> inventory = new List<ClueCard>();
    public List<PersonCard> recruits = new List<PersonCard>();
    public AgentAbility ability;

    public int targetGuessCount = 1;
    public int askAroundCount = 1;
    public bool blocked = false;   // blocked by Escort
    public bool dead = false;      // killed by Assassain

    public RosterConstraints rosterConstraints;

    public bool isPlayer = false;

    public static event Action afterAgentSelected = () => { };


    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    /// <summary>
    /// Give the agent a card in the initial deal
    /// </summary>
    public virtual int startingDealtCard(ClueCard card)
    {
        card.owner = this;
        return inventory.Count;
    }

    /// <summary>
    /// Give the agent a card.
    /// Return the number of cards in hand afterwards
    /// </summary>
    public virtual int acquireCard(Card card)
    {
        card.owner = this;
        return inventory.Count;
    }

    /// <summary>
    /// The agent loses a card in their hand.
    /// </summary>
    public abstract void loseCard(Card card);

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
    public abstract void guessTarget(int characterId);

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

    public abstract void onBlocked();

    public abstract void onAssassinated();

    public void onAgentSelected(int id, AgentSelectReason selectionReason)
    {
        if(id == this.id)
        {
            if (selectionReason == AgentSelectReason.Escort)         onBlocked();
            else if (selectionReason == AgentSelectReason.Assassain) onAssassinated();
            afterAgentSelected.Invoke();
        }
    }
}

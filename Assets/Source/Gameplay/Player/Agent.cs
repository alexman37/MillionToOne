using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Agent
{
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
    /// Agent uses a card
    /// </summary>
    public abstract void playCard(Card card);

    /// <summary>
    /// Agent asks another agent for information
    /// </summary>
    public abstract void askAgent(Agent asking, List<(CPD_Type, string)> inquiry);

    /// <summary>
    /// Agent uses their special ability
    /// </summary>
    public abstract void useAbility();

    public abstract void clearConstraints();

    public int getCardsCount()
    {
        return inventory.Count;
    }
}

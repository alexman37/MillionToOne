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

    public bool isPlayer = false;

    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    /// <summary>
    /// Give the agent a card
    /// </summary>
    public abstract void acquireCard(Card card);

    /// <summary>
    /// Give the agent several cards
    /// </summary>
    public abstract void acquireCards(List<Card> cards);

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

    public int getCardsCount()
    {
        return inventory.Count;
    }
}

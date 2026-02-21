using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Card is an item that players hold. They serve one of two purposes
///   - Contain information (Clue card, gold card)
///   - Perform an action (Action card)
/// 
/// </summary>
public abstract class Card
{
    public CardType cardType;
    public Agent owner;

    /// <summary>
    /// Acquire the card. Notify the player to add it to their inventory, and maybe do other stuff
    /// </summary>
    public abstract void acquire(Agent agent);

    /// <summary>
    /// Acquire the card directly from another agent
    /// </summary>
    public abstract void acquireFrom(Agent receiving, Agent giving);

    /// <summary>
    /// Play the card, showing it to everyone.
    /// </summary>
    public abstract void play();
}

// Action or Gold Card
public abstract class PersonCard : Card
{
    public override string ToString()
    {
        if(this is ActionCard)
        {
            return (this as ActionCard).actionCardType.ToString();
        } else
        {
            return (this as GoldCard).goldCardType.ToString();
        }
    }
}

public enum CardType
{
    CLUE,
    ACTION,
    GOLD
}
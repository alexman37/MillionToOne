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

    /// <summary>
    /// Acquire the card. Notify the player to add it to their inventory, and maybe do other stuff
    /// </summary>
    public abstract void acquire();

    /// <summary>
    /// Play the card, showing it to everyone.
    /// </summary>
    public abstract void play();
}

public enum CardType
{
    CLUE,
    ACTION,
    GOLD
}
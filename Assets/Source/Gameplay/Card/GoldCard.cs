using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gold Cards function similarly to Action cards, but are usually much better.
/// They're also named wrong. Consider them SHADY cards or CRIMINAL cards.
/// You can only obtain them by guessing a characteristic of the target correctly-
/// revealing that information to everyone else, but, securing yourself a nice advantage.
/// </summary>
public class GoldCard : PersonCard
{
    public GoldCardType goldCardType;

    public GoldCard(GoldCard template)
    {
        goldCardType = template.goldCardType;
        cardType = CardType.GOLD;
        // Physical aspect should be handled by the player agent acquiring the card
    }

    public GoldCard(GoldCardType goldType)
    {
        cardType = CardType.GOLD;
        goldCardType = goldType;
    }

    public void setPhysical(PhysicalActionCard pac)
    {
        physical = pac;
    }

    public override void acquire(Agent agent)
    {
        owner = agent;
    }

    public override void acquireFrom(Agent receiving, Agent giving)
    {
        owner = receiving;
        giving.loseCard(this);
    }

    public override void play()
    {
        // TODO: Is it the player or not?
        switch (goldCardType)
        {
            case GoldCardType.ESCORT:
                // TODO skip 1 other player's next turn
                break;
            case GoldCardType.HACKER:
                // TODO see 3 cards of any other player
                break;
            case GoldCardType.THIEF:
                // TODO steal a card of your choice from another player
                break;
            case GoldCardType.DOUBLE_AGENT:
                // TODO reverse any action onto the player who did it
                break;
            case GoldCardType.MERCENARIES:
                // TODO guess 8 additional targets
                break;
            case GoldCardType.ASSASSAIN:
                // TODO eliminate one agent from the game
                break;
            case GoldCardType.INSIDER:
                // TODO confirm or deny your current clue form's accuracy
                break;
        }
    }

    public static GoldCard convertIntern(GoldCardType goldType, PhysicalActionCard theirPhysical)
    {
        GoldCard newCard = new GoldCard(goldType);
        theirPhysical.reinit(newCard);
        newCard.physical = theirPhysical;
        return newCard;
    }

    public override string ToString()
    {
        return "GOLD card: " + goldCardType;
    }
}


public enum GoldCardType
{
    ESCORT,                 // Skip 1 other player's next turn
    HACKER,                 // See 3 clue cards from another player
    THIEF,                  // Steal 1 card from another player
    ASSASSAIN,              // Force a player to discard an action card. If they have none, they are eliminated (for this round)
    DOUBLE_AGENT,           // Reverse any action onto the player who started it
    MERCENARIES,            // Guess 8 additional targets this turn
    INSIDER                 // Confirm or deny your current clue form's accuracy
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// We're calling these "Action Cards" in code, as that was the generic name first given to them.
/// In reality they will be called RECRUITS. (or something like that.)
/// You can use them at the right moment (when acted upon or on your turn) for their ability,
/// or, you can hold onto them for their passive ability: +1 Asking topics.
/// </summary>
public class ActionCard : PersonCard
{
    private PhysicalActionCard physical;

    public ActionCardType actionCardType;


    public ActionCard(ActionCardType actionType)
    {
        cardType = CardType.ACTION;
        actionCardType = actionType;
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
        Debug.Log("Playing action card " + ToString() + " for agent " + owner);
    }

    // The Intern card uses this to convert itself into another action card.
    public PersonCard convert(PersonCard pc)
    {
        if(pc is ActionCard)
        {
            ActionCard ac = pc as ActionCard;
            actionCardType = ac.actionCardType;
            physical.reinit(ac);
            return this;
        } else
        {
            GoldCard gc = pc as GoldCard;
            return GoldCard.convertIntern(gc.goldCardType, physical);
        }
    }

    public override string ToString()
    {
        return "Action card: " + actionCardType;
    }
}

public enum ActionCardType
{
    CENSOR,               // Redact 1 of your clue cards, making it unusable for everyone else
    SIDEKICK,             // Ask 1 other person for clues this turn
    ANALYST,              // Privately reveal 1 card from another player's hand
    LAWYER,               // Force 1 other player to declassify a clue card (with no reward)
    BODYGUARD,            // Optionally block 1 request
    ENFORCER,             // Guess 2 additional targets this turn
    INTERN                // Turn this card into any other action or gold card
}
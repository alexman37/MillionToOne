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
    public ActionCardType actionCardType;


    public ActionCard(ActionCardType actionType)
    {
        cardType = CardType.ACTION;
        actionCardType = actionType;
    }

    public override void acquire(Agent agent)
    {
        owner = agent;
    }

    public override void play()
    {
        Debug.Log("Playing action card " + ToString() + " for agent " + owner);
        // TODO: Is it the player or not?
        switch(actionCardType)
        {
            case ActionCardType.CENSOR:
                // TODO redact chosen clue card
                break;
            case ActionCardType.SIDEKICK:
                // TODO you can now ask 2 people for information
                break;
            case ActionCardType.ANALYST:
                // TODO force one other player to show a card of your choice
                break;
            case ActionCardType.LAWYER:
                // TODO force chosen player to declassify a clue card of your choice for no reward
                break;
            case ActionCardType.BODYGUARD:
                // TODO block a negative action on yourself
                break;
            case ActionCardType.ENFORCER:
                // TODO can guess 2 more suspects this turn
                break;
            case ActionCardType.INTERN:
                // TODO turn this action card into any other
                break;
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
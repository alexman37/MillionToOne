using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCard : Card
{
    private ActionCardType actionCardType;


    public ActionCard(ActionCardType actionType)
    {
        actionCardType = actionType;
    }

    void Start()
    {
        cardType = CardType.ACTION;
    }

    public override void acquire()
    {

    }

    public override void play()
    {
        // TODO: Switch case of death
    }

    public override string ToString()
    {
        return "Action card: " + actionCardType;
    }
}

public enum ActionCardType
{
    REPEAT,               // Play again this turn.
    SKIP,                 // Skip another player's next turn.
    REVEAL,               // Force a player to reveal one of their cards (their choice).
    STEAL,                // Steal another player's card (your choice).
    DRAW_2,               // Draw additional clue cards (inventory space still maintained.)
    EXTRA_INVENTORY,      // +1 inventory space for this game.
    EXTRA_ASK_AROUND,     // +1 ask around potential for this game.
    ORACLE                // Confirm or deny a characteristic; everyone will know which one, but not the yes/no result.
}
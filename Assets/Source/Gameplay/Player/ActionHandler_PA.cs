using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Handle what to do when you play an action card
// (from the player's perspective)
public static class ActionHandler_PA
{
    public static PlayerAgent playerAgent;



    // Called when the player first uses an action card -
    // Some cards require selecting an agent first, and prompting that agent for if they can react to it
    public static void handlePlayedAction(PersonCard playedCard)
    {
        if (playerAgent == null) playerAgent = PlayerAgent.instance;

        if (playedCard is ActionCard)
        {
            ActionCard ac = playedCard as ActionCard;
            switch (ac.actionCardType)
            {
                case ActionCardType.CENSOR:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.REDACTION, 1, playerAgent, 0, true);
                    break;
                case ActionCardType.SIDEKICK:
                    playerAgent.askAroundCount += 1;
                    break;
                case ActionCardType.ANALYST:
                case ActionCardType.LAWYER:
                    TurnDriver.instance.queuedCard = playedCard;
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case ActionCardType.ENFORCER:
                    playerAgent.targetGuessCount += 2;
                    break;
                case ActionCardType.INTERN:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.TAKE_COPY, 1, playerAgent, 1, true);
                    break;
            }
        }
        else if (playedCard is GoldCard)
        {
            GoldCard gc = playedCard as GoldCard;
            switch (gc.goldCardType)
            {
                case GoldCardType.ESCORT:
                case GoldCardType.ASSASSAIN:
                case GoldCardType.HACKER:
                case GoldCardType.THIEF:
                    TurnDriver.instance.queuedCard = playedCard;
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case GoldCardType.MERCENARIES:
                    playerAgent.targetGuessCount += 8;
                    break;
                case GoldCardType.INSIDER:
                    bool verified = RosterForm.instance.VerifyForm();
                    if (verified) Debug.Log("The form was correct!");
                    else Debug.Log("Something in the form was wrong");
                    break;
            }
        }
    }


    // Called when the player's target confirms they cannot respond to an action card.
    // Now they can proceed with their original intent.
    public static void handleFinalPlayedAction(PersonCard playedCard, Agent onto)
    {
        if (playerAgent == null) playerAgent = PlayerAgent.instance;

        if (playedCard is ActionCard)
        {
            ActionCard ac = playedCard as ActionCard;
            switch (ac.actionCardType)
            {
                case ActionCardType.ANALYST:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.VIEW, 1, onto, 2, false);
                    break;
                case ActionCardType.LAWYER:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.DECLASS, 1, onto, 0, false);
                    break;
            }
        }
        else if (playedCard is GoldCard)
        {
            GoldCard gc = playedCard as GoldCard;
            switch (gc.goldCardType)
            {
                case GoldCardType.ESCORT:
                    onto.onBlocked();
                    break;
                case GoldCardType.ASSASSAIN:
                    onto.onAssassinated();
                    break;
                case GoldCardType.HACKER:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.VIEW, 3, onto, 2, false);
                    break;
                case GoldCardType.THIEF:
                    SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.TAKE, 1, onto, 2, false);
                    break;
            }
        }
    }
}

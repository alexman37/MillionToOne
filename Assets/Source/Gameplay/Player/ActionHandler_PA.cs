using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle what to do when you play an action card
// (from the player's perspective)
public class ActionHandler_PA
{
    public static PlayerAgent playerAgent;

    public static void handlePlayedAction(PersonCard playedCard)
    {
        if (playerAgent == null) playerAgent = PlayerAgent.instance;

        // Most of the time when we have to select an agent, we take one of their cards
        // Assume that until proven otherwise
        AgentDisplay.selectionReason = AgentSelectReason.CardSelect;

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
                    SelectionWindow.instance.prepareForDisplay(SelectionWindow.SelectionCardOutcome.VIEW, 1, 2, false);
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case ActionCardType.LAWYER:
                    SelectionWindow.instance.prepareForDisplay(SelectionWindow.SelectionCardOutcome.DECLASS, 1, 0, false);
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
                    AgentDisplay.selectionReason = AgentSelectReason.Escort;
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case GoldCardType.ASSASSAIN:
                    AgentDisplay.selectionReason = AgentSelectReason.Assassain;
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case GoldCardType.MERCENARIES:
                    playerAgent.targetGuessCount += 8;
                    break;
                case GoldCardType.HACKER:
                    SelectionWindow.instance.prepareForDisplay(SelectionWindow.SelectionCardOutcome.VIEW, 3, 2, false);
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case GoldCardType.THIEF:
                    SelectionWindow.instance.prepareForDisplay(SelectionWindow.SelectionCardOutcome.TAKE, 1, 2, false);
                    Total_UI.instance.changeUIState(Current_UI_State.AgentSelection);
                    break;
                case GoldCardType.INSIDER:
                    bool verified = RosterForm.instance.VerifyForm();
                    if (verified) Debug.Log("The form was correct!");
                    else Debug.Log("Something in the form was wrong");
                    break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ActionHandler_CPU
{
    // Will have to pass in the agent who calls upon this each time.


    // Called when the player first uses an action card -
    // Some cards require selecting an agent first, and prompting that agent for if they can react to it
    public static void handlePlayedAction(CPUAgent whosPlaying, PersonCard playedCard)
    {
        bool stillEndTurn = true;

        Agent bestTarget = whosPlaying.agentLogic.actionLogic.getBestTarget(playedCard);

        if (playedCard is ActionCard)
        {
            ActionCard ac = playedCard as ActionCard;
            Card bestCardToPlayOn;

            switch (ac.actionCardType)
            {
                case ActionCardType.CENSOR:
                    bestCardToPlayOn = whosPlaying.agentLogic.actionLogic.getBestCardToRedact(whosPlaying);
                    ClueCard cc = bestCardToPlayOn as ClueCard;
                    cc.redact();
                    whosPlaying.playCard(cc);
                    break;
                case ActionCardType.SIDEKICK:
                    whosPlaying.askAroundCount += 1;
                    stillEndTurn = false;
                    break;
                case ActionCardType.ANALYST:
                case ActionCardType.LAWYER:
                    TurnDriver.instance.queuedCard = playedCard;
                    bestTarget.promptForReaction(ac);
                    stillEndTurn = false;
                    // ... wait for reaction from CPU or player
                    break;
                case ActionCardType.ENFORCER:
                    whosPlaying.targetGuessCount += 2;
                    stillEndTurn = false;
                    break;
                case ActionCardType.INTERN:
                    bestCardToPlayOn = whosPlaying.agentLogic.actionLogic.getBestCardToCopy(whosPlaying);
                    PersonCard data = bestCardToPlayOn as PersonCard;
                    if (data.cardType == CardType.ACTION)
                    {
                        ActionCard copy = new ActionCard(data as ActionCard);
                        data.acquire(whosPlaying);
                        whosPlaying.acquireCard(copy);
                    }
                    else if (data.cardType == CardType.GOLD)
                    {
                        GoldCard copy = new GoldCard(data as GoldCard);
                        data.acquire(whosPlaying);
                        whosPlaying.acquireCard(copy);
                    }
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
                    bestTarget.promptForReaction(gc);
                    stillEndTurn = false;
                    // ... wait for reaction from CPU or player
                    break;
                case GoldCardType.MERCENARIES:
                    whosPlaying.targetGuessCount += 8;
                    stillEndTurn = false;
                    break;
                case GoldCardType.INSIDER:
                    Debug.LogError("CPU cannot play insider yet.");
                    // TODO confirm or deny some pre-existing suspicions the CPU has.
                    break;
            }
        }
        if (stillEndTurn)
        {
            whosPlaying.endOfTurn();
        }
    }

    // Called when the player's target confirms they cannot respond to an action card.
    // Now they can proceed with their original intent.
    public static void handleFinalPlayedAction(CPUAgent whosPlaying, PersonCard playedCard, Agent onto)
    {
        Card cardPicked;

        if (playedCard is ActionCard)
        {
            ActionCard ac = playedCard as ActionCard;

            switch (ac.actionCardType)
            {
                case ActionCardType.ANALYST:
                    cardPicked = whosPlaying.agentLogic.actionLogic.pickSemirandomCard(onto, 2);
                    // TODO The CPU either learns a clue card (and info) or an action card and updates threat
                    break;
                case ActionCardType.LAWYER:
                    cardPicked = whosPlaying.agentLogic.actionLogic.pickSemirandomCard(onto, 0);
                    whosPlaying.onClueCardDeclassified(cardPicked as ClueCard);
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
                    List<Card> cardsPicked;
                    cardsPicked = whosPlaying.agentLogic.actionLogic.pickSemirandomCards(3, onto, 2);
                    // TODO The CPU either learns a clue card (and info) or an action card and updates threat
                    break;
                case GoldCardType.THIEF:
                    cardPicked = whosPlaying.agentLogic.actionLogic.pickSemirandomCard(onto, 2);
                    cardPicked.owner.loseCard(cardPicked);
                    cardPicked.acquire(whosPlaying);
                    whosPlaying.acquireCard(cardPicked);
                    break;
            }
        }
        Debug.Log("Agent " + whosPlaying.agentName + "'s turn is over 2");
        whosPlaying.endOfTurn();
    }
}

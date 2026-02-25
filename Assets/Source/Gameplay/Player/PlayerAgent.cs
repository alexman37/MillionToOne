using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAgent : Agent
{
    public static PlayerAgent instance;

    bool isYourTurn = false;

    public static event Action<Card, int> playerGotCard = (_,n) => { };
    public static event Action<Card, int> playerLostCard = (_,n) => { };
    public static event Action<Card> updateFormWithCard = (_) => { };
    public static event Action<int> playerUpdateProgress = (_) => { };
    public static event Action playerTurnOver = () => { };


    // Singleton. Do not allow more than one
    public PlayerAgent()
    {
        // TODO player's name
        agentName = "Player";

        id = 0; // There can be only one...?

        if(instance == null)
        {
            instance = this;
        } else
        {
            Debug.LogWarning("Did not create a second PlayerAgent.");
        }

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        ClueCard.clueCardDeclassified += onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty += guessTargetCharacteristic;
        CharacterCard.charCardClicked += guessTarget;
        RosterForm.askAroundCompleted += completedAskAround;
        AgentDisplay.selectedAgent_AS += onAgentSelected;
    }

    ~PlayerAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
        CharacterCard.charCardClicked -= guessTarget;
        RosterForm.askAroundCompleted -= completedAskAround;
        AgentDisplay.selectedAgent_AS -= onAgentSelected;
    }

    public override void markAsReady()
    {
        if (dead)
        {
            Debug.LogWarning("Skipped player's turn, they are dead");
            endOfTurn();
            return;
        }
        if (blocked)
        {
            Debug.LogWarning("Skipped player's turn, they are blocked.");
            blocked = false;
            endOfTurn();
            return;
        }

        askAroundCount = 1;
        targetGuessCount = 1;
        isYourTurn = true;
        Debug.Log("It's the player's turn.");
        Total_UI.instance.changeUIState(Current_UI_State.PlayerTurn);
    }

    public override int startingDealtCard(ClueCard card)
    {
        base.startingDealtCard(card);

        inventory.Add(card);
        playerGotCard.Invoke(card, inventory.Count);
        updateFormWithCard.Invoke(card);

        updateConstraintsFromCard(card);
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        base.acquireCard(card);

        if(card.cardType == CardType.CLUE)
        {
            ClueCard cc = card as ClueCard;
            inventory.Add(cc);

            updateConstraintsFromCard(cc);
            playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

            playerGotCard.Invoke(card, inventory.Count);
        } else
        {
            PersonCard pc = card as PersonCard;
            recruits.Add(pc);

            playerGotCard.Invoke(card, recruits.Count);
        }

        updateFormWithCard.Invoke(card);
        Debug.Log("The player acquires card: " + card);

        return card.cardType == CardType.CLUE ? inventory.Count : recruits.Count;
    }

    public override void loseCard(Card card)
    {
        if(card is ClueCard)
        {
            ClueCard cc = card as ClueCard;
            int cardex = inventory.IndexOf(cc);
            inventory.RemoveAt(cardex);
            playerLostCard.Invoke(cc, cardex);
        } else
        {
            PersonCard pc = card as PersonCard;
            int cardex = recruits.IndexOf(pc);
            foreach(PersonCard poc in recruits)
            {
                Debug.Log("*** FOUND person " + poc);
            }
            Debug.Log("*** Cardex " + cardex);
            recruits.RemoveAt(cardex);
            playerLostCard.Invoke(pc, cardex);
        }
    }

    public override void playCard(Card card)
    {
        if(card.cardType == CardType.CLUE)
        {
            ClueCard clueCard = card as ClueCard;
            // Gameplay result depends on what the card is - clue or action
            clueCard.play();

            // Earn an action card
            if(!clueCard.redacted)
                TurnDriver.instance.dealActionCard();

            loseCard(card);

            endOfTurn();
        } 
        else
        {
            PersonCard pc = card as PersonCard;

            bool willLoseCard = true;

            pc.play();
            loseCard(card);

            // Most of the time when we have to select an agent, we take one of their cards
            // Assume that until proven otherwise
            AgentDisplay.selectionReason = AgentSelectReason.CardSelect;

            if (pc is ActionCard)
            {
                ActionCard ac = pc as ActionCard;
                switch (ac.actionCardType)
                {
                    case ActionCardType.CENSOR:
                        SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.REDACTION, 1, this, 0, true);
                        break;
                    case ActionCardType.SIDEKICK:
                        askAroundCount += 1;
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
                        targetGuessCount += 2;
                        break;
                    case ActionCardType.INTERN:
                        SelectionWindow.instance.displaySelection(SelectionWindow.SelectionCardOutcome.TAKE_COPY, 1, this, 1, true);
                        break;
                }
            }
            else if(pc is GoldCard)
            {
                GoldCard gc = pc as GoldCard;
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
                        targetGuessCount += 8;
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

    public override void onClueCardDeclassified(ClueCard cc)
    {
        if(!cc.redacted)
        {
            updateConstraintsFromCard(cc);
            updateFormWithCard.Invoke(cc);
            playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        }
    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {
        // Assume the agent you are asking is always a CPU.
        CPUAgent cAgent = asking as CPUAgent;
        foreach((CPD_Type, string) category in inquiry)
        {
            // TODO : Update information, end turn
            if (cAgent.infoTracker.HasCardFor(category))
            {
                Debug.Log("The asked CPU has: " + category);
            }
        }
    }

    public override void guessTargetCharacteristic(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        base.guessTargetCharacteristic(cpdType, cat, wasCorrect);

        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        if (wasCorrect)
        {
            // Success! Everyone knows it now, but you get cool rewards
            Debug.Log("You were correct");
        }

        else
        {
            // Fail - everyone else knows what you guessed is not it, and your turn is over
            Debug.Log("Not right");
        }

        endOfTurn();
    }

    public override void guessTarget(int characterId)
    {
        bool correct = TurnDriver.instance.currentRoster.targetId == characterId;
        if(targetGuessCount > 0)
        {
            targetGuessCount--;
            // TODO obv. gotta do more than just click/respond
            if (correct)
            {
                Debug.Log("YOU WIN!");
                // TODO
            }
            else
            {
                Debug.Log("Wrong guy!");
                if(targetGuessCount == 0)
                    endOfTurn();
            }
        } else
        {
            Debug.LogWarning("Out of target guesses. The turn should have ended already.");
        }
    }

    public override void useAbility()
    {

    }

    public override void endOfTurn()
    {
        playerTurnOver.Invoke();
    }

    // CPU handles their constraints locally.
    private void updateConstraintsFromCard(Card receivedCard)
    {
        if (receivedCard is ClueCard)
        {
            // TODO CPU may have to distinguish between guaranteed facts and guesses, so "lock" these constraints in
            ClueCard cc = receivedCard as ClueCard;
            if (cc.onTarget)
            {
                rosterConstraints.onlyConstraint(cc.cpdType, cc.category);
            }
            else
            {
                rosterConstraints.addConstraint(cc.cpdType, cc.category, true);
            }
        }
    }

    public override void onBlocked()
    {
        Debug.Log("Blocked player");
        blocked = true;
    }

    public override void onAssassinated()
    {
        if(recruits.Count > 0)
        {
            Debug.Log("Forced player to give up an action card");
            // TODO card select
        } else
        {
            Debug.Log("Eliminated player");
            dead = true;
        }
    }




    // Misc

    void completedAskAround()
    {
        askAroundCount -= 1;
    }
}

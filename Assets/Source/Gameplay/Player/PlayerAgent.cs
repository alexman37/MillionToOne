using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAgent : Agent
{
    public static PlayerAgent instance;

    public static event Action<Card, int> playerGotCard = (_,n) => { };
    public static event Action<Card, int> playerLostCard = (_,n) => { };
    public static event Action<Card> updateFormWithCard = (_) => { };
    public static event Action<int> playerUpdateProgress = (_) => { };
    public static event Action playerTurnOver = () => { };


    public PlayerAgent()
    {
        // TODO player's name
        agentName = "Player";

        id = 0;

        // Singleton. Do not allow more than one
        if (instance == null)
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
        Roster.guessedWrongCharacter += guessTarget;
        RosterForm.askAroundCompleted += completedAskAround;
        AgentDisplay.selectedAgent_AS += onAgentSelected;
    }

    ~PlayerAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
        Roster.guessedWrongCharacter -= guessTarget;
        RosterForm.askAroundCompleted -= completedAskAround;
        AgentDisplay.selectedAgent_AS -= onAgentSelected;
    }

    // Initial actions before the player's turn.
    public override void markAsReady()
    {
        // Skip forever if dead. Skip once if blocked.
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

    // Dealt a card at the start of the game
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

    // Acquires a card at any point in the game
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

    // Lose/use a card at any point: remove it
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
            recruits.RemoveAt(cardex);
            playerLostCard.Invoke(pc, cardex);
        }
    }

    // Use a card
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

            pc.play();
            loseCard(card);

            ActionHandler_PA.handlePlayedAction(pc);
        }
        
    }

    // When a clue card anywhere has been declassified
    public override void onClueCardDeclassified(ClueCard cc)
    {
        if(!cc.redacted)
        {
            updateConstraintsFromCard(cc);
            updateFormWithCard.Invoke(cc);
            playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        }
    }

    // Ask an agent for information
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

    // Guess target characteristic
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

    // When a target has been guessed, do these actions
    // Some are performed only if it's your turn
    public override void guessTarget(int characterId)
    {
        if(isYourTurn)
        {
            bool correct = TurnDriver.instance.currentRoster.targetId == characterId;
            if (targetGuessCount > 0)
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
                    if (targetGuessCount == 0)
                        endOfTurn();
                }
            }
            else
            {
                Debug.LogWarning("Out of target guesses. The turn should have ended already.");
            }
        }
    }

    // Use ability
    public override void useAbility()
    {

    }

    // When turn is over do these actions
    public override void endOfTurn()
    {
        isYourTurn = false;
        playerTurnOver.Invoke();
    }

    // Ask this player if they would like to use a reaction card to block an action on them
    // (even if they don't actually have anything for it)
    public override void promptForReaction(PersonCard withCard)
    {
        Debug.Log("Player prompted for action");
        SelectionWindow.instance.displayReaction(withCard, this);
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

    // When blocked, do this
    public override void onBlocked()
    {
        Debug.Log("Blocked player");
        blocked = true;
    }

    // When assassinated, do this
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

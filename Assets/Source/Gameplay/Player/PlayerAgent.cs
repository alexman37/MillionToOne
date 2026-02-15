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

        Roster.clearAllConstraints += clearConstraints;
        ClueCard.clueCardDeclassified += onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty += guessTargetCharacteristic;
        Agent.targetCharacteristicGuess += onTargetCardRevealed;
    }

    ~PlayerAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
        Agent.targetCharacteristicGuess -= onTargetCardRevealed;
    }

    public override void markAsReady()
    {
        isYourTurn = true;
        Debug.Log("It's the player's turn.");
    }

    public override int startingDealtCard(Card card)
    {
        inventory.Add(card);
        playerGotCard.Invoke(card, inventory.Count);

        updateConstraintsFromCard(card);
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        inventory.Add(card);
        playerGotCard.Invoke(card, inventory.Count);
        Debug.Log("The player acquires card: " + card);

        updateConstraintsFromCard(card);
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        playerTurnOver.Invoke();
        return inventory.Count;
    }

    public override int acquireCards(List<Card> cards)
    {
        inventory.AddRange(cards);
        return inventory.Count;
    }

    public override void playCard(Card card)
    {
        // Gameplay result depends on what the card is - clue or action
        card.play();

        // For the player, just remove it from their inventory
        int cardex = inventory.IndexOf(card);
        inventory.RemoveAt(cardex);
        playerLostCard.Invoke(card, cardex);

        playerTurnOver.Invoke();
    }

    public override void onClueCardDeclassified(ClueCard cc)
    {
        updateConstraintsFromCard(cc);
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {
        // Assume the agent you are asking is always a CPU.
        CPUAgent cAgent = asking as CPUAgent;
        foreach((CPD_Type, string) category in inquiry)
        {
            if (cAgent.rosterLogic.HasCardFor(category))
            {
                Debug.Log("The asked CPU has: " + category);
            }
        }
    }

    public void guessTargetCharacteristic(CPD_Type cpdType, string cat)
    {
        if (TurnDriver.instance.currentRoster.targetHasProperty(cpdType, cat))
        {
            base.guessTargetCharacteristic(cpdType, cat, true);

            // Success! Everyone knows it now, but you get cool rewards
            Debug.Log("You were correct");

            playerTurnOver.Invoke();
        }

        else
        {
            // Fail - everyone else knows what you guessed is not it, and your turn is over
            Debug.Log("Not right");

            base.guessTargetCharacteristic(cpdType, cat, false);

            playerTurnOver.Invoke();
        }
    }

    public override void onTargetCardRevealed(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        base.onTargetCardRevealed(cpdType, cat, wasCorrect);
        
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
    }

    public override void useAbility()
    {

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
                rosterConstraints.addConstraint(cc.cpdType, cc.category);
            }
        }
    }
}

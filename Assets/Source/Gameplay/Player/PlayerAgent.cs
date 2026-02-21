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

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        ClueCard.clueCardDeclassified += onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty += guessTargetCharacteristic;
        CharacterCard.charCardClicked += guessTarget;
    }

    ~PlayerAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
        CharacterCard.charCardClicked -= guessTarget;
    }

    public override void markAsReady()
    {
        isYourTurn = true;
        Debug.Log("It's the player's turn.");
    }

    public override int startingDealtCard(ClueCard card)
    {
        inventory.Add(card);
        playerGotCard.Invoke(card, inventory.Count);

        updateConstraintsFromCard(card);
        playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        if(card.cardType == CardType.CLUE)
        {
            ClueCard cc = card as ClueCard;
            inventory.Add(cc);

            updateConstraintsFromCard(cc);
            playerUpdateProgress.Invoke(TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        } else
        {
            PersonCard pc = card as PersonCard;
            recruits.Add(pc);
        }

        playerGotCard.Invoke(card, recruits.Count);
        Debug.Log("The player acquires card: " + card);

        return inventory.Count;

    }

    public override int acquireCards(List<Card> cards)
    {
        throw new NotImplementedException();
    }

    public override void playCard(Card card)
    {
        if(card.cardType == CardType.CLUE)
        {
            ClueCard clueCard = card as ClueCard;
            // Gameplay result depends on what the card is - clue or action
            clueCard.play();

            // For the player, just remove it from their inventory
            int cardex = inventory.IndexOf(clueCard);
            inventory.RemoveAt(cardex);
            playerLostCard.Invoke(clueCard, cardex);

            playerTurnOver.Invoke();
        } 
        else
        {
            PersonCard pc = card as PersonCard;
            int cardex = recruits.IndexOf(pc);
            recruits.RemoveAt(cardex);
            playerLostCard.Invoke(pc, cardex);
        }
        
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

        playerTurnOver.Invoke();
    }

    public override void guessTarget(int characterId, bool correct)
    {
        // TODO obv. gotta do more than just click/respond
        if(correct)
        {
            Debug.Log("YOU WIN!");
            // TODO
        } else
        {
            Debug.Log("Wrong guy! Your turn is over");
            playerTurnOver.Invoke();
        }
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
                rosterConstraints.addConstraint(cc.cpdType, cc.category, true);
            }
        }
    }
}

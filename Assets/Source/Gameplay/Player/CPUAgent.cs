using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CPUAgent : Agent
{
    // Tracks all relevant info this CPU would need to make decisions.
    public CPUInfoTracker infoTracker;
    // Algorithm for deciding what to do on your turn.
    public CPUAgentLogic agentLogic;

    public static event Action<int, Card, int> cpuGotCard = (_,__,n) => { };
    public static event Action<int, int> cpuUpdateProgress = (_, __) => { };
    public static event Action cpuTurnOver = () => { };

    public CPUAgent(int id, string name)
    {
        this.id = id;
        agentName = name;

        infoTracker = new CPUInfoTracker(this);
        agentLogic = new CPUAgentLogic(this);

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        ClueCard.clueCardDeclassified += onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty += guessTargetCharacteristic;
    }

    ~CPUAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
    }

    public override void markAsReady()
    {
        Debug.Log("It's CPU player " + agentName + "'s turn.");

        // TODO Do the thing with animations...
        agentLogic.processTurn();
    }

    public override int startingDealtCard(Card card)
    {
        inventory.Add(card);
        cpuGotCard.Invoke(id, card, inventory.Count);

        infoTracker.AddedCardToHand(card);

        updateConstraintsFromCard(card);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        inventory.Add(card);
        cpuGotCard.Invoke(id, card, inventory.Count);

        infoTracker.AddedCardToHand(card);

        updateConstraintsFromCard(card);

        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        Debug.Log("CPU player " + agentName + " acquires card: " + card);
        return inventory.Count;
    }

    public override int acquireCards(List<Card> cards)
    {
        throw new NotImplementedException();
    }

    public override void playCard(Card card)
    {
        // Gameplay result depends on what the card is - clue or action
        card.play();

        Debug.Log("CPU declassified " + card);

        // For the player, just remove it from their inventory
        inventory.Remove(card);

        cpuTurnOver.Invoke();
    }

    public override void onClueCardDeclassified(ClueCard cc)
    {
        infoTracker.MarkDefinitive(cc.cpdType, cc.category, cc.onTarget);

        updateConstraintsFromCard(cc);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {

    }

    public override void guessTargetCharacteristic(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        base.guessTargetCharacteristic(cpdType, cat, wasCorrect);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        if(wasCorrect)
        {
            infoTracker.solvedCPDs.Add(cpdType);
        }
    }

    public void guessTargetCharacteristic(CPD_Type cpdType, string cat)
    {
        bool wasCorrect = TurnDriver.instance.currentRoster.targetHasProperty(cpdType, cat);
        guessTargetCharacteristic(cpdType, cat, wasCorrect);
    }

    public override void guessTarget(int characterId, bool correct)
    {
        
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
            } else
            {
                rosterConstraints.addConstraint(cc.cpdType, cc.category, true);
            }
        }
    }





    // CPU-specific methods

    public void skipTurn()
    {
        Debug.Log("Skipping CPU " + agentName + "'s turn.");
        cpuTurnOver.Invoke();
    }
}
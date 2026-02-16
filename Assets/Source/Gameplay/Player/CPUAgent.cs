using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CPUAgent : Agent
{
    // Tracks all relevant info this CPU would need to make decisions.
    // TODO how much do we use constraints, how much roster logic?
    public CPURosterLogic rosterLogic = new CPURosterLogic();

    public static event Action<int, Card, int> cpuGotCard = (_,__,n) => { };
    public static event Action<int, int> cpuUpdateProgress = (_, __) => { };
    public static event Action cpuTurnOver = () => { };

    public CPUAgent(int id, string name)
    {
        this.id = id;
        agentName = name;

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
        deleteMe();
    }

    public override int startingDealtCard(Card card)
    {
        inventory.Add(card);
        cpuGotCard.Invoke(id, card, inventory.Count);

        rosterLogic.AddedCardToHand(card);
        updateConstraintsFromCard(card);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        inventory.Add(card);
        cpuGotCard.Invoke(id, card, inventory.Count);

        rosterLogic.AddedCardToHand(card);
        updateConstraintsFromCard(card);

        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        Debug.Log("CPU player " + agentName + " acquires card: " + card);
        return inventory.Count;
    }

    public override int acquireCards(List<Card> cards)
    {
        inventory.AddRange(cards);
        return inventory.Count;
    }

    public override void playCard(Card card)
    {

    }

    public override void onClueCardDeclassified(ClueCard cc)
    {
        updateConstraintsFromCard(cc);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {

    }

    public new void guessTargetCharacteristic(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        base.guessTargetCharacteristic(cpdType, cat, wasCorrect);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
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


    // TODO REMOVE ALL
    private void deleteMe()
    {
        Debug.LogWarning("CPU Turn not implemented yet");

        cpuTurnOver.Invoke();
    }
}




// TODO...
/*public class CPUAgentLogic
{
    
}*/
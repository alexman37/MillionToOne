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
    // The CPU's unique set of personality traits, which changes many things about its behavior
    // (See CPUAgentLogic for definition)
    public CPUPersonalityStats personalityStats;

    public static event Action<int, Card, int> cpuGotCard = (_,__,n) => { };
    public static event Action<int, int> cpuUpdateProgress = (_, __) => { };
    public static event Action cpuTurnOver = () => { };

    public static event Action<Agent, Agent, List<(CPD_Type, string)>>       cpuAskAround_Made              = (a,b,c) => { };
    public static event Action<Agent, Agent, List<(CPD_Type, string)>, int>  cpuAskAround_Response_Show     = (a,b,c,d) => { };
    public static event Action<ClueCard>                                     cpuAskAround_Response_Declass  = (a) => { };

    public static event Action<Agent, Agent, ReactionVerdict> cpuReacts = (a1,a2,v) => { };

    public CPUAgent(int id, string name)
    {
        this.id = id;
        agentName = name;

        personalityStats = new CPUPersonalityStats(false);
        infoTracker = new CPUInfoTracker(this);
        agentLogic = new CPUAgentLogic(this);

        infoTracker.askAroundMatrix.getGameData();

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        ClueCard.clueCardDeclassified += onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty += guessTargetCharacteristic;
        AgentDisplay.selectedAgent_AS += onAgentSelected;
        PlayerAgent.playerAskAround_Response_Show += onOutsideAskAroundResult;
        CPUAgent.cpuAskAround_Response_Show += onOutsideAskAroundResult;
    }

    ~CPUAgent()
    {
        Roster.clearAllConstraints -= clearConstraints;
        ClueCard.clueCardDeclassified -= onClueCardDeclassified;
        TargetCharGuess.playerGuessesTargetProperty -= guessTargetCharacteristic;
        AgentDisplay.selectedAgent_AS -= onAgentSelected;
        PlayerAgent.playerAskAround_Response_Show -= onOutsideAskAroundResult;
        CPUAgent.cpuAskAround_Response_Show -= onOutsideAskAroundResult;
    }

    public override void markAsReady()
    {
        Total_UI.instance.changeUIState(Current_UI_State.CPUTurn);

        if (dead)
        {
            Debug.LogWarning("Skipped CPU " + agentName + "'s turn, they are dead");
            endOfTurn();
            return;
        } 
        else if(blocked)
        {
            Debug.LogWarning("Skipped CPU " + agentName + "'s turn, they are blocked.");
            blocked = false;
            endOfTurn();
            return;
        }

        isYourTurn = true;
        askAroundCount = 1;
        targetGuessCount = 1;

        Debug.Log("It's CPU player " + agentName + "'s turn.");

        // Wait for animation manager to finish its thing - process logic over there when complete
        AnimationManager.instance.BeginTurnForCPU(this);
    }

    public override int startingDealtCard(ClueCard card)
    {
        base.startingDealtCard(card);

        inventory.Add(card);
        cpuGotCard.Invoke(id, card, inventory.Count);

        infoTracker.AddedCardToHand(card);

        updateConstraintsFromCard(card);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));

        return inventory.Count;
    }

    public override int acquireCard(Card card)
    {
        base.acquireCard(card);

        if (card.cardType == CardType.CLUE)
        {
            ClueCard cc = card as ClueCard;
            inventory.Add(cc);

            infoTracker.AddedCardToHand(cc);

            updateConstraintsFromCard(cc);

            cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        } else
        {
            PersonCard pc = card as PersonCard;
            recruits.Add(pc);
        }
        cpuGotCard.Invoke(id, card, recruits.Count);
        Debug.Log("CPU player " + agentName + " acquires card: " + card);

        return card.cardType == CardType.CLUE ? inventory.Count : recruits.Count;
    }

    public override void loseCard(Card card)
    {
        if (card is ClueCard)
        {
            ClueCard cc = card as ClueCard;
            int cardex = inventory.IndexOf(cc);
            inventory.RemoveAt(cardex);
            infoTracker.RemovedCardFromHand(cc);
        }
        else
        {
            PersonCard pc = card as PersonCard;
            int cardex = recruits.IndexOf(pc);
            recruits.RemoveAt(cardex);
        }
    }

    public override void playCard(Card card)
    {
        loseCard(card);

        if(card.cardType == CardType.CLUE)
        {
            ClueCard clueCard = card as ClueCard;
            // Gameplay result depends on what the card is - clue or action
            clueCard.play();

            Debug.Log("CPU declassified " + clueCard);

            endOfTurn();
        }
        else
        {
            PersonCard pc = card as PersonCard;

            pc.play();

            ActionHandler_CPU.handlePlayedAction(this, pc);
        }
    }

    public override void onClueCardDeclassified(ClueCard cc)
    {
        if (!cc.redacted)
        {
            infoTracker.MarkDefinitive(cc.cpdType, cc.category, cc.onTarget);

            updateConstraintsFromCard(cc);
            cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        }
    }

    public override List<(CPD_Type, string)> findInventoryOverlap(List<(CPD_Type, string)> inquiry)
    {
        List<(CPD_Type, string)> overlap = new List<(CPD_Type, string)>();
        foreach ((CPD_Type, string) topic in inquiry)
        {
            if (infoTracker.catsInHand.Contains(topic))
            {
                overlap.Add(topic);
            }
        }
        return overlap;
    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {
        // Must make sure this is only done once per ask-around
        AAMatrix.MarkInAskAroundCount(this.id, inquiry);

        asking.askedAbout(this, inquiry);

        cpuAskAround_Made.Invoke(this, asking, inquiry);
    }

    public override void askedAbout(Agent askedBy, List<(CPD_Type, string)> inquiry)
    {
        // 1. Find overlap between what cards were asked for and what cards you have
        List<(CPD_Type, string)> overlap = findInventoryOverlap(inquiry);

        // 2. Decide whether to declassify 1 card or show all of them to asker
        if (overlap.Count > 0)
        {
            (bool declass, CPD_Type cpdType, string cat) calc = agentLogic.onAskedAbout(askedBy, overlap);
            if (calc.declass)
            {
                Debug.LogWarning("CPU decided to declassify a card instead of showing you!");
                ClueCard cc = inventory.Find(cc => cc.cpdType == calc.cpdType && cc.category == calc.cat);
                Debug.Log("CPU should have card with " + calc.cpdType + ", " + calc.cat);
                Debug.Log("Found " + cc);
                // TODO: Declassify without the reward
                playCard(cc);
                cpuAskAround_Response_Declass.Invoke(cc);
            }
            else
            {
                Debug.LogWarning("CPU shows you cards");
                askedBy.learnedFromAA(this, overlap);
                cpuAskAround_Response_Show.Invoke(askedBy, this, inquiry, overlap.Count);
            }
        }
        else
        {
            Debug.LogWarning("Nothing to show...moving on.");
            askedBy.learnedFromAA(this, overlap);
            cpuAskAround_Response_Show.Invoke(askedBy, this, inquiry, 0);
        }
    }

    public override void learnedFromAA(Agent learnedFrom, List<(CPD_Type, string)> topics)
    {
        foreach((CPD_Type, string) topic in topics)
        {
            infoTracker.MarkDefinitive(topic.Item1, topic.Item2, false);
            // TODO will it eventually support "yes" cards?
        }
        endOfTurn();
    }

    public override void onOutsideAskAroundResult(Agent askedBy, Agent askedTo, List<(CPD_Type, string)> inquiry, int numCorrect)
    {
        if(id != askedBy.id && id != askedTo.id)
        {
            infoTracker.ProcessOutsideAA(askedBy, askedTo, inquiry, numCorrect);
        }
    }

    public override void guessTargetCharacteristic(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        base.guessTargetCharacteristic(cpdType, cat, wasCorrect);
        Debug.Log("Guessing target property " + cpdType);
        cpuUpdateProgress.Invoke(id, TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(rosterConstraints));
        if(wasCorrect)
        {
            Debug.Log("And they were correct!");
            infoTracker.CPDRevealed(cpdType, cat);
            TurnDriver.instance.giveReward(0, Roster.cpdByType[cpdType].getGuessReward());
        }
        endOfTurn();
    }

    public void guessTargetCharacteristic(CPD_Type cpdType, string cat)
    {
        bool wasCorrect = TurnDriver.instance.currentRoster.targetHasProperty(cpdType, cat);
        guessTargetCharacteristic(cpdType, cat, wasCorrect);
    }

    public override void guessTarget(int characterId)
    {
        bool correct = TurnDriver.instance.currentRoster.targetId == characterId;
        if (targetGuessCount > 0)
        {
            targetGuessCount--;
            // TODO obv. gotta do more than just click/respond
            if (correct)
            {
                Debug.Log("CPU WINS!");
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

    public override void useAbility()
    {

    }

    public override void endOfTurn()
    {
        Debug.Log("The CPU " + agentName + "'s turn has ended.");
        isYourTurn = false;
        cpuTurnOver.Invoke();
    }

    public override void promptForReaction(PersonCard withCard)
    {
        Debug.Log("CPU prompted for action");
        // TODO
        cpuReacts.Invoke(withCard.owner, this, ReactionVerdict.ALLOW);
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

    public override void onBlocked()
    {
        Debug.Log("Blocked player");
        blocked = true;
    }

    public override void onAssassinated()
    {
        if (recruits.Count > 0)
        {
            Debug.Log("Forced player to give up an action card");
            // TODO card select
        }
        else
        {
            Debug.Log("Eliminated player");
            dead = true;
        }
    }





    // CPU-specific methods

    public void skipTurn()
    {
        Debug.Log("Skipping CPU " + agentName + "'s turn.");
        endOfTurn();
    }
}
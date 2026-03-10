using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Random;

/// <summary>
/// CPU Agent Logic determines how CPU Agents make their decisions.
/// 
/// Here's the short answer: It ranks every possible action the CPU Agent could take based on
/// how much sense it actually makes (given their inventory and information), with a bias towards
/// certain actions dependent on the CPU's "personality traits". For example, a risk-taking CPU
/// will be more likely to guess a target characteristic for a reward, or guess the target outright.
/// 
/// Once all the actions are ranked, one is chosen based on the CPU's intelligence intelligence and randomness traits.
/// A CPU with max intelligence and zero randomness will perform the "best" action every time, making them very competent
/// but also very predictable.
/// </summary>
public class CPUAgentLogic
{
    CPUAgent selfAgent;
    CPUInfoTracker infoTracker;
    public CPU_AL_ActionLogic actionLogic { get; }

    // CPU's current ordering of how good (or bad) it thinks each possible action would be.
    private List<LogicAction> rankedLogicActions;

    public CPUAgentLogic(CPUAgent agent)
    {
        selfAgent = agent;
        infoTracker = agent.infoTracker;
        actionLogic = new CPU_AL_ActionLogic(selfAgent);

        rankedLogicActions = new List<LogicAction>();
    }


    // ------------------------------
    // Standard Decision-making...
    // ------------------------------


    /// <summary>
    /// Begin calculating the CPU agent's turn
    /// </summary>
    public void processTurn()
    {
        rankedLogicActions.Clear();

        aggregateOptions();
    }


    /// <summary>
    /// Look through your inventory and known information, and put together the actions list.
    /// </summary>
    private void aggregateOptions()
    {
        // 1. Skipping your turn, doing nothing.
        rankedLogicActions.Add(
            new LogicAction(LogicActionType.Nothing, 0)
        );

        foreach (ClueCard cc in selfAgent.inventory)
        {
            // 2. Declassifying a clue card to get an action card.
            rankedLogicActions.Add(
                // TODO this probably differs from declassifying in responding to ask around
                new LogicAction_Declassify(5 + infoTracker.askAroundMatrix.getDeclassifyScore((cc.cpdType, cc.category)), cc)
            );
        }

        foreach (PersonCard pc in selfAgent.recruits)
        {
            // 3. Using an action card.
            rankedLogicActions.Add(
                new LogicAction_PlayPersonCard(actionLogic.getActionCardScore(pc), pc)
            );
        }

        // 4. Guessing one of the target's characteristics for a reward.
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            // Only add ones that haven't been guessed yet.
            if (infoTracker.shouldGuessCPD(cpd.cpdType))
            {
                foreach (string cat in cpd.categories)
                {
                    if(infoTracker.shouldGuessCPDCategory(cpd.cpdType, cat))
                    {
                        rankedLogicActions.Add(
                            new LogicAction_GuessProperty(
                                scoreOf_guessProperty(cpd.cpdType, infoTracker.catsPossible[cpd.cpdType].Count),
                                (cpd.cpdType, cat)
                            )
                        );
                    }
                }
            }
        }


        // 5. Guessing the target outright.
        rankedLogicActions.Add(
            new LogicAction(LogicActionType.Guess_Target, scoreOf_guessTarget())
        );

        // 6. The CPU's single best "Ask Around" request, which is enough of a PITA to calculate / track that we
        //    should only consider this for now.
        AAMatrix.Inquiry inq = infoTracker.askAroundMatrix.getBestInquiry(selfAgent.getAskAroundLimit());
        rankedLogicActions.Add(new LogicAction_AskAround(inq.overallScore, inq.about, inq.askingAgent));

        // TODO insertion sort?
        rankedLogicActions.Sort();

        chooseAction();
    }

    /// <summary>
    /// Choose an action to perform once you've ranked all of them.
    /// </summary>
    private void chooseAction()
    {
        // Debugging, if you want it
        if (true)
        {
            string formattedOptions = "";
            for(int i = 0; i < rankedLogicActions.Count; i++)
            {
                LogicAction la = rankedLogicActions[i];
                formattedOptions += la.ToString() + "\n";
            }
            Debug_CPULogicPrintout.instance.updatePrintout(selfAgent.id - 1, formattedOptions);
        }
        
        // TODO: Eventually we have a more sophisticated way, but for now, just choose the best action every time
        LogicAction chosenAction = rankedLogicActions[0];
        Debug.Log("Chosen " + chosenAction.ToString());

        executeAction(chosenAction);
    }

    /// <summary>
    /// Execute the chosen action. The result varies widely depending on what type of action it is.
    /// </summary>
    private void executeAction(LogicAction action)
    {
        switch (action.actionType)
        {
            case LogicActionType.Declassify:
                selfAgent.playCard((action as LogicAction_Declassify).clueCard);
                break;

            case LogicActionType.Guess_Property:
                LogicAction_GuessProperty gp = action as LogicAction_GuessProperty;
                selfAgent.guessTargetCharacteristic(gp.property.cpdType, gp.property.category);
                break;

            case LogicActionType.Guess_Target:
                selfAgent.guessTarget(getRandomTargetID());
                break;

            case LogicActionType.Ask_Around:
                LogicAction_AskAround aa = action as LogicAction_AskAround;
                selfAgent.askAgent(aa.askAgent, aa.property);
                break;

            case LogicActionType.PersonCard:
                selfAgent.playCard((action as LogicAction_PlayPersonCard).personCard);
                break;

            default:
                selfAgent.skipTurn();
                break;
        }
    }


    /// <summary>
    /// The CPU was asked questions by another agent.
    /// They can show all guessed cards in their hand to the player who asked,
    /// Or declassify a single guessed clue card to everybody.
    /// Which action they take (and which cards are involved) is determined here.
    /// </summary>
    /// <returns>The topic of the card to declassify, or </returns>
    public (bool declassified, CPD_Type cpdType, string cat) onAskedAbout(Agent whoAsked, IEnumerable<(CPD_Type, string)> overlap)
    {
        // First assess the value of each card - how valuable are they to the player, and to all players?
        int count = 0;

        (CPD_Type, string) bestDeclass = ((CPD_Type)0, "");
        float bestDeclassScore = -9999999; // The highest score for any potential declassification

        foreach((CPD_Type cpdType, string cat) cc in overlap)
        {
            float declassScore = infoTracker.askAroundMatrix.getDeclassifyScore(cc);
            if(declassScore > bestDeclassScore)
            {
                bestDeclass = cc;
                bestDeclassScore = declassScore;
            }
            count++;
        }

        // No reason to declassify unless the CPU would have to show 2 or more cards.
        // TODO: or the CPU is spiteful.
        if (count < 2)
        {
            return (false, (CPD_Type)0, "");
        }
        else
        {
            float showScore = infoTracker.askAroundMatrix.getTotalScoreOfShow(whoAsked, overlap);
            if(showScore > bestDeclassScore)
            {
                return (false, (CPD_Type)0, "");
            } else
            {
                return (true, bestDeclass.Item1, bestDeclass.Item2);
            }
        }
    }



    // ------------------------------
    // Calculations...
    // ------------------------------

    private float scoreOf_guessProperty(CPD_Type cpdType, int numPossibilities)
    {
        return numPossibilities > 2 ? 0 : (3 - numPossibilities) * 2;
    }

    private float scoreOf_guessTarget()
    {
        if (infoTracker.confidence < 0.95f) return 0;
        else
        {
            return (float)Math.Pow(Math.Abs(0.95f - infoTracker.confidence) / 0.05f + 1, 10f);
        }
    }

    private int getRandomTargetID()
    {
        return Roster.SimulatedID.getRandomSimulatedID(selfAgent.rosterConstraints, null,
            TurnDriver.instance.currentRoster.getNewRosterSizeFromConstraints(selfAgent.rosterConstraints));
    }



    




    // ------------------------------
    // Logic Actions...
    // ------------------------------


    // An Action the CPU can perform for their turn.
    class LogicAction : IComparable
    {
        public LogicActionType actionType;
        public float score;

        public LogicAction(LogicActionType lat, float sc)
        {
            actionType = lat;
            score = sc;
        }

        // Sorted top to bottom.
        public int CompareTo(object obj)
        {
            return -1 * score.CompareTo((obj as LogicAction).score);
        }

        public override string ToString()
        {
            return actionType.ToString() + ": " + score;
        }
    }

    class LogicAction_Declassify : LogicAction
    {
        public ClueCard clueCard;

        public LogicAction_Declassify(float sc, ClueCard cc) : base(LogicActionType.Declassify, sc)
        {
            clueCard = cc;
        }

        public override string ToString()
        {
            return "Declass (" + clueCard.cpdType + "::" + clueCard.category + "): " + score;
        }
    }

    class LogicAction_PlayPersonCard : LogicAction
    {
        public PersonCard personCard;

        public LogicAction_PlayPersonCard(float sc, PersonCard pc) : base(LogicActionType.PersonCard, sc)
        {
            personCard = pc;
        }

        public override string ToString()
        {
            if (personCard.cardType == CardType.ACTION)
                return (personCard as ActionCard).actionCardType.ToString() + ": " + score;
            else
                return (personCard as GoldCard).goldCardType.ToString() + ": " + score;
        }
    }

    class LogicAction_GuessProperty : LogicAction
    {
        public (CPD_Type cpdType, string category) property;

        public LogicAction_GuessProperty(float sc, (CPD_Type cpdType, string category) props) : base(LogicActionType.Guess_Property, sc)
        {
            property = props;
        }

        public override string ToString()
        {
            return "Guess property: " + property.ToString() + ": " + score;
        }
    }

    class LogicAction_AskAround : LogicAction
    {
        public List<(CPD_Type cpdType, string category)> property;
        public Agent askAgent;

        public LogicAction_AskAround(float sc, List<(CPD_Type cpdType, string category)> props, Agent askToAgent) : base(LogicActionType.Ask_Around, sc)
        {
            property = props;
            askAgent = askToAgent;
        }

        public override string ToString()
        {
            return "AA for " + property.Count + " props: " + score;
        }
    }

    enum LogicActionType
    {
        Nothing,
        Declassify,
        PersonCard,
        Guess_Property,
        Guess_Target,
        Ask_Around
    }
}




// ------------------------------
// CPU Personality Traits...
// ------------------------------

public class CPUPersonalityStats
{
    public Dictionary<CPUPersonalityTrait, float> personalityTraits;

    public CPUPersonalityStats(bool randomize)
    {
        personalityTraits = new Dictionary<CPUPersonalityTrait, float>();

        // If no base stats supplied, randomize them all
        if (randomize)
        {
            personalityTraits.Add(CPUPersonalityTrait.Intelligence, Range(0, 1));
            personalityTraits.Add(CPUPersonalityTrait.Deceptive, Range(0, 1));
            personalityTraits.Add(CPUPersonalityTrait.Reckless, Range(0, 1));
            personalityTraits.Add(CPUPersonalityTrait.Aggressive, Range(0, 1));
            personalityTraits.Add(CPUPersonalityTrait.Secretive, Range(0, 1));
            personalityTraits.Add(CPUPersonalityTrait.Grudgy, Range(0, 1));
        }
        else
        {
            personalityTraits.Add(CPUPersonalityTrait.Intelligence, 0);
            personalityTraits.Add(CPUPersonalityTrait.Deceptive, 0);
            personalityTraits.Add(CPUPersonalityTrait.Reckless, 0);
            personalityTraits.Add(CPUPersonalityTrait.Aggressive, 0);
            personalityTraits.Add(CPUPersonalityTrait.Secretive, 0);
            personalityTraits.Add(CPUPersonalityTrait.Grudgy, 0);
        }
    }

    public bool atLeast(CPUPersonalityTrait trait, float threshold)
    {
        return personalityTraits[trait] >= threshold;
    }

    public bool below(CPUPersonalityTrait trait, float threshold)
    {
        return personalityTraits[trait] <= threshold;
    }

    public bool atLeastMany(List<(CPUPersonalityTrait trait, float threshold)> couples)
    {
        foreach((CPUPersonalityTrait trait, float threshold) couple in couples)
        {
            if (personalityTraits[couple.trait] < couple.threshold) return false;
        }
        return true;
    }

    public bool belowMany(List<(CPUPersonalityTrait trait, float threshold)> couples)
    {
        foreach ((CPUPersonalityTrait trait, float threshold) couple in couples)
        {
            if (personalityTraits[couple.trait] < couple.threshold) return false;
        }
        return true;
    }
}


// All traits from 0 - 1
public enum CPUPersonalityTrait
{
    Intelligence,    // Chooses actions randomly -- everything is calculated
    Deceptive,       // Straightforward -- Will try to deceive other players
    Reckless,        // Cautious -- Willing to take risks
    Aggressive,      // Minds own business -- Likes bringing down others
    Secretive,       // Doesn't care about letting info slip -- prioritizes secrecy
    Grudgy           // Will attack the leader -- will attack players who previously wronged them
}
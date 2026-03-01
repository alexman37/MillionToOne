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
    CPUPersonalityStats personalityStats;

    // CPU's current ordering of how good (or bad) it thinks each possible action would be.
    private List<LogicAction> rankedLogicActions;

    // CPU's assessed options for asking around other CPU's
    private AAMatrix askAroundMatrix;

    public CPUAgentLogic(CPUAgent agent)
    {
        selfAgent = agent;
        infoTracker = agent.infoTracker;

        // TODO may want to load this from somewhere else, eventually
        personalityStats = new CPUPersonalityStats(false);

        rankedLogicActions = new List<LogicAction>();
        askAroundMatrix = new AAMatrix(selfAgent.id);
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

        foreach(Card c in selfAgent.inventory)
        {
            // 2. Declassifying a clue card to get an action card.
            if(c is ClueCard)
            {
                ClueCard cc = c as ClueCard;
                rankedLogicActions.Add(
                    new LogicAction_Declassify(2, cc)
                );
            }

            // 3. Using an action card.
            else if (c is ActionCard)
            {
                // TODO
            }
        }

        // 4. Guessing one of the target's characteristics for a reward.
        foreach(CPD cpd in Roster.cpdConstrainables)
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
        AAMatrix.Inquiry inq = askAroundMatrix.getBestInquiry(1);
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
                //selfAgent.askAgent()
                break;

            default:
                selfAgent.skipTurn();
                break;
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
    }

    class LogicAction_GuessProperty : LogicAction
    {
        public (CPD_Type cpdType, string category) property;

        public LogicAction_GuessProperty(float sc, (CPD_Type cpdType, string category) props) : base(LogicActionType.Guess_Property, sc)
        {
            property = props;
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
    }

    enum LogicActionType
    {
        Nothing,
        Declassify,
        Guess_Property,
        Guess_Target,
        Ask_Around
    }




    // ------------------------------
    // CPU Personality Traits...
    // ------------------------------

    class CPUPersonalityStats
    {
        public Dictionary<CPUPersonalityTrait, float> personalityTraits;

        public CPUPersonalityStats(bool randomize)
        {
            personalityTraits = new Dictionary<CPUPersonalityTrait, float>();

            // If no base stats supplied, randomize them all
            if(randomize)
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
    }


    // All traits from 0 - 1
    enum CPUPersonalityTrait
    {
        Intelligence,    // Chooses actions randomly -- everything is calculated
        Deceptive,       // Straightforward -- Will try to deceive other players
        Reckless,        // Cautious -- Willing to take risks
        Aggressive,      // Minds own business -- Likes bringing down others
        Secretive,       // Doesn't care about letting info slip -- prioritizes secrecy
        Grudgy           // Will attack the leader -- will attack players who previously wronged them
    }
}
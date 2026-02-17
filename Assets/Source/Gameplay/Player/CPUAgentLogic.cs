using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    // CPU's current ordering of how good (or bad) it thinks each possible action would be.
    private List<LogicAction> rankedLogicActions;

    public CPUAgentLogic(CPUAgent agent)
    {
        selfAgent = agent;
        infoTracker = agent.infoTracker;

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
            rankedLogicActions.Add(
                new LogicAction(
                    LogicActionType.Guess_Target,
                    scoreOf_guessProperty(cpd.cpdType, infoTracker.catsPossible[cpd.cpdType])
                )
            );
            
        }


        // 5. Guessing the target outright.
        rankedLogicActions.Add(
            new LogicAction(LogicActionType.Guess_Target, scoreOf_guessTarget())
        );

        // TODO insertion sort?
        rankedLogicActions.Sort();

        chooseAction();
    }

    /// <summary>
    /// Choose an action to perform once you've ranked all of them.
    /// </summary>
    private void chooseAction()
    {
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
                //selfAgent.guessTargetCharacteristic();
                break;

            case LogicActionType.Guess_Target:
                //selfAgent.guessTarget();
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
        // TODO
        return infoTracker.confidence;
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

    enum LogicActionType
    {
        Nothing,
        Declassify,
        Guess_Property,
        Guess_Target,
        Ask_Around
    }
}
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

    // CPU's assessed options for asking around other CPU's
    private AAMatrix askAroundMatrix;

    public CPUAgentLogic(CPUAgent agent)
    {
        selfAgent = agent;
        infoTracker = agent.infoTracker;

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
        Debug.Log(inq);

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
                LogicAction_GuessProperty gp = action as LogicAction_GuessProperty;
                selfAgent.guessTargetCharacteristic(gp.property.cpdType, gp.property.category);
                break;

            case LogicActionType.Guess_Target:
                selfAgent.guessTarget(getRandomTargetID(), false);
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
    // The "Ask Around" Matrix...
    // ------------------------------
    class AAMatrix
    {
        int forId;

        List<Agent> agentsInOrder;
        int numPlayers;

        // How enticing it is to ask each agent about this key
        Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>> agentsPerKey;
        // All keys, sorted in order from most to least enticing
        List<KeyScore> sortedKeys;

        public AAMatrix(int id)
        {
            forId = id;
            agentsPerKey = new Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>>();
            sortedKeys = new List<KeyScore>();

            agentsInOrder = TurnDriver.instance.agentsInOrder;
            numPlayers = agentsInOrder.Count;


            List<AgentScore> defaultAgentScores = new List<AgentScore>();
            for(int i = 0; i < numPlayers; i++)
            {
                // The CPU should obviously never ask itself anything.
                defaultAgentScores.Add(new AgentScore(agentsInOrder[i], id != i ? 1 : -99999));
            }

            foreach (CPD cpd in Roster.cpdConstrainables)
            {
                foreach(string category in cpd.categories)
                {
                    agentsPerKey.Add((cpd.cpdType, category), new List<AgentScore>(defaultAgentScores));
                    sortedKeys.Add(new KeyScore((cpd.cpdType, category), 1));
                }
            }
        }



        // Return what the CPU's single best "Ask Around" request is at the moment.
        public Inquiry getBestInquiry(int howManyAsks)
        {
            // Assume agents properly indexed
            Dictionary<int, float> agentScores = new Dictionary<int, float>();
            List<(CPD_Type, string)> bestKeys = new List<(CPD_Type, string)>();

            // Initialize lists
            for(int n = 0; n < numPlayers; n++)
            {
                agentScores.Add(n, 0);
            }

            // For each important key, find how good each player is to ask
            for (int k = 0; k < howManyAsks; k++)
            {
                (CPD_Type, string) key = sortedKeys[k].key;
                bestKeys.Add(key);

                for(int a = 0; a < numPlayers; a++)
                {
                    agentScores[a] += agentsPerKey[key][a].score;
                }
            }

            // Ask the overall best fit
            float maxValue = -99999;
            int bestAgent = -1;

            for (int n = 0; n < numPlayers; n++)
            {
                if(agentScores[n] > maxValue)
                {
                    maxValue = agentScores[n];
                    bestAgent = n;
                }
            }

            // Get keys only
            return new Inquiry(agentsInOrder[bestAgent], bestKeys);
        }




        public class Inquiry
        {
            public Agent askingAgent;
            public List<(CPD_Type, string)> about;

            public Inquiry(Agent ask, List<(CPD_Type, string)> ab)
            {
                askingAgent = ask;
                about = ab;
            }

            public override string ToString()
            {
                return "Inquiry for " + askingAgent + " about " + about.Count + " properties";
            }
        }

        class AgentScore : IComparable
        {
            public Agent agent;
            public float score;

            public AgentScore(Agent a, float s)
            {
                agent = a;
                score = s;
            }

            // Sorted top to bottom.
            public int CompareTo(object obj)
            {
                return -1 * score.CompareTo((obj as AgentScore).score);
            }
        }

        class KeyScore : IComparable
        {
            public (CPD_Type cpdType, string cat) key;
            public float score;

            public KeyScore((CPD_Type cpdType, string cat) k, float s)
            {
                key = k;
                score = s;
            }

            // Sorted top to bottom.
            public int CompareTo(object obj)
            {
                return -1 * score.CompareTo((obj as KeyScore).score);
            }
        }
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

    class LogicAction_AskAround : LogicAction
    {
        public (CPD_Type cpdType, string category) property;

        public LogicAction_AskAround(float sc, (CPD_Type cpdType, string category) props) : base(LogicActionType.Ask_Around, sc)
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
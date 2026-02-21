using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ------------------------------
// The "Ask Around" Matrix...
// Dictates how CPUs determine who to ask for clues, and what to ask about.
// ------------------------------
public class AAMatrix
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
        for (int i = 0; i < numPlayers; i++)
        {
            // The CPU should obviously never ask itself anything.
            defaultAgentScores.Add(new AgentScore(agentsInOrder[i], id != i ? 1 : -99999));
        }

        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            foreach (string category in cpd.categories)
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
        for (int n = 0; n < numPlayers; n++)
        {
            agentScores.Add(n, 0);
        }

        // For each important key, find how good each player is to ask
        for (int k = 0; k < howManyAsks; k++)
        {
            (CPD_Type, string) key = sortedKeys[k].key;
            bestKeys.Add(key);

            for (int a = 0; a < numPlayers; a++)
            {
                agentScores[a] += agentsPerKey[key][a].score;
            }
        }

        // Ask the overall best fit
        float maxValue = -99999;
        int bestAgent = -1;

        for (int n = 0; n < numPlayers; n++)
        {
            if (agentScores[n] > maxValue)
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
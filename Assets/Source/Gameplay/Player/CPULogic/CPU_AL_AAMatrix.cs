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

    // Don't set these in constructor - they're probably not ready yet
    List<Agent> agentsInOrder;
    int numPlayers;

    // How enticing it is to ask each agent about this key
    Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>> agentsPerKey;
    // All keys, sorted in order from most to least enticing
    KeyScoreChart keyScoreChart;

    public AAMatrix(int id)
    {
        forId = id;
        agentsPerKey = new Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>>();

        keyScoreChart = new KeyScoreChart();
    }

    private void getGameData()
    {
        agentsInOrder = TurnDriver.instance.agentsInOrder;
        numPlayers = agentsInOrder.Count;

        int count = 0;
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            foreach (string category in cpd.categories)
            {
                agentsPerKey.Add((cpd.cpdType, category), new List<AgentScore>());
                count++;

                for (int i = 0; i < numPlayers; i++)
                {
                    // The CPU should obviously never ask itself anything.
                    agentsPerKey[(cpd.cpdType, category)].Add(new AgentScore(i, forId != i ? 1 : -99999));
                }

                keyScoreChart.keyscoreLookup.Add((cpd.cpdType, category), new KeyScore((cpd.cpdType, category), 1));
            }
        }
    }

    // Return what the CPU's single best "Ask Around" request is at the moment.
    public Inquiry getBestInquiry(int howManyAsks)
    {
        if (agentsInOrder == null) getGameData();

        // Assume agents properly indexed
        Dictionary<int, float> agentScores = new Dictionary<int, float>();
        List<(CPD_Type, string)> bestKeys = new List<(CPD_Type, string)>();

        // Initialize lists
        for (int n = 0; n < numPlayers; n++)
        {
            agentScores.Add(n, 0);
        }

        // For each important key, find how good each player is to ask
        List<KeyScore> topHits = keyScoreChart.GetTopN(howManyAsks);
        for (int k = 0; k < howManyAsks; k++)
        {
            (CPD_Type, string) key = topHits[k].key;
            bestKeys.Add(key);

            for (int a = 0; a < numPlayers; a++)
            {
                agentScores[a] = agentScores[a] + agentsPerKey[key][a].score;
            }
        }

        // Ask the overall best agent to ask about this
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

        float overallScore = maxValue; // TODO
        return new Inquiry(overallScore, agentsInOrder[bestAgent], bestKeys);
    }




    public class Inquiry
    {
        public float overallScore;
        public Agent askingAgent;
        public List<(CPD_Type, string)> about;

        public Inquiry(float sc, Agent ask, List<(CPD_Type, string)> ab)
        {
            overallScore = sc;
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
        public int agentId;
        public float score;

        public AgentScore(int aid, float s)
        {
            agentId = aid;
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

    class KeyScoreChart
    {
        // For direct and easy access
        public Dictionary<(CPD_Type, string), KeyScore> keyscoreLookup;

        public KeyScoreChart()
        {
            keyscoreLookup = new Dictionary<(CPD_Type, string), KeyScore>();
        }

        public void UpdateChart((CPD_Type, string) key, float newVal)
        {
            keyscoreLookup[key].score = newVal;
        }

        public void UpdateChartKeyBy((CPD_Type, string) key, float byAmount)
        {
            keyscoreLookup[key].score += byAmount;
        }

        // Get N highest-scoring keys in the chart
        public List<KeyScore> GetTopN(int numHits)
        {
            List<KeyScore> highestScorers = new List<KeyScore>();
            for(int i = 0; i < numHits; i++)
            {
                highestScorers.Add(new KeyScore(((CPD_Type)0, ""), -1));
            }

            // Find highest scorers
            foreach(KeyScore ks in keyscoreLookup.Values)
            {
                if (ks.CompareTo(highestScorers[numHits - 1]) < 0)
                {
                    // TODO optimize with binary insert, probably
                    highestScorers.Add(ks);
                    highestScorers.Sort();
                    highestScorers.RemoveAt(numHits);
                }
            }

            // Return all highest scorers greater than 0
            int lastGoodIndex = -1;
            for(int i = numHits - 1; i >= 0; i--) { 
                if(highestScorers[i].score > 0)
                {
                    lastGoodIndex = i + 1;
                    break;
                }
            }

            return lastGoodIndex > 0 ? highestScorers.GetRange(0, lastGoodIndex) : new List<KeyScore>();
        }
    }
}
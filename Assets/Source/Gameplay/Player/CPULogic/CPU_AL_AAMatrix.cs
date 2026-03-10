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
    protected Agent selfAgent;
    protected CPUInfoTracker infoTracker;
    protected CPUPersonalityStats personalityStats;

    // Don't set these in constructor - they're probably not ready yet
    List<Agent> agentsInOrder;
    int numPlayers;

    // Both keyscores and agentsPerKey can use this.
    protected static Dictionary<CPD_Type, List<string>> catsPerCPD;
    private static bool catsPerCPDinit = false;

    // How enticing it is to ask each agent about this key
    Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>> agentsPerKey;
    // All keys, sorted in order from most to least enticing
    KeyScoreChart keyScoreChart;
    // How often each key is asked about by each player.
    static Dictionary<(CPD_Type cpdType, string cat), List<int>> askAroundCount;

    public AAMatrix(Agent agent, CPUInfoTracker info, CPUPersonalityStats personality)
    {
        selfAgent = agent;
        infoTracker = info;
        personalityStats = personality;

        agentsPerKey = new Dictionary<(CPD_Type cpdType, string cat), List<AgentScore>>();

        keyScoreChart = new KeyScoreChart(this);
    }

    public void getGameData()
    {
        agentsInOrder = TurnDriver.instance.agentsInOrder;
        numPlayers = agentsInOrder.Count;

        if (!catsPerCPDinit)
        {
            catsPerCPD = new Dictionary<CPD_Type, List<string>>();
            // Assume the other static data structure also uninstantiated.
            askAroundCount = new Dictionary<(CPD_Type cpdType, string cat), List<int>>();
        }

        int count = 0;
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            if (!catsPerCPDinit)
            {
                catsPerCPD.Add(cpd.cpdType, new List<string>(cpd.categories));
            }

            foreach (string category in cpd.categories)
            {
                agentsPerKey.Add((cpd.cpdType, category), new List<AgentScore>());
                count++;

                if (!catsPerCPDinit)
                {
                    askAroundCount.Add((cpd.cpdType, category), new List<int>());
                }

                for (int i = 0; i < numPlayers; i++)
                {
                    // The CPU should obviously never ask itself anything.
                    if(i != selfAgent.id)
                        agentsPerKey[(cpd.cpdType, category)].Add(new AgentScore(i, 1));
                }

                keyScoreChart.keyscoreLookup.Add((cpd.cpdType, category), new KeyScore((cpd.cpdType, category), getBaseKeyScore(cpd, category)));
            }
        }
        catsPerCPDinit = true;
    }

    // Get the base score for a CPD at the start
    // Currently depends on: CPD Reward
    private float getBaseKeyScore(CPD cpd, string cat)
    {
        float value = 1;

        // Rewards!
        if(personalityStats.atLeast(CPUPersonalityTrait.Aggressive, 0f))
        {
            TargetCPDGuessReward reward = cpd.getGuessReward();
            if(reward == TargetCPDGuessReward.ActionCard)
            {
                value += 3;
            }
            else if (reward == TargetCPDGuessReward.GoldCard)
            {
                value += 5;
            }
            else // No reward
            {
                value -= 2;
            }
        }

        return value;
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

        float allKeyScores = 0;

        // For each important key, find how good each player is to ask
        List<KeyScore> topHits = keyScoreChart.GetTopN(howManyAsks);
        for (int k = 0; k < howManyAsks; k++)
        {
            (CPD_Type, string) key = topHits[k].key;
            bestKeys.Add(key);

            allKeyScores += keyScoreChart.GetScore(key);

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

        float overallScore = maxValue + allKeyScores;
        return new Inquiry(overallScore, agentsInOrder[bestAgent], bestKeys);
    }

    // Action when the CPU definitively learns a piece of information
    public void learnedAboutCPD(CPD_Type cpdType, string cat, bool onTarget)
    {
        keyScoreChart.OnLearnedAboutCPD(cpdType, cat, onTarget);
    }

    // Action when a CPD is revealed to all players
    public void cpdRevealed(CPD_Type cpdType)
    {
        keyScoreChart.OnCPDRevealed(cpdType);
    }

    // Actions to take when this CPU is very close to, or ready to, guess the target
    public void closeToGuessingTarget()
    {
        keyScoreChart.UpdateUrgency();
    }

    /// <summary>
    /// The moment any ask around request is made this DS should be updated
    /// </summary>
    public static void MarkInAskAroundCount(int idOfWhoAsked, IEnumerable<(CPD_Type, string)> topics)
    {
        foreach ((CPD_Type, string) topic in topics)
        {
            askAroundCount[topic].Add(idOfWhoAsked);
        }
    }

    // TODO: one day we could add some logic for when a card was declassified instead of all being revealed.
    public void processOutsideAA(Agent asker, Agent asked, List<(CPD_Type, string)> topics, int numShown)
    {
        foreach((CPD_Type, string) topic in topics)
        {
            // If you asked about this topic, it's less likely you have it.
            AgentScore askerAS = agentsPerKey[topic].Find(a => a.agentId == asker.id);
            if(askerAS != null)
            {
                askerAS.score -= 2;
            }
            

            // Can learn things about the agent being asked based on their response.
            AgentScore askedAS = agentsPerKey[topic].Find(a => a.agentId == asked.id);
            if(askedAS != null)
            {
                // "I had none of those cards" = this agent definitely doesn't have them
                if (numShown == 0)
                {
                    agentsPerKey[topic].Remove(askedAS);
                    // Want to know a little more now that another person def. didn't have it
                    keyScoreChart.UpdateChartKeyBy(topic, 3);
                }
                else
                {
                    askedAS.score += askerScoreBoostFromNumCorrectReported(topic.Item1, topic.Item2, topics.Count, numShown);
                }
            }
        }
    }

    // And also update the keyscore.
    private float askerScoreBoostFromNumCorrectReported(CPD_Type cpdType, string cat, int numAsked, int numReported)
    {
        // "I had every card you asked for"
        if(numAsked == numReported)
        {
            // We know these all aren't on the target now.
            keyScoreChart.UpdateChart((cpdType, cat), -999);
            learnedAboutCPD(cpdType, cat, false);

            return 1000;
        }
        // "I had some cards but not all of them"
        else
        {
            keyScoreChart.UpdateChartKeyBy((cpdType, cat), 2 * numReported * (numReported / numAsked));

            // 1 out of 2:  +2
            // 2 out of 4:  +4
            // 2 out of 5:  +3.2
            return 4 * numReported * (numReported / numAsked);
        }
    }


    // ------------
    // Functions for being asked about topics
    // ------------

    /// <summary>
    /// How damaging would it be to declassify this card? (The higher, the better for this CPU)
    /// </summary>
    public float getDeclassifyScore((CPD_Type, string) topic)
    {
        // TODO: we could improve on this
        // As a rough estimate - the more something has been asked about, the more likely it is to be known in general
        return askAroundCount[topic].Count;
    }

    /// <summary>
    /// How damaging would it be to show all given cards to specified agent? (The higher, the better for this CPU)
    /// </summary>
    public float getTotalScoreOfShow(Agent whoAsked, IEnumerable<(CPD_Type, string)> topics)
    {
        // TODO: we could improve on this
        // As a rough estimate - the more an agent asks about these topics, the more likely they already know something about it
        int totalAsks = 0;
        foreach((CPD_Type, string) topic in topics)
        {
            totalAsks += askAroundCount[topic].FindAll(i => i == whoAsked.id).Count;
        }
        return totalAsks;
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

        public override string ToString()
        {
            return key.cpdType.ToString() + "::" + key.cat + " = " + score;
        }
    }

    class KeyScoreChart
    {
        // For direct and easy access
        private AAMatrix reference;
        public Dictionary<(CPD_Type, string), KeyScore> keyscoreLookup;

        public KeyScoreChart(AAMatrix refer)
        {
            reference = refer;
            keyscoreLookup = new Dictionary<(CPD_Type, string), KeyScore>();
        }

        public float GetScore((CPD_Type cpdType, string cat) pair)
        {
            return keyscoreLookup[pair].score;
        }

        public void UpdateChart((CPD_Type, string) key, float newVal)
        {
            if(keyscoreLookup.ContainsKey(key))
                keyscoreLookup[key].score = newVal;
        }

        public void UpdateChartKeyBy((CPD_Type, string) key, float byAmount)
        {
            if (keyscoreLookup.ContainsKey(key))
                keyscoreLookup[key].score += byAmount;
        }

        public void RemoveFromChart((CPD_Type, string) key)
        {
            keyscoreLookup.Remove(key);
        }

        // When learning information:
        //    - Not on target: Make this property significantly less likely to be asked about, update all others positively.
        //    - On target: Update entire CPD to be significantly less likely to be asked about.
        public void OnLearnedAboutCPD(CPD_Type cpdType, string learnedCat, bool onTarget)
        {
            if(onTarget)
            {
                foreach(string cat in catsPerCPD[cpdType])
                {
                    if(cat == learnedCat)
                        UpdateChartKeyBy((cpdType, cat), -100);
                    else
                        UpdateChartKeyBy((cpdType, cat), -50);
                }
            } else
            {
                foreach (string cat in catsPerCPD[cpdType])
                {
                    if (cat == learnedCat)
                        UpdateChartKeyBy((cpdType, cat), -50);
                    else
                    {
                        float numTotal = catsPerCPD[cpdType].Count;
                        float numRemaining = reference.infoTracker.catsPossible[cpdType].Count;
                        UpdateChartKeyBy((cpdType, cat), 5 * (1 + (1f - numRemaining) / numTotal));
                    }
                }
            }
        }

        // When a CPD is revealed we want to remove it from the list of things
        public void OnCPDRevealed(CPD_Type cpdType)
        {
            foreach (string cat in catsPerCPD[cpdType])
            {
                RemoveFromChart((cpdType, cat));
            }
        }

        public void UpdateUrgency()
        {

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

            //DebugKeyscores();
            return lastGoodIndex > 0 ? highestScorers.GetRange(0, lastGoodIndex) : new List<KeyScore>();
        }

        public void DebugKeyscores()
        {
            string formatted = "";

            foreach((CPD_Type, string) key in keyscoreLookup.Keys)
            {
                formatted = formatted + keyscoreLookup[key].ToString() + "\n";
            }

            Debug_CPULogicPrintout.instance.updateAAprintout(reference.selfAgent.id - 1, formatted);
        }
    }
}
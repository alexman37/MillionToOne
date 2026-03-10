using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CPUInfoTracker
{
    private CPUAgent agentSelf;

    // What percentage of suspects you've eliminated so far: 0-1
    public float confidence;
    public bool inCrunchTime = false;
    private static float totalRosterSizeAtStart = -1;

    // What categories are currently in your hand (clue cards) - use to answer Ask around's
    public HashSet<(CPD_Type, string)> catsInHand = new HashSet<(CPD_Type, string)>();

    // How many categories are still possible for each CPD
    public Dictionary<CPD_Type, HashSet<string>> catsPossible = new Dictionary<CPD_Type, HashSet<string>>();

    // We know for certain the CPDs in this list, and can ignore guessing/using them in some ways
    public HashSet<CPD_Type> solvedCPDs = new HashSet<CPD_Type>();

    // CPU's assessed options for asking around other CPU's
    public AAMatrix askAroundMatrix;


    public CPUInfoTracker(CPUAgent self)
    {
        agentSelf = self;

        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            catsPossible.Add(cpd.cpdType, new HashSet<string>(cpd.categories));
        }

        CPUAgent.cpuUpdateProgress += updateConfidence;

        if (totalRosterSizeAtStart == -1) totalRosterSizeAtStart = (float)TurnDriver.instance.currentRoster.simulatedTotalRosterSize;

        askAroundMatrix = new AAMatrix(agentSelf, this, self.personalityStats);
    }

    ~CPUInfoTracker()
    {
        CPUAgent.cpuUpdateProgress -= updateConfidence;
    }


    /// <summary>
    /// Card added to hand
    /// </summary>
    public void AddedCardToHand(Card c)
    {
        if(c.cardType == CardType.CLUE)
        {
            ClueCard cc = c as ClueCard;
            catsInHand.Add((cc.cpdType, cc.category));
            MarkDefinitive(cc.cpdType, cc.category, cc.onTarget);

            askAroundMatrix.learnedAboutCPD(cc.cpdType, cc.category, cc.onTarget);
        } 
        
        else
        {
            // TODO
        }
    }

    /// <summary>
    /// Card removed from hand
    /// </summary>
    public void RemovedCardFromHand(Card c)
    {
        if (c.cardType == CardType.CLUE)
        {
            ClueCard cc = c as ClueCard;
            catsInHand.Remove((cc.cpdType, cc.category));
        }

        else
        {
            // TODO
        }
    }

    /// <summary>
    /// Whether by getting a clue card or seeing one declassified, or any other means,
    /// Mark a characteristic as guaranteed known.
    /// </summary>
    public void MarkDefinitive(CPD_Type cpdType, string category, bool onTarget)
    {
        if(onTarget)
        {
            catsPossible[cpdType] = new HashSet<string>() { category };
            solvedCPDs.Add(cpdType);
        } else
        {
            catsPossible[cpdType].Remove(category);
            if (catsPossible[cpdType].Count == 1)
                solvedCPDs.Add(cpdType);
        }
        askAroundMatrix.learnedAboutCPD(cpdType, category, onTarget);
    }

    /// <summary>
    /// A CPD was revealed to all players. Do not ask about it anymore, no reason to.
    /// </summary>
    public void CPDRevealed(CPD_Type cpdType, string category) {
        solvedCPDs.Add(cpdType);
        askAroundMatrix.cpdRevealed(cpdType);
    }

    /// <summary>
    /// Someone else asked around. You should track this information.
    /// </summary>
    public void ProcessOutsideAA(Agent asker, Agent asked, List<(CPD_Type, string)> topics, int numShown)
    {
        askAroundMatrix.processOutsideAA(asker, asked, topics, numShown);
    }







    private void updateConfidence(int id, int newRosterSize)
    {
        confidence = (float)newRosterSize / totalRosterSizeAtStart;
        inCrunchTime = meetsCrunchTimeThreshold(newRosterSize);
    }

    // "Crunch time": The CPU is close to getting it but not ready to guess yet.
    // Behavior depends on personality
    private bool meetsCrunchTimeThreshold(int rawRosterSize)
    {
        // TODO gotta be more thoughtful
        return rawRosterSize < 100 || confidence > 0.9f;
    }



    /// <summary>
    /// Do you have the specified category as a clue card in your hand right now?
    /// </summary>
    public bool HasCardFor((CPD_Type, string) category)
    {
        return catsInHand.Contains(category);
    }




    // Logic Utility


    public bool shouldGuessCPD(CPD_Type cpdType)
    {
        return solvedCPDs.Contains(cpdType);
    }

    public bool shouldGuessCPDCategory(CPD_Type cpdType, string category)
    {
        return catsPossible[cpdType].Contains(category);
    }
}
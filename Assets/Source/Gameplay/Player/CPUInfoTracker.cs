using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CPUInfoTracker
{
    // What percentage of suspects you've eliminated so far: 0-1
    public float confidence;

    // What categories are currently in your hand (clue cards) - use to answer Ask around's
    private HashSet<(CPD_Type, string)> catsInHand = new HashSet<(CPD_Type, string)>();

    // How many categories are still possible for each CPD
    public Dictionary<CPD_Type, int> catsPossible = new Dictionary<CPD_Type, int>();

    // We know for certain the CPDs in this list, and can ignore guessing/using them in some ways
    private HashSet<CPD_Type> solvedCPDs = new HashSet<CPD_Type>();


    public CPUInfoTracker()
    {
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            catsPossible.Add(cpd.cpdType, cpd.categories.Count);
        }
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
            catsPossible[cpdType] = 1;
            solvedCPDs.Add(cpdType);
        } else
        {
            catsPossible[cpdType] -= 1;
            if(catsPossible[cpdType] == 1)
            {
                solvedCPDs.Add(cpdType);
            }
        }
    }



    /// <summary>
    /// Do you have the specified category as a clue card in your hand right now?
    /// </summary>
    public bool HasCardFor((CPD_Type, string) category)
    {
        return catsInHand.Contains(category);
    }
}
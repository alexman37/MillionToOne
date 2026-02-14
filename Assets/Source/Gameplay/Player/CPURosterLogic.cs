using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CPURosterLogic
{
    // What categories are currently in your hand (clue cards)
    private HashSet<(CPD_Type, string)> catsInHand = new HashSet<(CPD_Type, string)>();


    /// <summary>
    /// Card added to hand: mark information down, assess new options
    /// </summary>
    public void AddedCardToHand(Card c)
    {
        if(c.cardType == CardType.CLUE)
        {
            ClueCard cc = c as ClueCard;
            catsInHand.Add((cc.cpdType, cc.category));
        } 
        
        else
        {
            // TODO
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


/*public class CPURosterLogic
{
    private Dictionary<CPD_Type, CPU_CPDDiscoveryProgress> cpdProgress = new Dictionary<CPD_Type, CPU_CPDDiscoveryProgress>();
    private Dictionary<CPD_Type, CPU_CategoryDiscoveryProgress> categoryProgress = new Dictionary<CPD_Type, CPU_CategoryDiscoveryProgress>();



    // A Data structure where you can look up the scores of any entry and quickly sort by the
    // NOTE: Tried some binary insert / remove stuff here...not worth the annoyance in the end b/c there will only be at
    //       most ~15 categories in a list, and they aren't even changed that often.
    class ScoredDictionary
    {
        private Dictionary<CPD_Type, CPU_DiscoveryProgress> lookupTable = new Dictionary<CPD_Type, CPU_DiscoveryProgress>();
        private List<CPD_Type> sortedKeys = new List<CPD_Type>();

        public void AddAllAtStart(List<CPD_Type> key)
        {
            //key
        }

        // Change the score of something
        public void Modify(CPD_Type key, float newValue)
        {
            //lookupTable[key].SetValue(;
            sortedKeys.Add(key);
            sortedKeys.Sort();
        }

        public void Remove(CPD_Type key)
        {
            lookupTable.Remove(key);
            sortedKeys.Remove(key);
        }
    }

    class CPU_CPDDiscoveryProgress : CPU_DiscoveryProgress
    {
        public CPULogic_CPDStatus status;
        public float confidence;

        public override int CompareTo(object other)
        {
            if (!(other is CPU_CPDDiscoveryProgress)) return 0;
            else return confidence.CompareTo((other as CPU_CPDDiscoveryProgress).confidence);
        }

        public override void SetValue(float newVal)
        {
            confidence = newVal;
        }
    }

    class CPU_CategoryDiscoveryProgress : CPU_DiscoveryProgress
    {
        public CPULogic_CategoryStatus status;
        public float urgency;

        public override int CompareTo(object other)
        {
            if (!(other is CPU_CategoryDiscoveryProgress)) return 0;
            else return urgency.CompareTo((other as CPU_CategoryDiscoveryProgress).urgency);
        }

        public override void SetValue(float newVal)
        {
            urgency = newVal;
        }
    }

    abstract class CPU_DiscoveryProgress : IComparable
    {
        public virtual int CompareTo(object other)
        {
            Debug.LogError("Something went wrong - used base discovery class");
            return 0;
        }

        public abstract void SetValue(float newVal);
    }
}






public enum CPULogic_CategoryStatus
{
    Confirmed,  // It has been proven this is the category of the target
    Suspected,  // We have some data on this category one way or another  (+ how much you think it's it)
    Unknown,    // We know nothing about this category yet
    Eliminated  // It has been proven this is not the target's category
}

public enum CPULogic_CPDStatus
{
    Unknown,  // We don't know the category for this CPD yet (+ urgency to find it)
    Found     // We know the category of this CPD
}*/
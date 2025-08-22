using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Facilitates generation, storage and access of the list of 100 suspects (characters 1-100), and the murder victim (character 0).
public class Roster
{
    public int simulatedTotalRosterSize; // total number of "characters" we're working with
    private int simulatedCurrentRosterSize;
    const int TOTAL_ROSTER_PERMUTATIONS = 999999; // How many different rosters can there be?
    private int rosterSeedOffset;

    public List<Character> roster; // real characters that actually exist because we had to generate them at some point.
    public List<Sprite> rosterSprites; //consistent list of the portrait per each character

    public RosterConstraints rosterConstraints;

    public static List<CPD> cpdInstances;      // All CPD singletons
    public static List<CPD> cpdConstrainables; // Only the constrainable CPDs (packaged in sim ID, in this order)
    protected static List<int> cpdCounts; // optimization for simulated ID unpacking
    protected static List<int> simIDtourGuide; // the first CPD should be multiplied by index 0...the second by index 1...etc. to get sim ID.
    public static Dictionary<CPD_Type, CPD> cpdByType;

    public static event Action rosterReady;
    public static event Action<int> constrainedResult;

    // Most of this is first-time setup only
    public Roster(int numChars)
    {
        if (constrainedResult == null) constrainedResult += (_) => { };
        if (rosterReady == null) rosterReady += () => { };

        // No need to recreate CPDs on each load
        if(cpdInstances == null)
        {
            cpdInstances = new List<CPD>
            {
                new CPD_FilePath(CPD_Type.HairStyle, true, "hairStylesTxt", "CharSprites/Hair/"),
                new CPD_Color(CPD_Type.HairColor, true, "hairTonesTxt"),
                new CPD_Color(CPD_Type.SkinTone, true, "skinTonesTxt"),
                new CPD_FilePath(CPD_Type.BodyType, true, "bodyTypesTxt", "CharSprites/Body/"),
                new CPD_FilePath(CPD_Type.Face, false, "faceTypesTxt", "CharSprites/Face/"),
                new CPD_FilePath(CPD_Type.HeadType, false, "headTypesTxt", "CharSprites/Head/"),
            };
            cpdConstrainables = new List<CPD>();
            cpdCounts = new List<int>();
            simIDtourGuide = new List<int>();
            // Set constrainables list
            for (int c = 0; c < cpdInstances.Count; c++)
            {
                if (cpdInstances[c].constrainable)
                {
                    cpdConstrainables.Add(cpdInstances[c]);
                }
            }
            // Set helpers for constrainables list
            for(int c = 0; c < cpdConstrainables.Count; c++)
            {
                int nextOffset = 1;
                cpdCounts.Add(cpdConstrainables[c].categories.Count);
                for (int x = c + 1; x < cpdConstrainables.Count; x++)
                {
                    nextOffset *= cpdConstrainables[x].categories.Count;
                }
                simIDtourGuide.Add(nextOffset);
            }
        }

        if(cpdByType == null)
        {
            cpdByType = new Dictionary<CPD_Type, CPD>();
            foreach(CPD cpd in cpdInstances)
            {
                cpdByType.Add(cpd.cpdType, cpd);
            }
        }

        simulatedTotalRosterSize = numChars;
        simulatedCurrentRosterSize = numChars;

        createRoster();
    }

    // called each time you start a new game
    public void createRoster()
    {
        rosterSeedOffset = UnityEngine.Random.Range(0, TOTAL_ROSTER_PERMUTATIONS);
        if (roster != null)
        {
            roster.Clear();
            rosterSprites.Clear();
        } else
        {
            roster = new List<Character>();
            rosterSprites = new List<Sprite>();
        }

        // "Clear" also serves as initialization for the constraints lists if need be
        rosterConstraints = new RosterConstraints();
        foreach (CPD cpd in cpdConstrainables)
        {
            rosterConstraints.clearConstraints(cpd);
        }
        
        applyConstraints(rosterConstraints);

        // TODO REMOVE
        for (int i = 0; i <= UI_Roster.CHARACTERS_TO_SHOW; i++)
        {
            int simId = SimulatedID.getRandomSimulatedID(rosterConstraints);

            roster.Add(new Character(i, simId));

            //Debug.Log("roster gen " + roster[i]);
            rosterSprites.Add(CharSpriteGen.genSpriteFromLayers(roster[i]));
        }

        rosterReady.Invoke();
    }



    public void redrawRosterVis()
    {
        // TODO do this in a separate step.
        applyConstraints(rosterConstraints);

        // Characters to show
        for (int i = 0; i <= UI_Roster.CHARACTERS_TO_SHOW; i++)
        {
            int simId = SimulatedID.getRandomSimulatedID(rosterConstraints);

            roster.Add(new Character(i, simId));

            roster[i].randomizeDemographics();

            rosterSprites[i] = CharSpriteGen.genSpriteFromLayers(roster[i]);
        }

        UI_Roster.instance.regenerateCharCards(simulatedCurrentRosterSize);
    }

    public void applyConstraints(RosterConstraints constraints)
    {
        // The roster size will decrease when applying a new constraint (and vice versa)
        int newRosterSize = simulatedTotalRosterSize;

        List<CPD_Type> types = new List<CPD_Type>(constraints.allCurrentConstraints.Keys);
        
        foreach (CPD_Type tp in types)
        {
            // Assuming all probabilities are equal.
            /*List<CPD_Variant> variants = cpdByType[tp].getConstrainedCategoryVariants(constraints.allCurrentConstraints[tp]);

            float accumulatedProbability = 0;

            foreach (CPD_Variant var in variants)
            {
                accumulatedProbability += var.probability;
            }*/
            newRosterSize = Mathf.RoundToInt(cpdByType[tp].getProportionOfCategories(constraints.allCurrentConstraints[tp]) * (float)newRosterSize);
        }
        simulatedCurrentRosterSize = newRosterSize;

        // TODO +1?
        Debug.Log("New roster size " + newRosterSize);

        constrainedResult.Invoke(simulatedCurrentRosterSize);
    }

    public void reInitializeVariants(CPD_Type onType, List<string> buttonsAreOff)
    {
        CPD cpd = cpdByType[onType];
        rosterConstraints.clearConstraints(cpd);
        foreach(string exclude in buttonsAreOff)
        {
            rosterConstraints.addConstraint(cpd.cpdType, exclude);
        }
    }

    public void DebugLogRoster()
    {
        for (int i = 0; i < roster.Count; i++)
        {
            Debug.Log(roster[i]);
        }
    }


    /// Everything to do with the simulated ID
    public static class SimulatedID
    {
        /// <summary>
        /// Given a "simulated ID" in [0, rosterSize), return all CPD variants this character would generate with.
        ///     The ID itself contains what category each field will be. Every character is guaranteed a unique set of categories,
        ///     and the specific variants within those categories are chosen by random seed.
        /// For variants and all other non-constrainable, "cosmetic" CPD's, the simulated ID also acts as a random seed.
        /// Setting the random seed before getting those values ensures we always "randomly generate" the same output for the character.
        /// There's just one catch: We have to offset every random seed by a constant amount, so we do not get the exact same roster every time.
        /// </summary>
        /// <param name="simulatedId">Simulated id in [0, rosterSize)</param>
        /// <returns>All variants of the character with this simulated ID</returns>
        public static List<CPD_Variant> unpackSimulatedID(int simulatedId)
        {
            UnityEngine.Random.InitState(simulatedId); // TODO: Add by rosterOffset to make every game random.

            // We gotta get all the categories associated with each sim ID - one at a time.
            List<CPD_Variant> vars = new List<CPD_Variant>();
            int c = 0;
            for (int iter = 0; iter < cpdInstances.Count; iter++)
            {
                // Two distinct cases. If the CPD is constrainable it directly affects simulated ID. Otherwise it's just "random".
                if(cpdInstances[iter].constrainable)
                {
                    int currCPDcategory = 0;

                    CPD currCpd = cpdConstrainables[c];
                    currCPDcategory = Mathf.FloorToInt(simulatedId / simIDtourGuide[c]);
                    List<CPD_Variant> possibles = currCpd.getPossibleValuesFromCategory(currCpd.categories[currCPDcategory]);
                    vars.Add(possibles[UnityEngine.Random.Range(0, possibles.Count)]);

                    simulatedId -= simIDtourGuide[c] * currCPDcategory;
                    c++;
                } else
                {
                    vars.Add(cpdInstances[iter].getRandom());
                }
                
            }

            return vars;
        }


        /// <summary>
        /// Given some roster constraints, generate a random simulated ID for this character.
        /// For any CPDs with constraints, we must choose a value allowed by them.
        /// For any other CPDs without constraints, we can randomize them.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static int getRandomSimulatedID(RosterConstraints constraints)
        {
            int workingID = 0;
            for (int c = 0; c < cpdConstrainables.Count; c++)
            {
                CPD currCpd = cpdConstrainables[c];

                // If being constrained, carefully consider which values are allowed...
                if (constraints.allCurrentConstraints.ContainsKey(currCpd.cpdType))
                {
                    (int catId, int varId) = currCpd.getRandomConstrainedIndex(constraints.allCurrentConstraints[currCpd.cpdType]);
                    workingID += simIDtourGuide[c] * catId;
                }
                // Otherwise you can just pick anything...
                else
                {
                    workingID += simIDtourGuide[c] * currCpd.getRandomIndex();
                }
            }
            return workingID;
        }
    }
}





/// <summary>
/// List of roster constraints - what categories of what CPD's to sort by
/// </summary>
public class RosterConstraints
{
    // What CPD type are you restricting, and, what categories in that CPD are you allowing?
    public Dictionary<CPD_Type, HashSet<string>> allCurrentConstraints;

    public RosterConstraints()
    {
        this.allCurrentConstraints = new Dictionary<CPD_Type, HashSet<string>>();
    }

    /// <summary>
    /// Adds a category to the constrained list for a particular CPD (do not accept this category.)
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">This category will no longer be accepted</param>
    public void addConstraint(CPD_Type onType, string constraint)
    {
        allCurrentConstraints[onType].Add(constraint);
    }

    /// <summary>
    /// Removes a category from the constrained list for a particular CPD (the category will be allowed again.)
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">This category will once again be allowed</param>
    public void removeConstraint(CPD_Type onType, string constraint)
    {
        allCurrentConstraints[onType].Remove(constraint);
    }

    /// <summary>
    /// Makes the given value the only acceptable value for a particular field's constraints
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">Restricts everything but this category</param>
    public void onlyConstraint(CPD_Type onType, string constraint)
    {
        allCurrentConstraints[onType].Clear();
        Roster.cpdByType[onType].categories.ForEach(cat => allCurrentConstraints[onType].Add(cat));
        allCurrentConstraints[onType].Remove(constraint);
    }

    /// <summary>
    /// Removes all constraints from a CPD (all categories will be allowed again)
    /// </summary>
    /// <param name="cpd">Clear all constraints from this CPD</param>
    public void clearConstraints(CPD cpd)
    {
        CPD_Type onType = cpd.cpdType;
        if (allCurrentConstraints.ContainsKey(onType))
        {
            allCurrentConstraints[onType].Clear();
        }

        else
        {
            Debug.LogWarning($"Setting up constraints for {onType}");
            allCurrentConstraints.Add(onType, new HashSet<string>());
        }
    }
}

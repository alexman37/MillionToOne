using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The database of characters.
/// How it works at a high level: We don't actually store data for any characters, besides the ones we are showing.
/// We can load a character exactly as they should be by unpacking their "simulated ID"...more on that below.
/// </summary>
public class Roster
{
    const int TOTAL_ROSTER_PERMUTATIONS = 999999; // How many different rosters can there be?
    private int rosterSeedOffset;                 // offset every random seed by this amount. It makes each new game unique.

    public int simulatedTotalRosterSize = 1; // total number of "characters" we're working with
    private int simulatedCurrentRosterSize = 1; // total number of characters given constraints

    public List<Character> shownRoster; // real characters that actually exist because we had to generate them at some point.
    public List<Sprite> shownRosterSprites; // the sprites of the currently shown characters (for fast access).
    public HashSet<int> currentRosterIDs; // the simulated IDs of all characters we are currently showing.

    public RosterConstraints rosterConstraints;  // All constraints currently on the roster

    public static List<CPD> cpdInstances;      // All CPD singletons
    public static List<CPD> cpdConstrainables; // Only the constrainable CPDs (packaged in sim ID, in this order)
    public static Dictionary<CPD_Type, CPD> cpdByType; // get CPD singleton by type
    protected static List<int> cpdCounts; // optimization for simulated ID unpacking
    protected static List<int> simIDtourGuide; // the first CPD should be multiplied by index 0...the second by index 1...etc. to get sim ID.

    public static event Action rosterReady;
    public static event Action<int> constrainedResult;


    // Optimization for "get Random simulated ID":
    // In that function, if we fail to get a random ID by lucky chance several times, we do an exhaustive search through all possible sim IDs
    // Since it has to be done in order, we save our position in the search here...so if we need more sim IDs we can pick up where we left off.
    protected static bool lastResortSearch = false;
    protected static int savedCPD = 0;
    protected static int savedMod = 0;
    protected static List<int> newSimIdModifiers;
    protected static List<int> allSimIdModifiers;

    // Most of this is first-time setup only
    public Roster()
    {
        if (constrainedResult == null) constrainedResult += (_) => { };
        if (rosterReady == null) rosterReady += () => { };

        // No need to recreate CPDs on each load
        if(cpdInstances == null)
        {
            cpdInstances = new List<CPD>
            {
                new CPD_FilePath(CPD_Type.HairStyle, true, "properties/hairStyles", "CharSprites64/Hair/"),
                new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones"),
                new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones"),
                new CPD_FilePath(CPD_Type.BodyType, true, "properties/bodyTypes", "CharSprites64/Body/"),
                new CPD_FilePath(CPD_Type.Face, false, "properties/faceTypes", "CharSprites64/Face/"),
                new CPD_FilePath(CPD_Type.HeadType, false, "properties/headTypes", "CharSprites64/Head/"),
                new CPD_FilePath(CPD_Type.Test1, true, "properties/testType", null),
                new CPD_FilePath(CPD_Type.Test2, true, "properties/testType", null),
                new CPD_FilePath(CPD_Type.Test3, true, "properties/testType", null),
                new CPD_FilePath(CPD_Type.Test4, true, "properties/testType", null),
                new CPD_FilePath(CPD_Type.Test5, true, "properties/testType", null),
                new CPD_FilePath(CPD_Type.Test6, true, "properties/testType", null),
            };
            cpdConstrainables = new List<CPD>();
            cpdCounts = new List<int>();
            cpdByType = new Dictionary<CPD_Type, CPD>();
            simIDtourGuide = new List<int>();
            currentRosterIDs = new HashSet<int>();

            // Set constrainables list
            for (int c = 0; c < cpdInstances.Count; c++)
            {
                if (cpdInstances[c].constrainable)
                {
                    cpdConstrainables.Add(cpdInstances[c]);
                }
                cpdByType.Add(cpdInstances[c].cpdType, cpdInstances[c]);
            }
            // Set helpers for constrainables list
            for(int c = 0; c < cpdConstrainables.Count; c++)
            {
                int nextOffset = 1;
                cpdCounts.Add(cpdConstrainables[c].categories.Count);
                simulatedTotalRosterSize *= cpdConstrainables[c].categories.Count;

                for (int x = c + 1; x < cpdConstrainables.Count; x++)
                {
                    nextOffset *= cpdConstrainables[x].categories.Count;
                }
                simIDtourGuide.Add(nextOffset);
            }
        }

        simulatedCurrentRosterSize = simulatedTotalRosterSize;

        createRoster();
    }

    /// <summary>
    /// Called each time you start a new game.
    /// </summary>
    public void createRoster()
    {
        rosterSeedOffset = UnityEngine.Random.Range(0, TOTAL_ROSTER_PERMUTATIONS);
        if (shownRoster != null)
        {
            shownRoster.Clear();
            shownRosterSprites.Clear();
        } else
        {
            shownRoster = new List<Character>();
            shownRosterSprites = new List<Sprite>();
        }

        // "Clear" also serves as initialization for the constraints lists if need be
        rosterConstraints = new RosterConstraints();
        foreach (CPD cpd in cpdConstrainables)
        {
            rosterConstraints.clearConstraints(cpd);
        }
        
        applyConstraints(rosterConstraints);

        // First list generation
        for (int i = 0; i <= UI_Roster.CHARACTERS_TO_SHOW; i++)
        {
            int simId = SimulatedID.getRandomSimulatedID(rosterConstraints, currentRosterIDs, simulatedCurrentRosterSize);

            shownRoster.Add(new Character(i, simId));

            //Debug.Log("roster gen " + roster[i]);
            shownRosterSprites.Add(CharSpriteGen.genSpriteFromLayers(shownRoster[i]));
            currentRosterIDs.Add(simId);
        }

        lastResortSearch = false;
        savedCPD = 0;
        savedMod = 0;

        rosterReady.Invoke();
    }


    /// <summary>
    /// Redraw the roster with new characters meeting constraints
    /// </summary>
    public void redrawRosterVis()
    {
        applyConstraints(rosterConstraints);
        List<Character> newShownRoster = new List<Character>();
        int size = Mathf.Min(UI_Roster.CHARACTERS_TO_SHOW, simulatedCurrentRosterSize);

        // Characters to show: first, choose any from the currently shown roster we'd like to keep.
        int count = 0;
        currentRosterIDs.Clear();
        for (int i = 0; i < Mathf.Min(UI_Roster.CHARACTERS_TO_SHOW, shownRoster.Count) && count < size; i++)
        {
            if (SimulatedID.idMeetsConstraints(shownRoster[i].simulatedId, rosterConstraints))
            {
                currentRosterIDs.Add(shownRoster[i].simulatedId);
                newShownRoster.Add(shownRoster[i]);
                shownRosterSprites[count] = shownRosterSprites[i];
                count++;
            } else
            {
                currentRosterIDs.Remove(shownRoster[i].simulatedId);
            }
        }

        shownRoster = newShownRoster;
        for (int i = count; i < size; i++)
        {
            int simId = SimulatedID.getRandomSimulatedID(rosterConstraints, currentRosterIDs, simulatedCurrentRosterSize);

            shownRoster.Add(new Character(i, simId));

            shownRosterSprites[i] = CharSpriteGen.genSpriteFromLayers(shownRoster[i]);
            currentRosterIDs.Add(simId);
        }

        lastResortSearch = false;
        savedCPD = 0;
        savedMod = 0;

        UI_Roster.instance.regenerateCharCards(simulatedCurrentRosterSize);
    }

    /// <summary>
    /// Apply new constraints to the constraints list
    /// </summary>
    public void applyConstraints(RosterConstraints constraints)
    {
        // The roster size will decrease when applying a new constraint (and vice versa)
        int newRosterSize = simulatedTotalRosterSize;

        List<CPD_Type> types = new List<CPD_Type>(constraints.allCurrentConstraints.Keys);
        
        foreach (CPD_Type tp in types)
        {
            // Assuming all probabilities are equal.
            newRosterSize = Mathf.RoundToInt(cpdByType[tp].getProportionOfCategories(constraints.allCurrentConstraints[tp]) * (float)newRosterSize);
        }
        simulatedCurrentRosterSize = newRosterSize;

        // TODO +1?
        Debug.Log("New roster size " + newRosterSize);

        constrainedResult.Invoke(simulatedCurrentRosterSize);
    }

    /// <summary>
    /// When a FormButton unconfirms, clear all constraints (quick optimization)
    /// </summary>
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
        for (int i = 0; i < shownRoster.Count; i++)
        {
            Debug.Log(shownRoster[i]);
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
                    List<CPD_Variant> possibles = currCpd.getPossibleVariantsFromCategory(currCpd.categories[currCPDcategory]);
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
        public static int getRandomSimulatedID(RosterConstraints constraints, HashSet<int> takenIDs, int currentRosterSize)
        {
            // How this works on a technical level:
            //   - We will attempt to generate a random ID, see if it's already taken (for big current rosters, this is unlikely.)
            //   - If it is taken, we'll try the same process again a few more times.
            //   - If we have failed multiple times, we assume the constrained list is too crowded,
            //          so we resort to iterating through all possible constrained IDs; in order, until finding one that works.
            //   - Optimization: if the roster size is below a certain threshold, automatically resort to iterating through all IDs.
            if(currentRosterSize > 20)
            {
                for (int attempt = 0; attempt < 5; attempt++)
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
                    if (!takenIDs.Contains(workingID))
                    {
                        return workingID;
                    }
                }
            }

            // Worst case scenario: Resort to iteration through all possible IDs. Return the first success.
            for(int cpdIndex = savedCPD; cpdIndex < cpdConstrainables.Count; cpdIndex++)
            {
                CPD currCpd = cpdConstrainables[cpdIndex];

                int magicNumber = simIDtourGuide[cpdIndex];
                List<int> currSimIdModifiers = currCpd.getAllConstrainedIndicies(constraints.allCurrentConstraints[currCpd.cpdType]);
                for(int i = 0; i < currSimIdModifiers.Count; i++)
                {
                    currSimIdModifiers[i] = magicNumber * currSimIdModifiers[i];
                }
                int catZeroes = 0;
                for(int i = cpdIndex + 1; i < cpdConstrainables.Count; i++)
                {
                    // There must be at least one category or there's a problem...
                    catZeroes += simIDtourGuide[i] * cpdConstrainables[i].getAllConstrainedIndicies(constraints.allCurrentConstraints[cpdConstrainables[i].cpdType])[0];
                }

                if (savedCPD == 0)
                {
                    allSimIdModifiers = new List<int>();
                }
                if (savedMod == 0)
                {
                    newSimIdModifiers = new List<int>();
                }
                // First pass
                if (allSimIdModifiers.Count == 0)
                {
                    for (int l = 0; l < currSimIdModifiers.Count; l++)
                    {
                        int aNewModifier = currSimIdModifiers[l];
                        int aNewIndex = aNewModifier + catZeroes;
                        newSimIdModifiers.Add(aNewModifier);
                        if (!takenIDs.Contains(aNewIndex))
                        {
                            return aNewIndex;
                        }
                    }
                } 
                // Every subsequent pass
                else
                {
                    for (int i = savedMod; i < allSimIdModifiers.Count; i++)
                    {
                        for (int l = 0; l < currSimIdModifiers.Count; l++)
                        {
                            int mod = allSimIdModifiers[i];
                            int aNewModifier = mod + currSimIdModifiers[l];
                            int aNewIndex = aNewModifier + catZeroes;
                            newSimIdModifiers.Add(aNewModifier);
                            if (!takenIDs.Contains(aNewIndex))
                            {
                                return aNewIndex;
                            }
                        }
                        savedMod++;
                    }
                }

                allSimIdModifiers = newSimIdModifiers;
                savedCPD++;
                savedMod = 0;
            }

            // Should be impossible
            Debug.LogError("Could not find any valid simulated ID");
            return -1;
        }

        /// <summary>
        /// Returns whether or not this ID is valid given a set of constraints
        /// </summary>
        /// <param name="simulatedId"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static bool idMeetsConstraints(int simulatedId, RosterConstraints constraints)
        {
            for (int iter = 0; iter < cpdConstrainables.Count; iter++)
            {
                int currCPDcategory = 0;

                CPD currCpd = cpdConstrainables[iter];
                currCPDcategory = Mathf.FloorToInt(simulatedId / simIDtourGuide[iter]);
                if(constraints.allCurrentConstraints[currCpd.cpdType].Contains(currCpd.categories[currCPDcategory]))
                {
                    return false;
                } else
                {
                    simulatedId -= simIDtourGuide[iter] * currCPDcategory;
                }
            }

            return true;
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
            //Debug.LogWarning($"Setting up constraints for {onType}");
            allCurrentConstraints.Add(onType, new HashSet<string>());
        }
    }
}

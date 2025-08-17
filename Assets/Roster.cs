using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Facilitates generation, storage and access of the list of 100 suspects (characters 1-100), and the murder victim (character 0).
public class Roster
{
    public int simulatedRosterSize; // total number of "characters" we're working with
    public List<Character> roster; // real characters that actually exist because we had to generate them at some point.
    public List<Sprite> rosterSprites; //consistent list of the portrait per each character
    public RosterDemographicMap rosterDMap;

    public static event Action<int> constrainedResult;

    public Roster(int numChars)
    {
        if (constrainedResult == null) constrainedResult += (_) => { };
        simulatedRosterSize = numChars;

        createRoster();
    }

    public void createRoster()
    {
        if (roster != null)
        {
            roster.Clear();
            rosterSprites.Clear();
        } else
        {
            roster = new List<Character>();
            rosterSprites = new List<Sprite>();
        }

        // Initialize all CPDs
        CPD_Hair cpd_hair = new CPD_Hair();
        CPD_HairColor cpd_hairColor = new CPD_HairColor();
        CPD_SkinTone cpd_skinTone = new CPD_SkinTone();
        CPD_BodyType cpd_bodyType = new CPD_BodyType();
        CPD_HeadType cpd_headType = new CPD_HeadType();
        CPD_Face cpd_face = new CPD_Face();

        // Generate Demographic Mappings.
        rosterDMap = new RosterDemographicMap();
        rosterDMap.constraints = new RosterConstraintList();

        initializeConstraint("CPD_Hair", cpd_hair.variants);
        initializeConstraint("CPD_HairColor", cpd_hairColor.variants);
        initializeConstraint("CPD_SkinTone", cpd_skinTone.variants);
        initializeConstraint("CPD_BodyType", cpd_bodyType.variants);

        
        applyConstraints(rosterDMap.constraints);

        // TODO REMOVE
        for (int i = 0; i <= UI_Roster.CHARACTERS_TO_SHOW; i++)
        {
            roster.Add(new Character(
                i //the id
            ));

            roster[i].randomizeDemographicsWithConstraints(rosterDMap.constraints);

            //Debug.Log("roster gen " + roster[i]);
            rosterSprites.Add(CharSpriteGen.genSpriteFromLayers(roster[i]));
        }
    }

    public void initializeConstraint(string fieldName, CPD_Field[] variants)
    {
        foreach(CPD_Field field in variants)
        {
            foreach (string s in field.generalDesc)
            {
                rosterDMap.constraints.addConstraint(fieldName, s);
            }
        }
    }

    /// <summary>
    /// Adds an acceptable value to the constrained list for a particular field.
    /// </summary>
    /// <param name="fieldName">Field name to constrain (EG Hair, HairColor...)</param>
    /// <param name="value">Value to accept (EG Gray, White...)</param>
    public void addConstraint(string fieldName, string value)
    {
        // Add to constraints list
        rosterDMap.constraints.addConstraint(fieldName, value);
        redrawRosterVis();
    }

    /// <summary>
    /// Removes an acceptable value from the constrained list for a particular field.
    /// </summary>
    /// <param name="fieldName">Field name to constrain (EG Hair, HairColor...)</param>
    /// <param name="value">Value to no longer accept (EG Gray, White...)</param>
    public void removeConstraint(string fieldName, string value)
    {
        // Add to constraints list
        rosterDMap.constraints.removeConstraint(fieldName, value);
        redrawRosterVis();
    }

    /// <summary>
    /// Makes the given value the only acceptable value for a particular field's constraints
    /// </summary>
    /// <param name="fieldName">Field name to constrain (EG Hair, HairColor...)</param>
    /// <param name="value">Sole value to accept (EG Gray, White...)</param>
    public void onlyConstraint(string fieldName, string value)
    {
        // Add to constraints list
        rosterDMap.constraints.onlyConstraint(fieldName, value);
        redrawRosterVis();
    }


    private void redrawRosterVis()
    {
        // TODO do this in a separate step.
        applyConstraints(rosterDMap.constraints);

        // Characters to show
        for (int i = 0; i <= UI_Roster.CHARACTERS_TO_SHOW; i++)
        {
            roster.Add(new Character(
                i //the id
            ));

            roster[i].randomizeDemographicsWithConstraints(rosterDMap.constraints);

            //Debug.Log("roster gen " + roster[i]);
            rosterSprites[i] = CharSpriteGen.genSpriteFromLayers(roster[i]);
        }
    }

    public void applyConstraints(RosterConstraintList constraints)
    {
        int newRosterSize = simulatedRosterSize;
        // Because of RosterConstraintList's strict adherence to one entry per CPD, we can assume each entry represents a unique CPD
        // And all possible values for that CPD are included.
        
        foreach (RosterConstraint constraint in constraints.allCurrentConstraints)
        {
            CPD_Field[] fields = stringToCPD(constraint.onField, constraint.possibleValues);
            float accumulatedProbability = 0;

            foreach (CPD_Field field in fields)
            {
                Debug.Log(field + " added probability " + field.probability);
                accumulatedProbability += field.probability;
            }
            Debug.Log(accumulatedProbability);
            newRosterSize = Mathf.CeilToInt(accumulatedProbability * (float)newRosterSize);
        }
        
        Debug.Log("New roster size " + newRosterSize);

        constrainedResult.Invoke(newRosterSize);
    }

    public void reInitializeVariants(string group, List<string> buttonsAreOff)
    {
        initializeConstraintFromString(group);
        foreach(string exclude in buttonsAreOff)
        {
            removeConstraint(group, exclude);
        }
    }

    public void DebugLogRoster()
    {
        for (int i = 0; i < roster.Count; i++)
        {
            Debug.Log(roster[i]);
        }
    }

    // fugly ass method
    private CPD_Field[] stringToCPD(string str, HashSet<string> possibles)
    {
        switch (str)
        {
            case "CPD_Hair": return CPD_Hair.instance.getPossibleValuesFromGenDesc(possibles);
            case "CPD_BodyType": return CPD_BodyType.instance.getPossibleValuesFromGenDesc(possibles);
            case "CPD_HairColor": return CPD_HairColor.instance.getPossibleValuesFromGenDesc(possibles);
            case "CPD_SkinTone": return CPD_SkinTone.instance.getPossibleValuesFromGenDesc(possibles);
            default: return null;
        }
    }

    private void initializeConstraintFromString(string str)
    {
        switch (str)
        {
            case "CPD_Hair": initializeConstraint(str, CPD_Hair.instance.variants); break;
            case "CPD_BodyType": initializeConstraint(str, CPD_BodyType.instance.variants); break;
            case "CPD_HairColor": initializeConstraint(str, CPD_HairColor.instance.variants); break;
            case "CPD_SkinTone": initializeConstraint(str, CPD_SkinTone.instance.variants); break;
            default: break;
        }
    }
}

public class RosterDemographicMap
{
    public RosterConstraintList constraints;

    public RosterDemographicMap()
    {
    }
}

public class RosterConstraintList
{
    public List<RosterConstraint> allCurrentConstraints;

    public RosterConstraintList()
    {
        this.allCurrentConstraints = new List<RosterConstraint>();
    }

    // Add to constraints list
    public void addConstraint(string fieldTypeName, string constraint)
    {
        bool found = false;
        foreach(RosterConstraint c in allCurrentConstraints)
        {
            if (c.onField == fieldTypeName)
            {
                found = true;
                if (!c.possibleValues.Contains(constraint))
                {
                    c.possibleValues.Add(constraint);
                }
                break;
            }
        }

        if(!found) {
            allCurrentConstraints.Add(new RosterConstraint(fieldTypeName, new HashSet<string> { constraint }));
        }
    }

    public void removeConstraint(string fieldTypeName, string constraint)
    {
        bool found = false;
        foreach (RosterConstraint c in allCurrentConstraints)
        {
            if (c.onField == fieldTypeName)
            {
                found = true;
                if (c.possibleValues.Contains(constraint))
                {
                    c.possibleValues.Remove(constraint);
                }
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"Didn't find a field {fieldTypeName} to remove constraints from.");
        }
    }

    public void onlyConstraint(string fieldTypeName, string constraint)
    {
        bool found = false;
        foreach (RosterConstraint c in allCurrentConstraints)
        {
            if (c.onField == fieldTypeName)
            {
                found = true;
                c.possibleValues.Clear();
                c.possibleValues.Add(constraint);
                break;
            }
        }

        if (!found)
        {
            allCurrentConstraints.Add(new RosterConstraint(fieldTypeName, new HashSet<string> { constraint }));
        }
    }
}

public class RosterConstraint
{
    public string onField;
    public HashSet<string> possibleValues; // GENERIC NAMES, not actual, fully descriptive names.

    public RosterConstraint(string onField, HashSet<string> acceptableVals)
    {
        this.onField = onField;
        this.possibleValues = acceptableVals;
    }
}

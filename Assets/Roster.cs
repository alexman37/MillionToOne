using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Facilitates generation, storage and access of the list of 100 suspects (characters 1-100), and the murder victim (character 0).
public class Roster
{
    public List<Character> roster;
    public List<Sprite> rosterSprites; //consistent list of the portrait per each character
    public RosterDemographicMap rosterDMap;

    public static event Action<int> constrainedResult;

    public Roster(int numChars)
    {
        if (constrainedResult == null) constrainedResult += (_) => { };

        createRoster(numChars);
    }

    public void createRoster(int numChars)
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

        Dictionary<string, CPD_Field[]> mappings = new Dictionary<string, CPD_Field[]>();
        mappings.Add("CPD_Hair", cpd_hair.variants);
        mappings.Add("CPD_HairColor", cpd_hairColor.variants);
        mappings.Add("CPD_SkinTone", cpd_skinTone.variants);
        mappings.Add("CPD_BodyType", cpd_bodyType.variants);
        mappings.Add("CPD_HeadType", cpd_headType.variants);
        mappings.Add("CPD_Face", cpd_face.variants);

        // Generate Demographic Mappings.
        rosterDMap = new RosterDemographicMap(mappings);
        rosterDMap.constraints = new RosterConstraintList();
        rosterDMap.constraints.addConstraint("CPD_HairColor", "Gray");
        rosterDMap.constraints.addConstraint("CPD_HairColor", "Ginger");
        rosterDMap.constraints.addConstraint("CPD_BodyType", "Normal");
        rosterDMap.constraints.addConstraint("CPD_SkinTone", "Mixed");
        rosterDMap.constraints.addConstraint("CPD_Hair", "Normal");
        applyConstraints(numChars, rosterDMap.constraints);

        // TODO REMOVE
        for (int i = 0; i <= numChars; i++)
        {
            roster.Add(new Character(
                i //the id
            ));

            roster[i].randomizeDemographicsWithConstraints(rosterDMap.constraints);

            //Debug.Log("roster gen " + roster[i]);
            rosterSprites.Add(CharSpriteGen.genSpriteFromLayers(roster[i]));
        }
    }

    public void applyConstraints(int oldRosterSize, RosterConstraintList constraints)
    {
        int newRosterSize = oldRosterSize;
        // Because of RosterConstraintList's strict adherence to one entry per CPD, we can assume each entry represents a unique CPD
        // And all possible values for that CPD are included.
        foreach(RosterConstraint constraint in constraints.allCurrentConstraints)
        {
            CPD_Field[] fields = stringToCPD(constraint.onField, constraint.possibleValues);
            Debug.Log(fields.Length);
            float accumulatedProbability = 0;
            foreach(CPD_Field field in fields)
            {
                Debug.Log(field);
                accumulatedProbability += field.probability;
            }
            newRosterSize = Mathf.CeilToInt(accumulatedProbability * (float)newRosterSize);
            Debug.Log("New roster size " + newRosterSize);
        }

        constrainedResult.Invoke(newRosterSize);
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
}

public class RosterDemographicMap
{
    public Dictionary<string, CPD_Field[]> demographicMappings;
    public RosterConstraintList constraints;

    public RosterDemographicMap(Dictionary<string, CPD_Field[]> maps)
    {
        demographicMappings = maps;
    }
}

public class RosterConstraintList
{
    public List<RosterConstraint> allCurrentConstraints;

    public RosterConstraintList()
    {
        this.allCurrentConstraints = new List<RosterConstraint>();
    }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
public class Character
{
    //Demographics: they are physical, undeniable descriptions of a person that can be discovered simply by seeing / meeting them.
    public int rosterId; // Where in the roster list of known characters (and sprites) this person is.
    public int simulatedId; // The unique ID from (0 - rosterSize - 1) that contains all this character's constrainable CPD values
                     // All other (cosmetic) random values generated using this simulatedId as a seed
    Dictionary<CPD_Type, CPD_Variant> createdCharacteristics; // Once we create a character we can assign them data in here


    // TODO ???
    //Attributes
    string firstName;
    string lastName;
    /*(int day, int month) birthday;
    int age;

    //Relationships
    int[] family; //Murderer fairly unlikely to kill family
    int[] friends; //Murderer VERY unlikely to kill a friend
    int[] contacts; //Can be a good or bad relationship
    int[] enemies; //Murderer fairly likely to kill enemies

    //Traits
    //List<Trait> traits = new List<Trait>();*/

    // The only thing you need to create a character is their position in the roster and their simulated ID!
    // Everything else can be determined on the fly as necessary
    public Character(int rosterId, int simulatedId)
    {
        this.rosterId = rosterId;
        this.simulatedId = simulatedId;

        randomizeDemographics();
    }

    /// <summary>
    /// Randomize demographics (CPDs) for this character.
    /// unpackSimulationID does most of the heavy lifting in this regard
    /// </summary>
    public void randomizeDemographics()
    {
        // Generate random demographics
        List<CPD_Variant> temp = Roster.SimulatedID.unpackSimulatedID(simulatedId);
        createdCharacteristics = new Dictionary<CPD_Type, CPD_Variant>();
        foreach (CPD_Variant var in temp)
        {
            createdCharacteristics.Add(var.cpdType, var);
        }

        // There are some other traits we want to give our characters here
        // We can still get away with using "random" traits, since the randomSeed was set to a predictable value in unpackSimulationID
        // and will not be reset until we call it again.
        (string f, string l) fullName = CharRandomValue.randomName(true); // TODO isMale
        firstName = fullName.f;
        lastName = fullName.l;
    }

    /// <summary>
    /// Get the CPD id of a characteristic
    /// CPD id == the index of this variant in the CPD's variants list.
    /// </summary>
    public int getCpdIDofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].cpdID;
    }

    /// <summary>
    /// Gets the filepath value from a CPD assumed to be a filepath
    /// </summary>
    public string getFilePath(CPD_Type cpdType)
    {
        return (createdCharacteristics[cpdType].critVal as CPD_CritVal_Filepath).filepath;
    }

    /// <summary>
    /// Gets the color value from a CPD assumed to be a color
    /// </summary>
    public Color getColorField(CPD_Type cpdType)
    {
        return (createdCharacteristics[cpdType].critVal as CPD_CritVal_Color).col;
    }

    /// <summary>
    /// Character's display name is just their first and last name
    /// </summary>
    public string getDisplayName(bool newline)
    {
        if (newline == false) return firstName + " " + lastName;
        else return firstName + "\n" + lastName;
    }

    public override string ToString()
    {
        string str = $"RosterID = [{rosterId}], SimulatedID = [{simulatedId}]\n" +
            "Name: " + firstName + " " + lastName + "\n" + "";
            /*"Body: " + bodyType + "\n" +
            "Head: " + headType + "\n" +
            "Ht: " + height + "\n" +
            "Wt: " + weight + "\n" +
            "Male: " + isMale + "\n" +
            "SkinTone: " + skinTone + "\n" +
            "Hair: " + hairStyle + "," + hairColor + "\n";*/
        return str;
    }
}

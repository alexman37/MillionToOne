using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//demographics are physical, undeniable descriptions of a person that can be discovered simply by seeing / meeting them.
public class Character
{
    //Demographics
    int rosterId; // Where in the roster list of known characters (and sprites) this person is.
    int simulatedId; // The unique ID from (0 - rosterSize - 1) that contains all this character's constrainable CPD values
                     // All other (cosmetic) random values generated using this simulatedId as a seed
    Dictionary<CPD_Type, CPD_Variant> createdCharacteristics; // Once we create a character we can assign them data in here

    //Attributes
    string firstName;
    string lastName;
    (int day, int month) birthday;
    int age;

    //Relationships
    int[] family; //Murderer fairly unlikely to kill family
    int[] friends; //Murderer VERY unlikely to kill a friend
    int[] contacts; //Can be a good or bad relationship
    int[] enemies; //Murderer fairly likely to kill enemies

    //Traits
    //List<Trait> traits = new List<Trait>();

    //Randomize everything!
    public Character(int rosterId, int simulatedId)
    {
        this.rosterId = rosterId;
        this.simulatedId = simulatedId;

        randomizeDemographics();
    }

    // Randomize demographics, for all fields NOT set yet (for ex., by constraints.)
    public void randomizeDemographics()
    {
        // CRIT POINT
        List<CPD_Variant> temp = Roster.SimulatedID.unpackSimulatedID(simulatedId);
        createdCharacteristics = new Dictionary<CPD_Type, CPD_Variant>();
        foreach (CPD_Variant var in temp)
        {
            createdCharacteristics.Add(var.cpdType, var);
        }

        //name
        (string f, string l) fullName = CharRandomValue.randomName(true); // TODO isMale
        firstName = fullName.f;
        lastName = fullName.l;
    }

    public int getCpdIDofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].cpdID;
    }

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

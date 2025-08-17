using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//demographics are physical, undeniable descriptions of a person that can be discovered simply by seeing / meeting them.
public class Character
{
    //Demographics
    int id; //Secret id that keeps track of which character this is in the scenario (0-100). 0 is the murder victim.
    int height;
    int weight;
    bool isMale;
    public BodyType bodyType;
    public HeadType headType;
    public SkinTone skinTone;
    public HairStyle hairStyle;
    public HairColor hairColor;
    public FaceType face;
    CPD_Moustache moustache;
    CPD_Beard beard;
    CPD_Glasses glasses;
    CPD_EyeColor eyeColor;

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
    public Character(int id)
    {
        this.id = id;
    }

    // Randomize demographics, for all fields NOT set yet (for ex., by constraints.)
    public void randomizeDemographics(bool heightSet, bool weightSet, bool isMaleSet)
    {
        // TODO - are we gonna replace these? Or make them more like the others?
        if (!heightSet) height = CharRandomValue.range(0, 6);
        if (!weightSet) weight = CharRandomValue.range(0, 6);
        if (!isMaleSet) isMale = CharRandomValue.coin();

        if (headType == null) headType = CPD_HeadType.instance.getRandom();
        if (bodyType == null) bodyType = CPD_BodyType.instance.getRandom();
        if (face == null) face = CPD_Face.instance.getRandom();
        if (skinTone == null) skinTone = CPD_SkinTone.instance.getRandom();
        if (hairColor == null) hairColor = CPD_HairColor.instance.getRandom();
        if (hairStyle == null) hairStyle = CPD_Hair.instance.getRandom();

        //name
        (string f, string l) fullName = CharRandomValue.randomName(isMale);
        firstName = fullName.f;
        lastName = fullName.l;
    }

    public void randomizeDemographicsWithConstraints(RosterConstraintList constraints)
    {
        if(constraints != null)
        {
            foreach (RosterConstraint constraint in constraints.allCurrentConstraints)
            {
                switch (constraint.onField)
                {
                    case "CPD_BodyType":
                        bodyType = CPD_BodyType.instance.getRandomConstrained(constraint.possibleValues);
                        break;
                    case "CPD_Hair":
                        hairStyle = CPD_Hair.instance.getRandomConstrained(constraint.possibleValues);
                        break;
                    case "CPD_SkinTone":
                        skinTone = CPD_SkinTone.instance.getRandomConstrained(constraint.possibleValues);
                        break;
                    case "CPD_HairColor":
                        hairColor = CPD_HairColor.instance.getRandomConstrained(constraint.possibleValues);
                        break;
                    default: break;
                }
            }
        }
        
        randomizeDemographics(false, false, false); // TODO constrain on gender
    }

    public string getDisplayName(bool newline)
    {
        if (newline == false) return firstName + " " + lastName;
        else return firstName + "\n" + lastName;
    }

    public override string ToString()
    {
        string str = "[" + id + "] " + firstName + " " + lastName + "\n" +
            "Body: " + bodyType + "\n" +
            "Head: " + headType + "\n" +
            "Ht: " + height + "\n" +
            "Wt: " + weight + "\n" +
            "Male: " + isMale + "\n" +
            "SkinTone: " + skinTone + "\n" +
            "Hair: " + hairStyle + "," + hairColor + "\n";
        return str;
    }
}

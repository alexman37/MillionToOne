using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//During character generation, values for a character are randomized. This organizes it.
public static class CharRandomValue
{
    private static List<string> firstNamesM = getNames("FirstNamesM");
    private static List<string> firstNamesF = getNames("FirstNamesF");
    private static List<string> lastNames = getNames("LastNames");

    private static int firstNamesMSize = firstNamesM.Count;
    private static int firstNamesFSize = firstNamesF.Count;
    private static int lastNamesSize = lastNames.Count;

    //Return a random integer in range (min, max).
    //Used by: height, weight, etc.
    public static int range(int min, int max)
    {
        return Random.Range(min, max);
    }

    //Return a random boolean
    //Used by: isMale
    public static bool coin()
    {
        return (Random.value > 0.5f);
    }

    //Return a random name
    //The list of first and last names is supplied in "FirstNamesM/F.txt" and "LastNamesM/F.txt"
    public static (string, string) randomName(bool isMale)
    {
        if (isMale) return (firstNamesM[Random.Range(0, firstNamesMSize)], lastNames[Random.Range(0, lastNamesSize)]);
        else return (firstNamesF[Random.Range(0, firstNamesFSize)], lastNames[Random.Range(0, lastNamesSize)]);
    }

    private static List<string> getNames(string path)
    {
        TextAsset file = Resources.Load<TextAsset>(path);
        return new List<string>(file.text.Split('\n'));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Character Profile Demographic." A single feature that a character has, such as their hair, hair color, or skin tone.
/// A complete character is made up of multiple of these.
/// </summary>
public class CPD<T> where T : CPD_Field, new()
{
    private string propertiesPath;
    private string spritesPath;

    // Given the path of this CPD's properties file and where its sprites are stored, we can initialize all variants.
    public CPD(string propertiesPath, string spritesPath, CPD_Type tp)
    {
        this.propertiesPath = propertiesPath;

        if(tp == CPD_Type.Filename)
        {
            this.spritesPath = spritesPath;
            variants = initialize_FileName();
        }
        else
        {
            variants = initialize_Color();
        }
            
    }

    private float probCounter = 0.0f;
    private float probX = 0.0f;

    public T[] variants;
    private Dictionary<string, List<T>> genericNamesToVariants; 

    // Create all variants of this CPD from the properties path (FileName variant).
    private T[] initialize_FileName()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath);
        string[] lines = txt.text.Split('\n');
        List<T> temporaryStorage = new List<T>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Split('#')[0].Trim(); //ignore comments
            if (line.Length > 0)
            {
                string[] fields = line.Split(';');

                //probability
                float p;
                if (fields[2] == "X")
                {
                    probX += 1.0f;
                    p = -1;
                }
                else
                {
                    p = float.Parse(fields[2]);
                    probCounter += p;
                }

                T st = new T();

                CPD_Field_FileName variant = st as CPD_Field_FileName;
                variant.id = i;
                variant.name = fields[1];
                variant.generalDesc = fields[3].Split(',');
                variant.probability = p;
                variant.filename = spritesPath + fields[0];

                temporaryStorage.Add(variant as T);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD FileName " + new T().GetType() + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        T[] vars = new T[temporaryStorage.Count];
        genericNamesToVariants = new Dictionary<string, List<T>>();
        for (int i = 0; i < temporaryStorage.Count; i++)
        {
            T st = temporaryStorage[i];

            if (st.probability == -1)
            {
                st.probability = probX;
            }

            vars[i] = st;

            foreach(string gen in st.generalDesc)
            {
                if (!genericNamesToVariants.ContainsKey(gen))
                {
                    genericNamesToVariants[gen] = new List<T> { st };
                }
                else
                {
                    genericNamesToVariants[gen].Add(st);
                }
            }
        }

        return vars;
    }

    // Create all variants of this CPD from the properties path (Color variant).
    private T[] initialize_Color()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath);
        string[] lines = txt.text.Split('\n');
        List<T> temporaryStorage = new List<T>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Split('#')[0].Trim(); //ignore comments
            if (line.Length > 0)
            {
                string[] fields = line.Split(';');

                //color
                float[] colors = new float[3];
                string[] strColors = fields[2].Split(',');
                for (int s = 0; s < strColors.Length; s++)
                {
                    string curr = strColors[s];
                    if (strColors[s].Contains("-"))
                    {
                        string[] _ = curr.Split('-');
                        colors[s] = Random.Range(int.Parse(_[0]), int.Parse(_[1])) / 255.0f;
                    }
                    else if (strColors[s].Contains("X"))
                    {
                        if (strColors[s] == "X0") colors[s] = colors[0];
                        if (strColors[s] == "X1") colors[s] = colors[1];
                    }
                    else colors[s] = int.Parse(strColors[s]) / 255.0f;
                }
                Color col = new Color(colors[0], colors[1], colors[2]);

                //probability
                float p;
                if (fields[1] == "X")
                {
                    probX += 1.0f;
                    p = -1;
                }
                else
                {
                    p = float.Parse(fields[1]);
                    probCounter += p;
                }

                T st = new T();

                CPD_Field_Color variant = st as CPD_Field_Color;
                variant.id = i;
                variant.name = fields[0];
                variant.generalDesc = fields[3].Split(',');
                variant.probability = p;
                variant.color = col;

                temporaryStorage.Add(variant as T);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD Color " + new T().GetType() + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        T[] vars = new T[temporaryStorage.Count];
        genericNamesToVariants = new Dictionary<string, List<T>>();
        for (int i = 0; i < temporaryStorage.Count; i++)
        {
            T st = temporaryStorage[i];

            if (st.probability == -1)
            {
                st.probability = probX;
            }

            vars[i] = st;

            foreach (string gen in st.generalDesc)
            {
                if (!genericNamesToVariants.ContainsKey(gen))
                {
                    genericNamesToVariants[gen] = new List<T> { st };
                }
                else
                {
                    genericNamesToVariants[gen].Add(st);
                }
            }
        }

        return vars;
    }

    public T getRandom()
    {
        return variants[Random.Range(0, variants.Length)];
    }

    public T getRandomConstrained(HashSet<string> acceptableValues)
    {
        List<T> allPossible = new List<T>();
        foreach(string acceptable in acceptableValues)
        {
            allPossible.AddRange(genericNamesToVariants[acceptable]);
        }
        if (allPossible.Count == 0) return null;
        else return allPossible[Random.Range(0, allPossible.Count)];
    }

    // For a single constraint
    public T[] getPossibleValuesFromGenDesc(string genDesc)
    {
        T[] arrs = new T[genericNamesToVariants[genDesc].Count];
        for(int i = 0; i < genericNamesToVariants[genDesc].Count; i++)
        {
            arrs[i] = genericNamesToVariants[genDesc][i];
        }
        return arrs;
    }

    // For multiple constraints
    public T[] getPossibleValuesFromGenDesc(IEnumerable genDescs)
    {
        List<T[]> temp2d = new List<T[]>();
        HashSet<int> underTheHoodDuplicates = new HashSet<int>();
        int count = 0;
        foreach (string genDesc in genDescs)
        {
            T[] arrs = new T[genericNamesToVariants[genDesc].Count];
            List<T> cachedList = genericNamesToVariants[genDesc];

            for (int i = 0; i < arrs.Length; i++)
            {
                if(!underTheHoodDuplicates.Contains(cachedList[i].id))
                {
                    arrs[i] = cachedList[i];
                    count += 1;
                    underTheHoodDuplicates.Add(cachedList[i].id);
                }
            }
            temp2d.Add(arrs);
            
        }

        T[] merged = new T[count];
        count = 0;
        foreach(T[] items in temp2d)
        {
            for(int i = 0; i < items.Length; i++)
            {
                if(items[i] != null)
                {
                    merged[count] = items[i];
                    count++;
                }
            }
        }

        return merged;
    }
}

public enum CPD_Type
{
    Filename,
    Color
}

/// <summary>
/// Defines the broad properties of all variants of a CPD.
/// For example, for Body types - you make a BodyType class that inherits this,
/// Then create a single instance for each new body type.
/// There are methods in place to select a specific BodyType by ID, or just get a random one.
/// </summary>
public class CPD_Field
{
    public int id;
    public string name;
    public string[] generalDesc;
    public float probability;

    public CPD_Field()
    {
        // We need this constructore for...reasons.
    }

    public CPD_Field(int id, string name, string[] desc, float prob)
    {
        this.id = id;
        this.name = name;
        this.generalDesc = desc;
        this.probability = prob;
    }

    // All CPD fields print out their name.
    public override string ToString()
    {
        return this.name;
    }
}


/// <summary>
/// Most CPD fields which use a sprite or something else have to load it from a file.
/// </summary>
public class CPD_Field_FileName : CPD_Field
{
    public string filename;

    public CPD_Field_FileName(int id, string name, string[] general, float probability, string filename) : base(id, name, general, probability)
    {
        this.filename = filename;
    }

    public Sprite getSprite()
    {
        return Resources.Load<Sprite>(filename);
    }
}

/// <summary>
/// Some CPD fields are simply differences in color
/// </summary>
public class CPD_Field_Color : CPD_Field
{
    public Color32 color;

    public CPD_Field_Color(int id, string name, string[] general, float probability, Color32 color) : base(id, name, general, probability)
    {
        this.color = color;
    }
}
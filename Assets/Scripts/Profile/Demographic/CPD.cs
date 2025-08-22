using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Character Profile Demographic." A single feature that a character has, such as their hair, hair color, or skin tone.
/// A complete character is made up of multiple of these.
/// </summary>
public abstract class CPD
{
    public CPD_Type cpdType;
    protected string propertiesPath;
    public bool constrainable;

    protected float probCounter = 0.0f;
    protected float probX = 0.0f;

    public List<CPD_Variant> variants;
    public List<string> categories;
    protected Dictionary<string, int> categoryIndices;
    protected Dictionary<string, List<CPD_Variant>> categoriesToVariants;

    public abstract List<CPD_Variant> initialize();

    public CPD_Variant getRandom()
    {
        return variants[Random.Range(0, variants.Count)];
    }

    public int getRandomIndex()
    {
        return Random.Range(0, variants.Count);
    }

    public CPD_Variant getRandomConstrained(HashSet<string> restrictedCats)
    {
        List<CPD_Variant> allPossible = new List<CPD_Variant>();
        foreach(string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                allPossible.AddRange(categoriesToVariants[cat]);
            }
        }
        if (allPossible.Count == 0) return null;
        else return allPossible[Random.Range(0, allPossible.Count)];
    }

    public (int catId, int varId) getRandomConstrainedIndex(HashSet<string> restrictedCats)
    {
        CPD_Variant chosen = getRandomConstrained(restrictedCats);
        return (categoryIndices[chosen.category], chosen.cpdID);
    }

    // For a single constraint
    public List<CPD_Variant> getPossibleValuesFromCategory(string cat)
    {
        return categoriesToVariants[cat];
    }

    // For a single constraint
    public List<CPD_Variant> getConstrainedCategoryVariants(HashSet<string> restrictedCats)
    {
        List<CPD_Variant> allPossible = new List<CPD_Variant>();
        foreach (string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                allPossible.AddRange(getPossibleValuesFromCategory(cat));
            }
        }
        return allPossible;
    }

    // For multiple constraints
    public List<CPD_Variant> getPossibleValuesFromCategory(IEnumerable categories)
    {
        List<CPD_Variant> temp2d = new List<CPD_Variant>();
        foreach (string cat in categories)
        {
            temp2d.AddRange(categoriesToVariants[cat]);
        }
        return temp2d;
    }
}


public enum CPD_Type
{
    // Constrainable
    HairStyle,
    HairColor,
    SkinTone,
    BodyType, // TODO remove

    // Not constrainable
    Face,
    HeadType
}


// A CPD where critical values are loaded from a file
public class CPD_FilePath : CPD
{
    private string spritesPath;

    // Given the path of this CPD's properties file and where its sprites are stored, we can initialize all variants.
    public CPD_FilePath(CPD_Type cat, bool constrainable, string propertiesPath, string spritesPath)
    {
        this.constrainable = constrainable;
        this.cpdType = cat;
        this.propertiesPath = propertiesPath;
        this.spritesPath = spritesPath;
        initialize(); 
    }

    public override List<CPD_Variant> initialize()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath); // TODO use assetbundles instead?
        string[] lines = txt.text.Split('\n');

        variants = new List<CPD_Variant>();
        categoriesToVariants = new Dictionary<string, List<CPD_Variant>>();
        categories = new List<string>();
        categoryIndices = new Dictionary<string, int>();

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

                string cat = fields[3];
                if (!categoriesToVariants.ContainsKey(cat))
                {
                    categoriesToVariants.Add(cat, new List<CPD_Variant>());
                    categories.Add(cat);
                    categoryIndices.Add(cat, categories.Count - 1);
                }

                CPD_Variant variant = new CPD_Variant(
                    cpdType,
                    categoriesToVariants[cat].Count,
                    variants.Count,
                    new CPD_CritVal_Filepath(spritesPath + fields[0]),
                    fields[1],
                    cat,
                    p
                );

                variants.Add(variant);
                categoriesToVariants[cat].Add(variant);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD FileName " + cpdType + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        // Lastly, if we're giving all elements equal probability, do so here.
        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i].probability == -1)
            {
                variants[i].probability = probX;
            }
        }

        return variants;
    }
}

public class CPD_Color : CPD
{
    // Given the path of this CPD's properties file and where its sprites are stored, we can initialize all variants.
    public CPD_Color(CPD_Type cat, bool constrainable, string propertiesPath)
    {
        this.constrainable = constrainable;
        this.cpdType = cat;
        this.propertiesPath = propertiesPath;
        initialize();
    }

    public override List<CPD_Variant> initialize()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath);
        string[] lines = txt.text.Split('\n');

        variants = new List<CPD_Variant>();
        categoriesToVariants = new Dictionary<string, List<CPD_Variant>>();
        categoryIndices = new Dictionary<string, int>();
        categories = new List<string>();

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

                string cat = fields[3];
                if (!categoriesToVariants.ContainsKey(cat))
                {
                    categoriesToVariants.Add(cat, new List<CPD_Variant>());
                    categories.Add(cat);
                    categoryIndices.Add(cat, categories.Count - 1);
                }

                CPD_Variant variant = new CPD_Variant(
                    cpdType,
                    categoriesToVariants[cat].Count,
                    variants.Count,
                    new CPD_CritVal_Color(col),
                    fields[0],
                    cat,
                    p
                );

                variants.Add(variant);
                categoriesToVariants[cat].Add(variant);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD Color " + cpdType + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i].probability == -1)
            {
                variants[i].probability = probX;
            }
        }

        return variants;
    }
}

public class CPD_Number : CPD
{
    public override List<CPD_Variant> initialize()
    {
        throw new System.NotImplementedException();
    }
}



/// <summary>
/// Defines a specific variant of a CPD
/// For example, the HairStyle CPD might have a "normal1" and "normal2" variant, both of which fall under the "normal" category
/// </summary>
public class CPD_Variant
{
    public CPD_Type cpdType;
    public int categoryID; // order within category.
    public int cpdID; // order within CPD.
    public CPD_CriticalValue critVal; // the actual "thing" stored in this CPD (filepath? color? number? etc...)
    public string name;
    public string category;
    public float probability;

    public CPD_Variant(CPD_Type cpdCat, int catID, int cpdID, CPD_CriticalValue critVal, string name, string cat, float prob)
    {
        this.cpdType = cpdCat;
        this.categoryID = catID;
        this.cpdID = cpdID;
        this.critVal = critVal;
        this.name = name;
        this.category = cat;
        this.probability = prob;
    }

    // All CPD fields print out their name.
    public override string ToString()
    {
        return this.name;
    }
}

public abstract class CPD_CriticalValue
{

}

public class CPD_CritVal_Filepath : CPD_CriticalValue
{
    public string filepath;

    public CPD_CritVal_Filepath(string f)
    {
        filepath = f;
    }

    public Sprite getSprite()
    {
        return Resources.Load<Sprite>(filepath);
    }
}

public class CPD_CritVal_Color : CPD_CriticalValue
{
    public Color col;

    public CPD_CritVal_Color(Color c)
    {
        col = c;
    }
}

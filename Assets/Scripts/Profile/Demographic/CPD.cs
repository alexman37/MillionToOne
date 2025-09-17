using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Character Profile Demographic." A single feature that a character has, such as their hair, hair color, or skin tone.
/// A complete character is made up of multiple of these.
/// </summary>
public abstract class CPD
{
    public CPD_Type cpdType;          // Check the enum below.
    protected string propertiesPath;  // Each CPD has a text file that outlines the data of all its variants - this is the path to it
    public bool constrainable;        // Whether or not this CPD is constrainable, e.g. sortable, part of the game.

    protected float probCounter = 0.0f;
    protected float probX = 0.0f;

    public List<CPD_Variant> variants;                // All variants for this CPD in the order they apepared
    public List<string> categories;                   // All categories for this CPD in the order they appeared
    public Dictionary<string, int> categoryIndices;   // All categories matched up with their position in cats list (faster than FindIndex)
    protected Dictionary<string, List<CPD_Variant>> categoriesToVariants;  // All variants associated with a particular category

    public abstract List<CPD_Variant> initialize();

    /// <summary>
    /// Returns a completely random CPD variant
    /// </summary>
    public CPD_Variant getRandom()
    {
        return variants[Random.Range(0, variants.Count)];
    }

    /// <summary>
    /// Returns a completely random CPD variant index
    /// </summary>
    public int getRandomIndex()
    {
        return Random.Range(0, variants.Count);
    }

    /// <summary>
    /// Return a random variant with respect to constrained categories
    /// </summary>
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

    /// <summary>
    /// Return a random variant index with respect to constrained categories
    /// </summary>
    public (int catId, int varId) getRandomConstrainedIndex(HashSet<string> restrictedCats)
    {
        CPD_Variant chosen = getRandomConstrained(restrictedCats);
        return (categoryIndices[chosen.category], chosen.cpdID);
    }

    /// <summary>
    /// Get all possible category indicies with respect to whichever categories are constrained
    /// </summary>
    public List<int> getAllConstrainedIndicies(HashSet<string> restrictedCats)
    {
        List<int> cats = new List<int>();
        foreach(string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                cats.Add(categoryIndices[cat]);
            }
        }
        return cats;
    }

    /// <summary>
    /// Get all possible variants from a category
    /// </summary>
    public List<CPD_Variant> getPossibleVariantsFromCategory(string cat)
    {
        return categoriesToVariants[cat];
    }

    /// <summary>
    /// Get all variants from whatever categories are possible (given a set of constraints)
    /// </summary>
    public List<CPD_Variant> getConstrainedCategoryVariants(HashSet<string> restrictedCats)
    {
        List<CPD_Variant> allPossible = new List<CPD_Variant>();
        foreach (string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                allPossible.AddRange(getPossibleVariantsFromCategory(cat));
            }
        }
        return allPossible;
    }

    /// <summary>
    /// What percentage of categories are still available? (Used for fast roster size recalculations)
    /// </summary>
    public float getProportionOfCategories(HashSet<string> restrictedCats)
    {
        return ((float)(categories.Count - restrictedCats.Count) / (float)categories.Count);
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


/// <summary>
/// A CPD whose critical value is a path to some file to load.
/// </summary>
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

/// <summary>
/// A CPD whose critical value is a color
/// </summary>
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
                string[] strColors = fields[2].Split(',');
                CPD_ColorType critVal;

                // Color range
                if (fields[2].Contains("-"))
                {
                    string[] rs = strColors[0].Split('-');
                    int rMin = int.Parse(rs[0]); int rMax = rs.Length > 1 ? int.Parse(rs[1]) : int.Parse(rs[0]);
                    string[] gs = strColors[1].Split('-');
                    int gMin, gMax;
                    if (gs[0] == "X0") { gMin = rMin; gMax = rMin; }
                    else { gMin = int.Parse(gs[0]); gMax = gs.Length > 1 ? int.Parse(gs[1]) : int.Parse(gs[0]); }
                    string[] bs = strColors[2].Split('-');
                    int bMin, bMax;
                    if (bs[0] == "X0") { bMin = rMin; bMax = rMin; }
                    else if (bs[0] == "X1") { bMin = gMin; bMax = gMin; }
                    else { bMin = int.Parse(bs[0]); bMax = bs.Length > 1 ? int.Parse(bs[1]) : int.Parse(bs[0]); }
                    critVal = new ColorRange(rMin, rMax, gMin, gMax, bMin, bMax);
                } 
                // Single color value
                else
                {
                    critVal = new ConstantColor(new Color(float.Parse(strColors[0]) / 255f, float.Parse(strColors[1]) / 255f, float.Parse(strColors[2]) / 255f));
                }
                /*for (int s = 0; s < strColors.Length; s++)
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
                Color col = new Color(colors[0], colors[1], colors[2]);*/

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
                    new CPD_CritVal_Color(critVal),
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
    public CPD_ColorType col;

    public CPD_CritVal_Color(Color c)
    {
        col = new ConstantColor(c);
    }

    public CPD_CritVal_Color(CPD_ColorType c)
    {
        col = c;
    }
}

public abstract class CPD_ColorType
{
    public abstract Color getColor(int simId);
}

public class ConstantColor : CPD_ColorType
{
    private Color col;

    public ConstantColor(Color co)
    {
        col = co;
    }
    public override Color getColor(int simId)
    {
        return col;
    }
}

// Color Range
public class ColorRange : CPD_ColorType
{
    // all color values from 0-255 (for now)
    private int minR;
    private int maxR;
    private int minG;
    private int maxG;
    private int minB;
    private int maxB;

    public ColorRange(int minR, int maxR, int minG, int maxG, int minB, int maxB)
    {
        this.minR = minR;
        this.maxR = maxR;
        this.minG = minG;
        this.maxG = maxG;
        this.minB = minB;
        this.maxB = maxB;
    }

    public override Color getColor(int simId)
    {
        Random.InitState(simId);
        return new Color(
            (float)Random.Range(minR, maxR) / 255f,
            (float)Random.Range(minG, maxG) / 255f,
            (float)Random.Range(minB, maxB) / 255f,
        1);
    }
}

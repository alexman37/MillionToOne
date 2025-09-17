using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything to do with generating the portraits for characters.
/// </summary>
public static class CharSpriteGen
{
    public delegate Color ReplacementFunc(Color prior, Color observed, Color[] target);

    private static Color skinColor = new Color(1, 0, 0, 1);
    private static Color skinColorBorder = new Color(0.6f, 0, 0, 1);
    private static Color hairColor = new Color(0, 0, 1, 1);
    private static Color hairColorBorder = new Color(0, 0, 0.6f, 1);
    private static Color hairColorShaded = new Color(0, 0, 0.2f, 1);
    private static Color bodyColor = new Color(0, 1, 0, 1);
    private static Color bodyColorBorder = new Color(0, 0.6f, 0, 1);


    /// <summary>
    /// Given a character, draw their portrait.
    /// This entails creating several layers in a scripted pattern, applying CPD's in some order.
    /// </summary>
    /// <returns></returns>
    public static Sprite genSpriteFromLayers(Character ch)
    {
        //SpriteGenLayer clean = new SpriteGenLayer(ch.background.getSprite(), Color.green, ch.skinTone.color);
        List<Color> bodyColsReplace = getColorsOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone), ch.simulatedId);
        bodyColsReplace.AddRange(getColorsOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone), ch.simulatedId));
        SpriteGenLayer body = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.BodyType, ch.getCpdIDofCharacteristic(CPD_Type.BodyType)),
            bodyReplacement);
        SpriteGenLayer head = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HeadType, ch.getCpdIDofCharacteristic(CPD_Type.HeadType)),
            headReplacement);
        SpriteGenLayer hair = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HairStyle, ch.getCpdIDofCharacteristic(CPD_Type.HairStyle)),
            hairReplacement);
        SpriteGenLayer face = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.Face, ch.getCpdIDofCharacteristic(CPD_Type.Face)), 
            defaultReplacement);

        SpriteGenLayer[] newLayers = { body, head, hair, face };
        // TODO ???
        Color[][] cols = { new Color[]{ ch.getColorField(CPD_Type.SkinTone), new Color(Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f)) },
            new Color[]{ ch.getColorField(CPD_Type.SkinTone) },
            new Color[]{ ch.getColorField(CPD_Type.HairColor) },
            new Color[]{ Color.black } };

        Texture2D newTex = new Texture2D(64, 64);
        newTex = prepareCanvas(newTex);
        newTex = addLayers(newTex, newLayers, cols);
        newTex.filterMode = FilterMode.Point;
        return Sprite.Create(newTex, new Rect(0, 0, 64, 64), new Vector2(0f, 0f), 2);
    }

    /// <summary>
    /// Add several layers to the given texture
    /// </summary>
    public static Texture2D addLayers(Texture2D oldLayer, SpriteGenLayer[] newLayers, Color[][] replacements)
    {
        for (int i = 0; i < newLayers.Length; i++)
        {
            oldLayer = addLayer(oldLayer, newLayers[i], replacements[i]);
        }

        return oldLayer;
    }

    /// <summary>
    /// Add a single layer to a given texture
    /// TODO: Do we need to handle slight transparency?
    /// </summary>
    public static Texture2D addLayer(Texture2D oldLayer, SpriteGenLayer sgl, Color[] target)
    {
        Debug.Log(sgl);
        int w = sgl.layer.texture.width;
        int h = sgl.layer.texture.height;
        for (int x = 0; x < oldLayer.width; x++)
        {
            for (int y = 0; y < oldLayer.height; y++)
            {
                Color newcol = sgl.layer.texture.GetPixel(x, y);
                if (newcol.a == 0 || x >= w || y >= h) ;//do nothing;
                // TODO do we wanna handle slight transparency here?
                else
                {
                    Color prior = oldLayer.GetPixel(x, y);
                    oldLayer.SetPixel(x, y, sgl.recolor(prior, newcol, target));
                }
            }
        }

        oldLayer.Apply();
        return oldLayer;
    }

    /// <summary>
    /// Get the sprite of a CPD variant, assuming it is indeed a FilePath type variant
    /// </summary>
    private static Sprite getSpriteOfCPD(CPD_Type cpdT, int varIndex)
    {
        return (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Filepath).getSprite();
    }

    /// <summary>
    /// Get the color of a CPD variant, assuming it is indeed a Color type variant
    /// </summary>
    private static Color getColorOfCPD(CPD_Type cpdT, int varIndex, int simId)
    {
        return (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Color).col.getColor(simId);
    }

    /// <summary>
    /// Get Colors for CPD variant in this order: normal, border, shaded
    /// </summary>
    private static List<Color> getColorsOfCPD(CPD_Type cpdT, int varIndex, int simId)
    {
        Color workWith = (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Color).col.getColor(simId);
        return new List<Color> {
            workWith,
            new Color(workWith.r * 0.6f, workWith.g * 0.6f, workWith.b * 0.6f, workWith.a),
            //new Color(workWith.r * 0.2f, workWith.g * 0.2f, workWith.b * 0.2f, workWith.a),
        };
    }

    private static Texture2D prepareCanvas(Texture2D canvas)
    {
        for (int x = 0; x < canvas.width; x++)
        {
            for (int y = 0; y < canvas.height; y++)
            {
                canvas.SetPixel(x, y, Color.clear);
            }
        }

        canvas.Apply();
        return canvas;
    }




    /// REPLACEMENT FUNCTIONS
    private static Color defaultReplacement(Color prior, Color observed, Color[] _)
    {
        /*Color observed = oldLayer.GetPixel(x, y);
        oldLayer.SetPixel(x, y, observed);*/
        return observed;
    }

    private static Color headReplacement(Color prior, Color observed, Color[] target)
    {
        Color targ = target[0];
        if (observed == skinColor) return targ;
        else if (observed == skinColorBorder) return new Color(targ.r * 0.6f, targ.g * 0.6f, targ.b * 0.6f);
        else return observed;
    }

    private static Color bodyReplacement(Color prior, Color observed, Color[] target)
    {
        Color skin = target[0];
        Color clothes = target[1];
        if (observed == bodyColor) return clothes;
        else if (observed == bodyColorBorder) return new Color(clothes.r * 0.6f, clothes.g * 0.6f, clothes.b * 0.6f);
        if (observed == skinColor) return skin;
        else if (observed == skinColorBorder) return new Color(skin.r * 0.6f, skin.g * 0.6f, skin.b * 0.6f); // TODO second color
        else return observed;
    }

    private static Color hairReplacement(Color prior, Color observed, Color[] target)
    {
        Color targ = target[0];
        if (observed == hairColor) return targ;
        else if (observed == hairColorBorder) return new Color(targ.r * 0.6f, targ.g * 0.6f, targ.b * 0.6f);
        else if (observed == hairColorShaded)
        {
            if (prior.a == 0)
            {
                return new Color(targ.r * 0.2f, targ.g * 0.2f, targ.b * 0.2f);
            }
            else return prior;
        }
        else return observed;
    }
}



/// <summary>
/// When creating a character portrait, we do so in layers.
/// In other words, draw CPDs in a structured way, starting at the bottom and moving up from there
/// SpriteGenLayer defines a sprite image to use and draw on top of others, as well as
///     "colsToWatch", the list of colors to look for in the image to replace
///     "colsToFill", the list of colors to replace watched colors with.
/// Goes without saying mapping between colsToWatch -> colsToFill should be 1:1
/// </summary>
public class SpriteGenLayer {
    public Sprite layer;
    public CharSpriteGen.ReplacementFunc recolor;

    public SpriteGenLayer(Sprite lay, CharSpriteGen.ReplacementFunc rep)
    {
        this.layer = lay;
        this.recolor = rep;
    }

    public override string ToString()
    {
        return "SGL: " + layer;
    }
}

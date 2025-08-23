using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything to do with generating the portraits for characters.
/// </summary>
public static class CharSpriteGen
{

    /// <summary>
    /// Given a character, draw their portrait.
    /// This entails creating several layers in a scripted pattern, applying CPD's in some order.
    /// </summary>
    /// <returns></returns>
    public static Sprite genSpriteFromLayers(Character ch)
    {
        //SpriteGenLayer clean = new SpriteGenLayer(ch.background.getSprite(), Color.green, ch.skinTone.color);
        SpriteGenLayer body = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.BodyType, ch.getCpdIDofCharacteristic(CPD_Type.BodyType)),
            Color.green,
            getColorOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone)));
        SpriteGenLayer head = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HeadType, ch.getCpdIDofCharacteristic(CPD_Type.HeadType)),
            Color.red,
            getColorOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone)));
        SpriteGenLayer hair = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HairStyle, ch.getCpdIDofCharacteristic(CPD_Type.HairStyle)),
            Color.blue,
            getColorOfCPD(CPD_Type.HairColor, ch.getCpdIDofCharacteristic(CPD_Type.HairColor)));
        SpriteGenLayer face = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.Face, ch.getCpdIDofCharacteristic(CPD_Type.Face)));

        SpriteGenLayer[] newLayers = { body, head, hair, face };

        Texture2D newTex = new Texture2D(32, 32);
        newTex = addLayers(newTex, newLayers);
        newTex.filterMode = FilterMode.Point;
        return Sprite.Create(newTex, new Rect(0, 0, 32, 32), new Vector2(0f, 0f), 2);
    }

    /// <summary>
    /// Add several layers to the given texture
    /// </summary>
    public static Texture2D addLayers(Texture2D oldLayer, SpriteGenLayer[] newLayers)
    {
        for (int i = 0; i < newLayers.Length; i++)
        {
            oldLayer = addLayer(oldLayer, newLayers[i]);
        }

        return oldLayer;
    }

    /// <summary>
    /// Add a single layer to a given texture
    /// TODO: Do we need to handle slight transparency?
    /// </summary>
    public static Texture2D addLayer(Texture2D oldLayer, SpriteGenLayer sgl)
    {
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
                    if(sgl.colsToFill != null) {
                        int index = sgl.colsToWatch.IndexOf(newcol);
                        if (index >= 0) oldLayer.SetPixel(x, y, sgl.colsToFill[index]);
                        else oldLayer.SetPixel(x, y, newcol);
                    }
                    else oldLayer.SetPixel(x, y, newcol);
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
    private static Color getColorOfCPD(CPD_Type cpdT, int varIndex)
    {
        return (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Color).col;
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
    public List<Color> colsToWatch;
    public List<Color> colsToFill;

    public SpriteGenLayer(Sprite lay, Color colToWatch, Color colToFill)
    {
        this.layer = lay;
        this.colsToWatch = new List<Color> { colToWatch };
        this.colsToFill = new List<Color> { colToFill };
    }

    public SpriteGenLayer(Sprite lay, List<Color> colsToWatch, List<Color> colsToFill)
    {
        this.layer = lay;
        this.colsToWatch = colsToWatch;
        this.colsToFill = colsToFill;
    }

    public SpriteGenLayer(Sprite lay)
    {
        this.layer = lay;
        this.colsToWatch = null;
        this.colsToFill = null;
    }

    public override string ToString()
    {
        return "SGL: " + layer;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CharSpriteGen
{

    public static Sprite genSpriteFromLayers(Character ch)
    {
        //SpriteGenLayer clean = new SpriteGenLayer(ch.background.getSprite(), oneList(Color.green), oneList(ch.skinTone.color));
        SpriteGenLayer body = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.BodyType, ch.getCpdIDofCharacteristic(CPD_Type.BodyType)),
            oneList(Color.green),
            oneList(getColorOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone))));
        SpriteGenLayer head = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HeadType, ch.getCpdIDofCharacteristic(CPD_Type.HeadType)),
            oneList(Color.red),
            oneList(getColorOfCPD(CPD_Type.SkinTone, ch.getCpdIDofCharacteristic(CPD_Type.SkinTone))));
        SpriteGenLayer hair = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.HairStyle, ch.getCpdIDofCharacteristic(CPD_Type.HairStyle)),
            oneList(Color.blue),
            oneList(getColorOfCPD(CPD_Type.HairColor, ch.getCpdIDofCharacteristic(CPD_Type.HairColor))));
        SpriteGenLayer face = new SpriteGenLayer(
            getSpriteOfCPD(CPD_Type.Face, ch.getCpdIDofCharacteristic(CPD_Type.Face)));

        SpriteGenLayer[] newLayers = { body, head, hair, face };

        Texture2D newTex = new Texture2D(32, 32);
        newTex = addLayers(newTex, newLayers);
        newTex.filterMode = FilterMode.Point;
        return Sprite.Create(newTex, new Rect(0, 0, 32, 32), new Vector2(0f, 0f), 2);
    }

    public static Texture2D addLayers(Texture2D oldLayer, SpriteGenLayer[] newLayers)
    {
        for (int i = 0; i < newLayers.Length; i++)
        {
            oldLayer = addLayer(oldLayer, newLayers[i]);
        }

        return oldLayer;
    }

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

    static List<Color> oneList(Color c)
    {
        List<Color> cols = new List<Color>();
        cols.Add(c);
        return cols;
    }

    private static Sprite getSpriteOfCPD(CPD_Type cpdT, int varIndex)
    {
        return (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Filepath).getSprite();
    }

    private static Color getColorOfCPD(CPD_Type cpdT, int varIndex)
    {
        return (Roster.cpdByType[cpdT].variants[varIndex].critVal as CPD_CritVal_Color).col;
    }
}



public class SpriteGenLayer {
    public Sprite layer;
    public List<Color> colsToWatch;
    public List<Color> colsToFill;


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

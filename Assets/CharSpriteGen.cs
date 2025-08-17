using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CharSpriteGen
{

    public static Sprite genSpriteFromLayers(Character ch)
    {
        //SpriteGenLayer clean = new SpriteGenLayer(ch.background.getSprite(), oneList(Color.green), oneList(ch.skinTone.color));
        SpriteGenLayer body = new SpriteGenLayer(ch.bodyType.getSprite(), oneList(Color.green), oneList(ch.skinTone.color));
        SpriteGenLayer head = new SpriteGenLayer(ch.headType.getSprite(), oneList(Color.red), oneList(ch.skinTone.color));
        SpriteGenLayer hair = new SpriteGenLayer(ch.hairStyle.getSprite(), oneList(Color.blue), oneList(ch.hairColor.color));
        SpriteGenLayer face = new SpriteGenLayer(ch.face.getSprite());

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

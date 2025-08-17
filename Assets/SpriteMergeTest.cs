using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteMergeTest : MonoBehaviour
{
    public Image img;
    public Sprite cleanslate;
    public Sprite hair;
    public Sprite body;
    public Sprite head;
    public Sprite face;

    // Start is called before the first frame update
    void Start()
    {
        
        var newTex = new Texture2D(img.sprite.texture.width, img.sprite.texture.height);
        Sprite[] layersInOrder = { cleanslate, body, head, hair, face };

        //Texture2D result = addLayers(img.sprite.texture, layersInOrder);
        Texture2D result = CharSpriteGen.addLayer(img.sprite.texture, new SpriteGenLayer(cleanslate));
        result = CharSpriteGen.addLayer(result, new SpriteGenLayer(body, oneList(Color.green), oneList(Color.white)));
        result = CharSpriteGen.addLayer(result, new SpriteGenLayer(head, oneList(Color.red), oneList(Color.white)));
        result = CharSpriteGen.addLayer(result, new SpriteGenLayer(hair, oneList(Color.blue), oneList(Color.white)));
        result = CharSpriteGen.addLayer(result, new SpriteGenLayer(face));
        Sprite fin = Sprite.Create(result, new Rect(0, 0, 32, 32), new Vector2(0f, 0f));
        img.sprite = fin;
        Debug.Log("test complete");
    }

    List<Color> oneList(Color c)
    {
        List<Color> cols = new List<Color>();
        cols.Add(c);
        return cols;
    }
}

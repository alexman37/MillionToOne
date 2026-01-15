using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Deprecated class for testing sprite generation. Do not use.
/// </summary>
public class CharSpriteTester : MonoBehaviour
{
    public SpriteRenderer charachterImgTemplate;
    public Shader colorSwapper;
    private GameObject currImg;

    private Character cTest;

    // Start is called before the first frame update
    void Start()
    {
        //Character cTest = new Character(0, 0);
        //cp.randomizeDemographics(false, false, false);
        //Debug.Log(cp.hairColor);
        //getNewRandomSprite(cp.skinTone, cp.hairColor);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            cTest = new Character(0, 0);
            cTest.randomizeDemographics();
            getNewRandomSprite();
        }

        //name testing
        if (Input.GetKeyDown(KeyCode.E))
        {
            for(int i = 0; i < 5; i++) {
                (string f, string l) name = CharRandomValue.randomName(true);
                Debug.Log(name.f + " " + name.l);

                name = CharRandomValue.randomName(false);
                Debug.Log(name.f + " " + name.l);
            }
        }
    }

    public void getNewRandomSprite()
    {
        Destroy(currImg);

        currImg = Instantiate(charachterImgTemplate.gameObject, this.transform, true);
        currImg.transform.position = new Vector3(100, 100);

        int randHeightOffset = Random.Range(6, 24);

        //create: head (w/ skin), hair (w/ color), face
        //charachterImgTemplate.material.SetColor("_NewSkinColor", st.color);
        //charachterImgTemplate.material.SetColor("_NewHairColor", hc.color);
        addNewLayerToPortrait("Body");
        addNewLayerToPortrait("Head", randHeightOffset);
        addNewLayerToPortrait("Face", randHeightOffset);
        addNewLayerToPortrait("Hair", randHeightOffset);
    }

    private float rfc()
    {
        return Random.Range(0, 1);
    }

    private void addNewLayerToPortrait(string path)
    {
        SpriteRenderer img = genImage(path, 0);

        Color c = img.color;
        c.a = 1.0f;
        img.color = c;
    }

    private void addNewLayerToPortrait(string path, string shaderVar, Color col)
    {
        SpriteRenderer img = genImage(path, 0);

        img.material.SetColor(shaderVar, col);
        Color c = img.color;
        c.a = 1.0f;
        img.color = c;
    }

    private void addNewLayerToPortrait(string path, int verticalOffset)
    {
        SpriteRenderer img = genImage(path, verticalOffset);

        Color c = img.color;
        c.a = 1.0f;
        img.color = c;
    }

    private SpriteRenderer genImage(string path, int offset)
    {
        List<string> allFiles = getAllFilesInDir("Assets/CharSprites/Resources/CharSprites/" + path);

        Sprite randSpr = Resources.Load<Sprite>("CharSprites/" + path + "/" + allFiles[Mathf.FloorToInt(Random.Range(0, allFiles.Count))].Split('.')[0]);
        //Sprite headSpr = Resources.Load<Sprite>("CharSprites/Head/" + "normal");
        SpriteRenderer Img = Instantiate(charachterImgTemplate, currImg.transform, true);
        Img.sprite = randSpr;
        Img.gameObject.GetComponent<RectTransform>().position = new Vector3(100, 100 + offset);

        return Img;
    }

    private List<string> getAllFilesInDir(string path)
    {
        DirectoryInfo thePath = new DirectoryInfo(path);
        FileInfo[] fileInfo = thePath.GetFiles("*.png", SearchOption.AllDirectories);
        List<string> paths = new List<string>();

        foreach (FileInfo file in fileInfo)
        {
            paths.Add(file.Name);
        }

        return paths;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosterForm : MonoBehaviour
{
    public static RosterForm instance;

    public GameObject formButtonGroupTemplate;
    string[] formFields = new string[] { "CPD_Hair", "CPD_HairColor", "CPD_SkinTone", "CPD_BodyType" };

    private float nextFormGroupOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        createFormButtonGroups();
    }

    // Constant function...
    public void createFormButtonGroups()
    {
        foreach(string s in formFields)
        {
            createWithParameters(s, Roster.rosterDMap.uniqueDescriptions[s]);
        }
    }

    private void createWithParameters(string group, HashSet<string> values)
    {
        GameObject next = GameObject.Instantiate(formButtonGroupTemplate, this.transform);
        next.name = group;

        FormButtonGroup formGroup = next.GetComponent<FormButtonGroup>();

        nextFormGroupOffset += formGroup.buildFormButtonGroup(group, values, -nextFormGroupOffset) + 10;
    }
}

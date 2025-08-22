using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosterForm : MonoBehaviour
{
    public static RosterForm instance;

    public GameObject formButtonGroupTemplate;
    CPD[] formFields;

    private float nextFormGroupOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        formFields = Roster.cpdConstrainables.ToArray();
        createFormButtonGroups();
    }

    // Constant function...
    public void createFormButtonGroups()
    {
        foreach(CPD cpd in formFields)
        {
            createWithParameters(cpd.cpdType, cpd.categories);
        }
    }

    private void createWithParameters(CPD_Type cpdType, List<string> cpdCategories)
    {
        GameObject next = GameObject.Instantiate(formButtonGroupTemplate, this.transform);
        next.name = cpdType.ToString();

        FormButtonGroup formGroup = next.GetComponent<FormButtonGroup>();

        nextFormGroupOffset += formGroup.buildFormButtonGroup(cpdType, cpdCategories, -nextFormGroupOffset) + 10;
    }
}

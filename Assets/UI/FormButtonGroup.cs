using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A FormButtonGroup is created for each constrainable CPD in the game.
/// It's made up of FormButtons which all need to communicate with each other in some situations.
/// </summary>
public class FormButtonGroup : MonoBehaviour
{
    public CPD_Type cpdType;
    public Image img;

    public List<FormButton> formButtons;

    // TEMPLATE
    public GameObject formObjectComponent;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    /// <summary>
    /// Create all form objects from the template.
    /// </summary>
    public float buildFormButtonGroup(CPD_Type cpdType, IEnumerable buttonInstructions, float offset)
    {
        this.cpdType = cpdType;
        formButtons = new List<FormButton>();

        int count = 0;
        float standardHeight = 0;
        foreach (string cat in buttonInstructions)
        {
            GameObject next = GameObject.Instantiate(formObjectComponent, transform);
            next.SetActive(true);
            RectTransform rt = next.GetComponent<RectTransform>();
            standardHeight = rt.rect.height;
            rt.anchoredPosition += new Vector2(0, count * -standardHeight);

            next.name = cat;

            FormButton formButton = next.GetComponent<FormButton>();
            formButton.cpdType = cpdType;
            formButton.category = cat;
            formButton.title.text = cat;

            formButton.partOfGroup = this;
            formButtons.Add(formButton);

            count++;
        }

        GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offset);

        return count * standardHeight;
    }
}

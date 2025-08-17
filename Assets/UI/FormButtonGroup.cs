using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormButtonGroup : MonoBehaviour
{
    public string onField;
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
    public float buildFormButtonGroup(string onField, HashSet<string> buttonInstructions, float offset)
    {
        this.onField = onField;
        formButtons = new List<FormButton>();

        int count = 0;
        float standardHeight = 0;
        foreach (string genDesc in buttonInstructions)
        {
            GameObject next = GameObject.Instantiate(formObjectComponent, transform);
            next.SetActive(true);
            RectTransform rt = next.GetComponent<RectTransform>();
            standardHeight = rt.rect.height;
            rt.anchoredPosition += new Vector2(0, count * -standardHeight);

            next.name = genDesc;

            FormButton formButton = next.GetComponent<FormButton>();
            formButton.onField = onField;
            formButton.value = genDesc;
            formButton.title.text = genDesc;

            formButton.partOfGroup = this;
            formButtons.Add(formButton);

            count++;
        }

        GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offset);

        return count * standardHeight;
    }
}

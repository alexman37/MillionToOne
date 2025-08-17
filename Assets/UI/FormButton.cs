using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class FormButton : MonoBehaviour
{
    FormButtonState state;
    public string onField;
    public string value;
    Image img1;
    Image img2;

    public Button noButton;
    public Button yesButton;
    public TextMeshProUGUI title;

    private bool acceptingInput = true;
    private bool noTicked = false;
    private bool yesTicked = false;

    public FormButtonGroup partOfGroup;

    // Update a particular field with some new constraints. FormButtonState determines if you add or remove them.
    public static event Action<string, string, FormButtonState> updatedConstraint;
    // Re-initialize the list of constraints when un-confirming an option. Use pre-existing "no" values.
    public static event Action<string, List<string>> reinitializeConstraints;

    // Start is called before the first frame update
    void Start()
    {
        state = FormButtonState.Unknown;
        img1 = noButton.gameObject.GetComponent<Image>();
        img2 = yesButton.gameObject.GetComponent<Image>();

        updatedConstraint += (_,__,f) => { };
    }

    public void toggleYesButton()
    {
        if(acceptingInput)
        {
            yesTicked = !yesTicked;
            if (yesTicked)
            {
                state = FormButtonState.Confirmed;
                buttonInGroupConfirmed(onField, value, true);
            }
            else
            {
                if (noTicked)
                {
                    state = FormButtonState.Eliminated;
                }
                else
                {
                    state = FormButtonState.Unknown;
                }
                buttonInGroupConfirmed(onField, value, false);
            }

            updateConstraintForButton();
        }
    }

    public void toggleNoButton()
    {
        if(acceptingInput)
        {
            noTicked = !noTicked;
            if (noTicked && state != FormButtonState.Confirmed)
            {
                state = FormButtonState.Eliminated;
                updateConstraintForButton();
            }
            else if (!noTicked)
            {
                state = FormButtonState.Unknown;
                updateConstraintForButton();
            }
        }
    }

    private void updateConstraintForButton()
    {
        // Adjust draws
        if(state == FormButtonState.Confirmed)
        {
            img1.color = Color.white;
            img2.color = Color.green;
        } else if (state == FormButtonState.Eliminated)
        {
            img2.color = Color.white;
            img1.color = Color.red;
        } else
        {
            img1.color = Color.white;
            img2.color = Color.white;
        }

        updatedConstraint.Invoke(onField, value, state);
    }

    /// <summary>
    /// Make sure every other button is either set to NO or 
    /// </summary>
    private void buttonInGroupConfirmed(string group, string buttonValue, bool wasConfirmed)
    {
        List<FormButton> buttonsToToggle = partOfGroup.formButtons;
        Debug.Log("There are " + partOfGroup.formButtons.Count + " buttons in this group");
        List<string> buttonsAreOff = new List<string>();

        foreach(FormButton b in buttonsToToggle)
        {
            if(b.value != buttonValue)
            {
                b.acceptingInput = !wasConfirmed;
                b.img1.color = wasConfirmed ? Color.gray : (b.noTicked ? Color.red : Color.white);
                b.state = b.noTicked ? FormButtonState.Eliminated : FormButtonState.Unknown;
            } else
            {
                b.img1.color = wasConfirmed ? Color.gray : (b.noTicked ? Color.red : Color.white);
            }

            if(b.noTicked) buttonsAreOff.Add(b.value);
        }
        if(!wasConfirmed)
        {
            reinitializeConstraints.Invoke(group, buttonsAreOff);
        }
    }
}

public enum FormButtonState
{
    Unknown,
    Eliminated,
    Confirmed
}
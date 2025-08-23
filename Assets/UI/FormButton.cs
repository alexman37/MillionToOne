using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A FormButton is created for each category of a single CPD.
/// The FormButton has a "yes" button for guessing that the target has this category, and we want to use only it.
/// It also has a "no" button for ruling out this category completely.
/// There are 3 general states for a form button to be in: Eliminated (Ruled out), Confirmed, or Unknown.
/// </summary>
public class FormButton : MonoBehaviour
{
    FormButtonState state;
    public CPD_Type cpdType; // which CPD is this Formbutton acting on?
    public string category; // which category is this FormButton tracking for the given CPD?

    // TODO replace with fancier sprites
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
    public static event Action<CPD_Type, string, FormButtonState> updatedConstraint;
    // Re-initialize the list of constraints when un-confirming an option. Use pre-existing "no" values.
    public static event Action<CPD_Type, List<string>> reinitializeConstraints;

    // Start is called before the first frame update
    void Start()
    {
        state = FormButtonState.Unknown;
        img1 = noButton.gameObject.GetComponent<Image>();
        img2 = yesButton.gameObject.GetComponent<Image>();

        updatedConstraint += (_,__,f) => { };
    }

    /// <summary>
    /// When hit YES button, you may either confirm or unconfirm it
    /// </summary>
    public void toggleYesButton()
    {
        if (acceptingInput)
        {
            yesTicked = !yesTicked;
            if (yesTicked)
            {
                state = FormButtonState.Confirmed;
                buttonInGroupConfirmed(true);
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
                buttonInGroupConfirmed(false);
            }

            updateConstraintForButton();
        }
    }

    /// <summary>
    /// When hit no button, you may either eliminate it or make it a possibility again
    /// </summary>
    public void toggleNoButton()
    {
        if(acceptingInput && !yesTicked)
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

    /// <summary>
    /// Given the new state of the FormButton, how should we draw it?
    /// </summary>
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

        updatedConstraint.Invoke(cpdType, category, state);
    }

    /// <summary>
    /// When we confirm a particular category we have to "soft disable" all the other categories.
    /// However we do keep track of whichever ones we'd definitely ruled out before.
    /// That way, if we unconfirm this category, our old eliminations are still in place.
    /// </summary>
    private void buttonInGroupConfirmed(bool wasConfirmed)
    {
        List<FormButton> buttonsToToggle = partOfGroup.formButtons;
        List<string> buttonsAreOff = new List<string>();

        foreach(FormButton b in buttonsToToggle)
        {
            if(b.category != category)
            {
                b.acceptingInput = !wasConfirmed;
                b.img1.color = wasConfirmed ? Color.gray : (b.noTicked ? Color.red : Color.white);
                b.state = b.noTicked ? FormButtonState.Eliminated : FormButtonState.Unknown;
            } else
            {
                b.img1.color = wasConfirmed ? Color.gray : (b.noTicked ? Color.red : Color.white);
            }

            if(b.noTicked) buttonsAreOff.Add(b.category);
        }
        if(!wasConfirmed)
        {
            reinitializeConstraints.Invoke(cpdType, buttonsAreOff);
        }
    }
}

public enum FormButtonState
{
    Unknown,
    Eliminated,
    Confirmed
}
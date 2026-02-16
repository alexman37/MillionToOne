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
    Image imgCol1;
    Image imgCol2;
    Image imgCol3;

    public Button noButton;
    public Button priButton;
    public Button yesButton;
    public TextMeshProUGUI title;

    [SerializeField] private GameObject askAroundMenu;
    [SerializeField] private Button askAroundAsk;
    [SerializeField] private Image askAroundAssessment;
    private bool currentlyAskingAround;

    private bool locked = false; // if true, you can not change this value ever again
    private bool acceptingInput = true; // currently allowed / not allowed to change this value
    private bool noTicked = false; // currently chosen as "no"
    private bool yesTicked = false; // currently chosen as "yes"

    public FormButtonGroup partOfGroup;

    // Update a particular field with some new constraints. FormButtonState determines if you add or remove them.
    public static event Action<CPD_Type, string, FormButtonState> updatedConstraint;
    // Re-initialize the list of constraints when un-confirming an option. Use pre-existing "no" values.
    public static event Action<CPD_Type, List<string>> reinitializeConstraints;

    public static event Action<(CPD_Type, string)> addToAskAroundList = (_) => { };
    public static event Action<(CPD_Type, string)> removeFromAskAroundList = (_) => { };

    // Start is called before the first frame update
    void Start()
    {
        state = FormButtonState.Unknown;
        imgCol1 = noButton.gameObject.GetComponent<Image>();
        imgCol2 = priButton.gameObject.GetComponent<Image>();
        imgCol3 = yesButton.gameObject.GetComponent<Image>();

        updatedConstraint += (_,__,f) => { };
    }

    private void OnEnable()
    {
        PlayerAgent.playerGotCard += updateConstraintFromCard;
        TargetCharGuess.playerGuessesTargetProperty += updateConstraintFromTargetGuess;
        AgentDisplay.selectedAgent += askAroundForAgent;
        AgentDisplay.deselectedAgent += stopAskingAround;
        RosterForm.completedAskAround += stopAskingAround;
    }

    private void OnDisable()
    {
        PlayerAgent.playerGotCard -= updateConstraintFromCard;
        TargetCharGuess.playerGuessesTargetProperty -= updateConstraintFromTargetGuess;
        AgentDisplay.selectedAgent -= askAroundForAgent;
        AgentDisplay.deselectedAgent -= stopAskingAround;
        RosterForm.completedAskAround -= stopAskingAround;
    }

    /// <summary>
    /// When hit YES button, you may either confirm or unconfirm it
    /// </summary>
    public void toggleYesButton()
    {
        if (!locked && acceptingInput)
        {
            yesTicked = !yesTicked;
            if (yesTicked)
            {
                state = FormButtonState.Confirmed;
                buttonInGroupConfirmed(true, false);
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
                buttonInGroupConfirmed(false, false);
            }

            updateConstraintForButton(false);
        }
    }

    /// <summary>
    /// When hit no button, you may either eliminate it or make it a possibility again
    /// </summary>
    public void toggleNoButton()
    {
        if(!locked && acceptingInput && !yesTicked)
        {
            noTicked = !noTicked;
            if (noTicked && state != FormButtonState.Confirmed)
            {
                state = FormButtonState.Eliminated;
                updateConstraintForButton(false);
            }
            else if (!noTicked)
            {
                state = FormButtonState.Unknown;
                updateConstraintForButton(false);
            }
        }
    }

    /// <summary>
    /// Guarantee this constraint is correct
    /// </summary>
    public void confirmThroughCard()
    {
        state = FormButtonState.Confirmed;
        buttonInGroupConfirmed(true, true);
        updateConstraintForButton(true);
        yesTicked = true;
        acceptingInput = false;
        locked = true;
    }

    /// <summary>
    /// Guarantee this constraint is incorrect
    /// </summary>
    public void eliminateThroughCard()
    {
        state = FormButtonState.Eliminated;
        updateConstraintForButton(true);
        noTicked = true;
        acceptingInput = false;
        locked = true;
    }

    /// <summary>
    /// Given the new state of the FormButton, how should we draw it?
    /// </summary>
    private void updateConstraintForButton(bool withCertainty)
    {
        if(!locked)
        {
            // Adjust draws
            if (state == FormButtonState.Confirmed)
            {
                imgCol1.sprite = withCertainty ? RosterForm.instance.getFormSprite("dyi") : RosterForm.instance.getFormSprite("myi");
                imgCol2.sprite = withCertainty ? RosterForm.instance.getFormSprite("dyi") : RosterForm.instance.getFormSprite("myi");
                imgCol3.sprite = withCertainty ? RosterForm.instance.getFormSprite("dy") : RosterForm.instance.getFormSprite("my");
            }
            else if (state == FormButtonState.Eliminated)
            {
                imgCol1.sprite = withCertainty ? RosterForm.instance.getFormSprite("dn") : RosterForm.instance.getFormSprite("mn");
                imgCol2.sprite = withCertainty ? RosterForm.instance.getFormSprite("dni") : RosterForm.instance.getFormSprite("mni");
                imgCol3.sprite = withCertainty ? RosterForm.instance.getFormSprite("dni") : RosterForm.instance.getFormSprite("mni");
            }
            else
            {
                imgCol1.sprite = RosterForm.instance.getFormSprite("x");
                imgCol2.sprite = RosterForm.instance.getFormSprite("x");
                imgCol3.sprite = RosterForm.instance.getFormSprite("x");
            }

            updatedConstraint.Invoke(cpdType, category, state);
        }
    }

    /// <summary>
    /// Update form constraints from receiving a card
    /// </summary>
    private void updateConstraintFromCard(Card card, int _)
    {
        if(card is ClueCard)
        {
            ClueCard cc = card as ClueCard;
            if (this.cpdType == cc.cpdType && this.category == cc.category)
            {
                this.state = cc.onTarget ? FormButtonState.Confirmed : FormButtonState.Eliminated;

                if (this.state == FormButtonState.Confirmed) confirmThroughCard();
                else eliminateThroughCard();
                locked = true;
            }
        }
    }

    /// <summary>
    /// Update form constraints from a target card being guessed on (correctly or not)
    /// </summary>
    private void updateConstraintFromTargetGuess(CPD_Type cpdType, string cat, bool wasCorrect)
    {
        if (this.cpdType == cpdType && this.category == cat)
        {
            // Correct choice: Update everything
            if (wasCorrect) confirmThroughCard();
            else eliminateThroughCard();
            locked = true;
        }
    }

    /// <summary>
    /// When we confirm a particular category we have to "soft disable" all the other categories.
    /// However we do keep track of whichever ones we'd definitely ruled out before.
    /// That way, if we unconfirm this category, our old eliminations are still in place.
    /// </summary>
    private void buttonInGroupConfirmed(bool wasConfirmed, bool withCertainty)
    {
        List<FormButton> buttonsToToggle = partOfGroup.formButtons;
        List<string> buttonsAreOff = new List<string>();

        foreach(FormButton b in buttonsToToggle)
        {
            if(!b.locked)
            {
                if (b.category != category)
                {
                    b.acceptingInput = !wasConfirmed;
                    b.state = b.noTicked ? FormButtonState.Eliminated : FormButtonState.Unknown;

                    b.imgCol3.sprite = b.noTicked ? RosterForm.instance.getFormSprite("mni") :
                    (wasConfirmed ? RosterForm.instance.getFormSprite("myi") : RosterForm.instance.getFormSprite("x"));
                }

                b.imgCol1.sprite = b.noTicked ? RosterForm.instance.getFormSprite("mn") :
                        (wasConfirmed ? RosterForm.instance.getFormSprite("myi") : RosterForm.instance.getFormSprite("x"));
                b.imgCol2.sprite = b.noTicked ? RosterForm.instance.getFormSprite("mni") :
                    (wasConfirmed ? RosterForm.instance.getFormSprite("myi") : RosterForm.instance.getFormSprite("x"));

                if (b.noTicked) buttonsAreOff.Add(b.category);
            }
        }
        if(!wasConfirmed)
        {
            reinitializeConstraints.Invoke(cpdType, buttonsAreOff);
        }
    }



    ///   ASK  AROUND  SUBMENU


    private void askAroundForAgent(int id)
    {
        // TODO get all data on this agent
        askAroundMenu.SetActive(true);
    }

    private void stopAskingAround()
    {
        askAroundMenu.SetActive(false);
        askAroundAsk.image.color = Color.white;
        currentlyAskingAround = false;
    }

    // TODO...do we really have to create a new object for this every time
    private void addToAskAround()
    {
        addToAskAroundList.Invoke((cpdType, category));
        askAroundAsk.image.color = Color.green;
        currentlyAskingAround = true;
    }

    private void removeFromAskAround()
    {
        removeFromAskAroundList.Invoke((cpdType, category));
        askAroundAsk.image.color = Color.white;
        currentlyAskingAround = false;
    }

    public void toggleAskAround()
    {
        if (currentlyAskingAround) removeFromAskAround();
        else addToAskAround();
    }
}

public enum FormButtonState
{
    Unknown,
    Eliminated,
    Confirmed
}
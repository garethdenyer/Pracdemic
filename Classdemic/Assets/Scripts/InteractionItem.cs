using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionItem : MonoBehaviour
{
    //attached to interaction item

    public TMP_Text otherCharacter;
    public TMP_Text type;

    public int charBDBindex;
    public int interactType;
    public int charAdbIndex;
    public int interactionno;
    public bool active;
    public Image activeMarker;

    EditInteractionPanel editintscript;

    public GameObject submitButton;

    private void Start()
    {
        editintscript = FindObjectOfType<EditInteractionPanel>();
    }

    public void PressingSubmit()
    {
        editintscript.EditIntPanel.SetActive(true);

        //pass charBDBindex to the dropdown
        editintscript.SetSelectedCharacterByID(charBDBindex);

        //pass the type to teh dropdown
        editintscript.SetSelectedTypeByIndex(interactType) ;

        //pass all the other variables
        editintscript.intID = interactionno;
        editintscript.dbCharA = charAdbIndex;

        //belt and braces
        editintscript.dbCharB = charBDBindex;
        editintscript.type = interactType;

        editintscript.ConfirmProtagonist();

    }

}

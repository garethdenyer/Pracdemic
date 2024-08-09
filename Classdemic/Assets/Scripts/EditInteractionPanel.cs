using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class EditInteractionPanel : MonoBehaviour
{
    // On the EmptyController

    public int intID;
    public int dbCharA;
    public int dbCharB;
    public int type;

    public TMP_Text protagonist;

    public TMP_Dropdown charBChoice;
    public TMP_Dropdown typeChoice;

    public GameObject EditIntPanel;

    private Dictionary<string, int> charNameToIDMap = new Dictionary<string, int>();

    public void PopulateCharDPDN()
    {
        //this sets everything up and actually could be done after all the characters have been downloaded.

        List<string> sortedCharNames = this.GetComponent<CharacterManager>().characterNames.OrderBy(name => name).ToList();

        charBChoice.ClearOptions();
        charBChoice.AddOptions(sortedCharNames);

        // Create a dictionary to map character names to their corresponding IDs.
        for (int i = 0; i < this.GetComponent<CharacterManager>().characterNames.Count; i++)
        {
            charNameToIDMap[this.GetComponent<CharacterManager>().characterNames[i]] = int.Parse(this.GetComponent<CharacterManager>().characterNos[i]);
        }

        // Add a listener to the dropdown's OnValueChanged event to handle character selection.
        charBChoice.onValueChanged.AddListener(OnCharacterDropdownValueChanged);
    }

    private void OnCharacterDropdownValueChanged(int index)
    {
        string selectedCharacterName = charBChoice.options[index].text;

        // Now you can use the dictionary to get the corresponding ID.
        int selectedCharacterID;
        if (charNameToIDMap.TryGetValue(selectedCharacterName, out selectedCharacterID))
        {
            // Use the selectedCharacterID as needed.
            Debug.Log("Selected Character ID: " + selectedCharacterID);
            dbCharB = selectedCharacterID;
        }
    }

    public void OnTypeDPDNValueChanged (int index)
    {
        type = index;
    }


    // Method to set the selected character based on a character ID.
    public void SetSelectedCharacterByID(int characterID)
    {
        string characterName = this.GetComponent<CharacterManager>().characterNames.Find(name => charNameToIDMap[name] == characterID);
        if (!string.IsNullOrEmpty(characterName))
        {
            SetSelectedCharacterByName(characterName);
        }
    }

    public void SetSelectedCharacterByName(string characterName)
    {
        int index = charBChoice.options.FindIndex(option => option.text == characterName);
        if (index != -1)
        {
            charBChoice.value = index;
        }
    }

    // Method to set the selected type based on an index (0 for "Casual", 1 for "Close").
    public void SetSelectedTypeByIndex(int index)
    {
        if (index >= 0 && index < typeChoice.options.Count)
        {
            typeChoice.value = index;
        }
    }

    public void ConfirmProtagonist()
    {
        protagonist.text = this.GetComponent<CharacterManager>().characterNames[dbCharA - this.GetComponent<CharacterManager>().IDListOffset];
    }

    public void SendDataToPHP()
    {
        StartCoroutine(UpdateInteraction());
    }

    IEnumerator UpdateInteraction()
    {
        List<IMultipartFormSection> theForm = new List<IMultipartFormSection>();
        theForm.Add(new MultipartFormDataSection("idPost", intID.ToString()));
        theForm.Add(new MultipartFormDataSection("APost", dbCharA.ToString()));
        theForm.Add(new MultipartFormDataSection("BPost", dbCharB.ToString()));
        theForm.Add(new MultipartFormDataSection("typePost", type.ToString()));

        using (UnityWebRequest therequest = UnityWebRequest.Post("https://labdatagen.com/EditPersonalInteraction.php", theForm))
        {
            yield return therequest.SendWebRequest();
            string responseText = therequest.downloadHandler.text;
            Debug.Log(responseText);

            therequest.Dispose();
            Debug.Log("Request Disposed");

            this.GetComponent<CharacterManager>().RetrivePersonalInteractions();

            EditIntPanel.SetActive(false);
        }
    }


}

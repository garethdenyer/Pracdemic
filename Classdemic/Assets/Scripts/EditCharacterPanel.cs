using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.XR;

public class EditCharacterPanel : MonoBehaviour
{
    // On EmptyHolder

    public TMP_Dropdown maskChoices;
    public TMP_Dropdown handsChoices;

    public GameObject editCharPanel;

    public void SendDataToPHP()
    {
        StartCoroutine(UpdateInteraction());
    }

    IEnumerator UpdateInteraction()
    {
        int CharNo = this.GetComponent<CharacterManager>().personalCharacter;

        List<IMultipartFormSection> theForm = new List<IMultipartFormSection>();
        theForm.Add(new MultipartFormDataSection("charPost", CharNo.ToString()));
        theForm.Add(new MultipartFormDataSection("maskPost", maskChoices.value.ToString()));
        theForm.Add(new MultipartFormDataSection("handsPost", handsChoices.value.ToString()));

        using (UnityWebRequest therequest = UnityWebRequest.Post("https://labdatagen.com/EditChar.php", theForm))
        {
            yield return therequest.SendWebRequest();
            string responseText = therequest.downloadHandler.text;
            Debug.Log(responseText);

            therequest.Dispose();
            Debug.Log("Request Disposed");

            //update the text in the Character Summary
            //translated this is equal to the text value of the selected dropdown in each case
            this.GetComponent<CharacterManager>().maskStatus.text = "<b>Mask</b> " + maskChoices.options[maskChoices.value].text;
            this.GetComponent<CharacterManager>().handsStatus.text = "<b>Hands</b> " + handsChoices.options[handsChoices.value].text;

            //hide edit panel
            editCharPanel.SetActive(false);
        }
    }

    public void OpenMaskHandPanel()
    {
        editCharPanel.SetActive(true);
    }
}

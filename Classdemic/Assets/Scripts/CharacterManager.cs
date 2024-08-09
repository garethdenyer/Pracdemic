
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    // Attached to Empty Holder

    #region Variables
    public List<GameObject> characters = new List<GameObject>();  //list of our characters

    public GameObject characterPrefab;

    //lists to hold character attributes as they come in
    public List<string> characterNames = new List<string>();
    public List<string> characterNos = new List<string>(); //not actually used
    public List<string> characterInitalViralLoads = new List<string>();
    public List<string> masks = new List<string>();
    public List<string> hands = new List<string>();

    //lists to hold timecourse information - how load change with time
    public List<int> viralLoadTC = new List<int>();
    // Define a dictionary to map dbload ranges to daysInfected values
    Dictionary<int, int> viralLoadToDaysInfected = new Dictionary<int, int>
    {
        { 91, 5 },
        { 81, 4 },
        { 71, 6 },
        { 61, 3 },
        { 51, 7 },
        { 41, 8 },
        { 31, 2 },
        { 21, 9 },
        { 11, 1 },
        { 0, 0 }
    };

    public int IDListOffset = 7; //the difference between the UniqueIDs passed from DB and the position of the character in lists

    public TMP_InputField infectiousness;
    public TMP_InputField noRandomsIF;
    public TMP_Text currInfectness;
    public int infectivity;
    public int noRandoms;

    //lits to hold information about interactions
    public List<string> interactionNos = new List<string>();
    public List<string> charAs = new List<string>();
    public List<string> charBs = new List<string>();
    public List<string> interactionTypes = new List<string>();

    //fields required for showing information about an interaction
    public TMP_Text interactionReportText;
    public GameObject interactionReportPanel;
    public TMP_Text statusText;
    public GameObject statusReport;
    public Slider interactionSpeedControl;
    public GameObject runInteractionsPanel;
    public GameObject makeInteractionsPanel;

    public TMP_Text interactionsSummary;
    public TMP_Text charactersSummary;
    public GameObject charSummaryButton;
    public GameObject intnSummaryButton;
    public TMP_Text roundIndicator;
    public GameObject charImportPanel;
    public GameObject infoItems;
    public GameObject personalCharacterSummaryPanel;
    public GameObject summarySection;

    int intround;
    int days;
    public TMP_Text daysIndicator;

    public GameObject toggleMarkersButton;
    public bool showMarkers;
    public bool highComplexity;
    public TMP_Text complexity;

    public int personalCharacter;
    public TMP_Text personalCharacterText;
    public TMP_Text maskStatus;
    public TMP_Text handsStatus;

    public GameObject interactionItemPrefab;
    public GameObject interactionsPanelContent;
    public GameObject interactionsPanelCharBContent;
    public List<GameObject> chosenInteractions = new List<GameObject>();
    public List<GameObject> charBInteractions = new List<GameObject>();

    public TMP_Text pracdemicTitle;

    #endregion

    private void Start()
    {
        intround = 0;
        roundIndicator.text = intround.ToString();
        days = 0;
        daysIndicator.text = days.ToString();
        viralLoadTC = new List<int>() { 0, 10, 25, 50, 80, 100, 70, 30, 10, 5 };

        //attach a listener to the inpuf field's OnValue changed event
        infectiousness.onValueChanged.AddListener(ExtractInfectFromInputField);
        noRandomsIF.onValueChanged.AddListener(ExtractRandNoFromInputField);

        infectiousness.text = "1";
        highComplexity = true;

        RefreshMoribundCharacters();

        charImportPanel.SetActive(true);

        StartCoroutine(MakeRecordofRun());
    }

    public void ToggleMarkers()
    {
        if (showMarkers)
        {
            showMarkers = false;
            foreach (GameObject newchar in characters)
            {
                newchar.GetComponent<Character>().SetAllMaterialsToPlain();
            }
        }
        else
        {
            showMarkers = true;
            foreach (GameObject newchar in characters)
            {
                newchar.GetComponent<Character>().UpdateMaskIndicator();
                newchar.GetComponent<Character>().UpdateHandsIndiactor();
                newchar.GetComponent<Character>().UpdateVirusIndicator();
                newchar.GetComponent<Character>().UpdateImmunisedIndicator();
            }
        }
    }

    public void ToggleComplexity()
    {
        if (highComplexity)
        {
            highComplexity = false;
            complexity.text = "Simple Transfer";
        }
        else
        {
            highComplexity = true;
            complexity.text = "Multifactor Transfer";
        }
    }

    public void CharacterDownload()
    {
        charImportPanel.SetActive(false);
        statusReport.SetActive(true);
        statusText.text = "Trying to access database of characters";
        StartCoroutine(ImportCharacters());
    }

    public void RefreshMoribundCharacters()
    {
        statusReport.SetActive(true);
        statusText.text = "Contacting database....";
        StartCoroutine(RefreshTakenStatus());
    }

    public void AllocateUserCharacter()
    {
        this.GetComponent<Timer>().timeRemaining = 600f;
        StartCoroutine(AllocateCharacter());
    }

    public void MakeAllCharSummary() //does all the steps from making the table to exporting to CSV
    {
        //set the headers
        //charactersSummary.text = "Name" + '\t' + "Load" + '\t' + "Days" + '\t' + "Immunity" + '\n';
        charactersSummary.text = "Name" + '\t' + "Infected" + '\n';

        //make the table
        foreach (GameObject charry in characters)
        {
            int infrep = charry.GetComponent<Character>().viralLoad > 0 ? 1 : 0;

            //charactersSummary.text += charry.name + '\t' + infrep.ToString() + '\t' +
            //    charry.GetComponent<Character>().daysInfected.ToString("N0") + '\t' +
            //    charry.GetComponent<Character>().immunity.ToString("N0") + '\n';

            charactersSummary.text += charry.name + '\t' + infrep.ToString()  + '\n';
        }

        //export the data
        this.GetComponent<ExportTMPtoCSV>().ExportData(charactersSummary.text, "CharacterSummary", roundIndicator.text);
    }

    public void MakeInteractionSummary() //exports the data
    {
        this.GetComponent<ExportTMPtoCSV>().ExportData(interactionsSummary.text, "InteractionSummary", roundIndicator.text);
    }

    public void CreateOneCharacter(Vector3 spawnpos, int charno) //makes one character
    {
        //makes a character, gives it a name bsed on list, sends it to a position, sets the basic stuatuses, adds to list
        GameObject newchar = Instantiate(characterPrefab);
        newchar.name = characterNames[charno];
        newchar.GetComponent<Character>().nametag.text = newchar.name;
        newchar.transform.position = spawnpos;

        //mask and hand status shoudl come from db
        newchar.GetComponent<Character>().maskStatus = int.Parse(masks[charno]); //0 off, 1 on
        newchar.GetComponent<Character>().handStatus = int.Parse(hands[charno]); //0 dirty, 1 clean, 2 sanitised

        //viralLoad will come from a knowledge of days of infection from the database
        int dbload = int.Parse(characterInitalViralLoads[charno]);

        // Find the appropriate daysInfected based on dbload
        int daysInfected = 0;
        foreach (var kvp in viralLoadToDaysInfected)
        {
            if (dbload > kvp.Key)
            {
                daysInfected = kvp.Value;
                break;
            }
        }

        // Set the daysInfected and viralLoad
        newchar.GetComponent<Character>().daysInfected = daysInfected;
        newchar.GetComponent<Character>().viralLoad = viralLoadTC[daysInfected];

        //set state of immunity - if infected then we are immune
        if (newchar.GetComponent<Character>().daysInfected > 0)
        {
            newchar.GetComponent<Character>().immunity = 1;
        }
        else
        {
            newchar.GetComponent<Character>().immunity = 0;
        }

        //add to list
        characters.Add(newchar);

        //update visual atributes
        if (showMarkers)
        {
            newchar.GetComponent<Character>().UpdateMaskIndicator();
            newchar.GetComponent<Character>().UpdateHandsIndiactor();
            newchar.GetComponent<Character>().UpdateVirusIndicator();
            newchar.GetComponent<Character>().UpdateImmunisedIndicator();
        }

    }

    public void ATwoCharInteraction(int CharA, int CharB, int interactionTime, int interntype) //basic function for interacting characters
    {
        #region The Protagonists - definining the characters and their loads
        //find the two characters that are going to interact
        GameObject CharAgo = characters[CharA];
        GameObject CharBgo = characters[CharB];

        //determine the viral load in each character
        int CharAViralLoad = CharAgo.GetComponent<Character>().viralLoad;
        int CharBViralLoad = CharBgo.GetComponent<Character>().viralLoad;

        //determine the mask status of each chacter
        string ChaAmsk = (CharAgo.GetComponent<Character>().maskStatus == 0) ? "Off" : "On";
        string ChaBmsk = (CharBgo.GetComponent<Character>().maskStatus == 0) ? "Off" : "On";

        //determine the hand status of each character
        string ChaAhnd = "";
        switch (CharAgo.GetComponent<Character>().handStatus)
        {
            case 0:
                ChaAhnd = "Unwashed";
                break;
            case 1:
                ChaAhnd = "Washed";
                break;
            case 2:
                ChaAhnd = "Santized";
                break;
        }
        string ChaBhnd = "";
        switch (CharBgo.GetComponent<Character>().handStatus)
        {
            case 0:
                ChaBhnd = "Unwashed";
                break;
            case 1:
                ChaBhnd = "Washed";
                break;
            case 2:
                ChaBhnd = "Sanitized";
                break;
        }

        //confirm the pairing in the text report
        interactionReportText.text += CharAgo.name + " (Load: " + CharAViralLoad.ToString() + ") interacting with "
            + CharBgo.name + " (Load: " + CharBViralLoad.ToString() + ")" + '\n';
        //interactionsSummary.text += CharAgo.name + '\t' + CharAViralLoad.ToString() + '\t' + ChaAmsk + '\t' + ChaAhnd + '\t' +
        //                      CharBgo.name + '\t' + CharBViralLoad.ToString() + '\t' + ChaBmsk + '\t' + ChaBhnd + '\t';
        interactionsSummary.text += CharAgo.name  + '\t' + ChaAmsk + '\t' + ChaAhnd + '\t' +
                      CharBgo.name + '\t' +  ChaBmsk + '\t' + ChaBhnd + '\t';

        #endregion

        #region Escape if both or neither affected
        //there are two conditions where there will be no exchange of virus - when both infected, or both not infected
        if (CharAViralLoad > 0 && CharBViralLoad > 0)  //if both are infected already, quit
        {
            interactionReportText.text += "Both already infected - next pair please" + '\n';
            interactionsSummary.text += '\n';
            return;
        }

        if (CharAViralLoad == 0 && CharBViralLoad == 0)  //if both characters are not infected, quit
        {
            interactionReportText.text += "Neither infected - next pair please" + '\n';
            interactionsSummary.text += '\n';
            return;
        }
        #endregion

        #region Define transmitter and receiver and dose to be potenitally exchanged
        // if we are still here, 
        // Determine the transmitter and receiver
        GameObject transmitter;
        GameObject receiver;

        // only one of the first or second characters have the virus... which one is it?
        if (CharAViralLoad > 0)
        {
            transmitter = CharAgo;
            receiver = CharBgo;
        }
        else
        {
            transmitter = CharBgo;
            receiver = CharAgo;
        }

        if (receiver.GetComponent<Character>().immunity > 0)  //if receiver is immune
        {
            interactionReportText.text += receiver.name + " is immune- next pair please" + '\n';
            //interactionsSummary.text += receiver.name + " is immune" + '\n';
            interactionsSummary.text +=  '\n';
            return;
        }

        //cast the viralLoad of the transmitter to a float for subsequent calculations
        float dosetransmitted = (float)transmitter.GetComponent<Character>().viralLoad;

        //not sure why we need this line, although it does confirm who the transmitter is
        interactionReportText.text += "Transmitter is  " + transmitter.name + " with a load of " + dosetransmitted.ToString() + '\n';
        #endregion

        if (highComplexity)
        {
            #region Air Transfer
            //Calculate the amount transmitted by air.  This is affected by mask status.
            // Apply maskstatus reduction for transmitter
            string masktext = "unknown";
            switch (transmitter.GetComponent<Character>().maskStatus)
            {
                case 0:
                    dosetransmitted *= 1f;
                    masktext = " not wearing mask, transmits full dose of ";
                    break;
                case 1:
                    dosetransmitted *= 0.1f;
                    masktext = " wearing mask, reduces dose 10-fold to ";
                    break;
            }

            //report on mask status of transmitter and effect it has on dose transmitted
            interactionReportText.text += "Transmitter " + masktext + dosetransmitted.ToString("N1") + '\n';

            // Apply maskstatus reduction for receiver
            switch (receiver.GetComponent<Character>().maskStatus)
            {
                case 0:
                    dosetransmitted *= 1f;
                    masktext = " not wearing mask, receives  ";
                    break;
                case 1:
                    dosetransmitted *= 0.1f;
                    masktext = " wearing mask, reduces dose 10-fold to ";
                    break;
            }

            //report on mask status of reciver and effect it has on dose transmitted
            interactionReportText.text += "Receiver " + masktext + dosetransmitted.ToString("N1") + '\n';

            // Apply the increase in viralload in the receiver based on maskstatus and interaction time
            receiver.GetComponent<Character>().viralLoad += (int)dosetransmitted * interactionTime;
            #endregion

            #region Physical contact transfer
            // Calculate the amount of virus transmitted through handstatus and physicalContact
            float handFactor = 0;
            switch (interntype) //0 is casual, 1 is close (ie, physical)
            {
                case 0: //if no physical then handStatus doesn't matter
                    handFactor = 0;
                    break;
                case 1 when transmitter.GetComponent<Character>().handStatus == 1: //if physical then washed hand does have an effect
                    handFactor = 0.5f;
                    break;
                case 1 when transmitter.GetComponent<Character>().handStatus == 2: //if physical then dirty hand is devastating
                    handFactor = 1;
                    break;
            }

            //the receiver viralLoad is whatever the transmitter has times the hand factor - note the cast to int
            receiver.GetComponent<Character>().viralLoad += transmitter.GetComponent<Character>().viralLoad * (int)handFactor;

            interactionReportText.text += "Transmitter hand status changes viral load given to receiver to " + receiver.GetComponent<Character>().viralLoad.ToString("N1") + '\n';
            #endregion
        }
        else
        {
            receiver.GetComponent<Character>().viralLoad += (int)dosetransmitted;
        }

        #region Determine if infection is successful based on virus infectivity

        interactionReportText.text += receiver.name + " experiences " + receiver.GetComponent<Character>().viralLoad.ToString() + " and threshold is "
            + infectivity.ToString() + '\n';

        if (receiver.GetComponent<Character>().viralLoad < infectivity)
        {
            interactionReportText.text += receiver.GetComponent<Character>().viralLoad.ToString() + " not enough to get infected" + '\n';
            receiver.GetComponent<Character>().viralLoad = 0;
            //interactionsSummary.text += receiver.name + " luckily not infected  " + '\n';
            interactionsSummary.text +=  '\n';
        }
        else
        {
            //interactionsSummary.text += receiver.name + " infected " + '\n';
            interactionsSummary.text +=  '\n';
            interactionReportText.text += receiver.name + " infected " + '\n';
            receiver.GetComponent<Character>().daysInfected = 1;
            receiver.GetComponent<Character>().viralLoad = viralLoadTC[1];
            receiver.GetComponent<Character>().immunity = 1;
        }
        #endregion

        if (showMarkers)
        {
            //dont need to update mask or hand status
            receiver.GetComponent<Character>().UpdateVirusIndicator();
            receiver.GetComponent<Character>().UpdateImmunisedIndicator();
        }

    }

    public void GoThroughInteractions() //called by RUN button
    {
        interactionReportPanel.SetActive(true);
        intround += 1;
        interactionsSummary.text = "Intn" + '\t' + "CharA" + '\t' + "Mask" + '\t' + "Hand" + '\t' + "CharB" + '\t' + "Mask" + '\t' + "Hand" + '\n';
        StartCoroutine(PerformInteractions());
    }

    IEnumerator ImportCharacters()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/GetCharacters.php");

        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            statusText.text = "Error: " + request.error;
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            statusText.text = "Characters Downloaded. Spawning...";

            // Handle the response data here.

            //flush all the lists
            characterNames.Clear();
            characterNos.Clear();
            characterInitalViralLoads.Clear();
            masks.Clear();
            hands.Clear();

            //parse out the individual lines
            string[] lineitems = responseText.Split(';');

            //within each lineitem, parse the fields and build lists
            for (int i = 0; i < lineitems.Length - 1; i++)
            {
                //parsing the fields within lineitem
                string[] fields = lineitems[i].Split('|');

                //creating the list with first field which is UniqueID
                characterNos.Add(fields[1]);

                //creating the list with second field which is names
                characterNames.Add(fields[2]);

                //creating the list with third field which is Viral Load
                //but not if RandNos is >0
                if (noRandoms < 1)
                {
                    characterInitalViralLoads.Add(fields[3]);
                }
                else
                {
                    characterInitalViralLoads.Add("0");
                }

                //creating hte working list of mask status'
                masks.Add(fields[4]);

                //creating the working list of hand status'
                hands.Add(fields[5]);
            }

            pracdemicTitle.gameObject.SetActive(false);

            //spawn the characters
            StartCoroutine(SpawnCharacters());
        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    IEnumerator ImportInteractions()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/GetInteractions.php");

        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            statusText.text = "Error: " + request.error;
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            //Debug.Log("Response: " + responseText);

            // Handle the response data here.

            //flush all the lists
            interactionNos.Clear();
            charAs.Clear();
            charBs.Clear();
            interactionTypes.Clear();

            //parse out the individual lines
            string[] lineitems = responseText.Split(';');

            //within each lineitem, parse the fields and build lists
            for (int i = 0; i < lineitems.Length - 1; i++)
            {
                //parsing the fields within lineitem
                string[] fields = lineitems[i].Split('|');

                //creating the list with first field which is interactionID
                interactionNos.Add(fields[1]);

                //creating the list with second field which is CharA
                charAs.Add(fields[2]);

                //creating the list with third field which is CharB
                charBs.Add(fields[3]);

                //creating the list with fourth field which is InteractionType
                interactionTypes.Add(fields[4]);

            }
        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();

        //done
        statusText.text = "Interactions downloaded... ready to run!";
        makeInteractionsPanel.SetActive(false);
        personalCharacterSummaryPanel.SetActive(false);
        runInteractionsPanel.SetActive(true);
        infoItems.SetActive(true);
    }

    IEnumerator SpawnCharacters()
    {
        for (int i = 0; i < characterNames.Count; i++)
        {
            float randx = Random.Range(-20f, 20f);
            float randy = Random.Range(-16f, 20f); //leabves room at front for interacting pairs
            Vector3 spawnat = new Vector3(randx, 0, randy);

            CreateOneCharacter(spawnat, i);

            yield return null; // Yield control for one frame
        }

        //go through all the characters and make some randomly infected
        if (noRandoms > 0)
        {
            for (int c = 0; c < noRandoms; c++)
            {
                int chartoinfect = GetNonInfectedCharacterIndex();
                characters[chartoinfect].GetComponent<Character>().viralLoad = 10;
                characters[chartoinfect].GetComponent<Character>().daysInfected = 1;
                characters[chartoinfect].GetComponent<Character>().immunity = 1;
            }
        }

        //show toggle markers button
        toggleMarkersButton.SetActive(true);

        //update the dropdowns related to characters
        this.GetComponent<EditInteractionPanel>().PopulateCharDPDN();

        //done
        statusText.text = "Now download or make interactions";

        AllocateUserCharacter();

        personalCharacterSummaryPanel.SetActive(true);
        //show the Character Summary button
        summarySection.SetActive(true);
        charSummaryButton.SetActive(true);
        makeInteractionsPanel.SetActive(true);
    }

    IEnumerator PerformInteractions() //go thrhough the tables of interactions doing each one in turn
    {
        //disable panels that are irrelvant
        runInteractionsPanel.SetActive(false);
        statusReport.SetActive(false);

        for (int i = 0; i < interactionNos.Count; i++)
        {
            int indexCharAinCharList = int.Parse(charAs[i]) - IDListOffset; //the index of the character in the list is less than the UniqueID
            int indexCharBinCharList = int.Parse(charBs[i]) - IDListOffset;
            int IntType = int.Parse(interactionTypes[i]);

            //save the position of the characters
            Vector3 charAgoPos = characters[indexCharAinCharList].transform.position;
            Vector3 charBgoPos = characters[indexCharBinCharList].transform.position;

            //move the two characters to the front
            characters[indexCharAinCharList].transform.position = new Vector3(-1f, 0f, -19f);
            characters[indexCharBinCharList].transform.position = new Vector3(1f, 0f, -19f);

            //give the interaction a title
            interactionReportText.text = "<b>Interaction " + (i + 1).ToString() + " of " + interactionNos.Count.ToString() + "</b>" + '\n' + '\n';
            interactionsSummary.text += (i + 1).ToString() + '\t';
            //do the interaction
            ATwoCharInteraction(indexCharAinCharList, indexCharBinCharList, 1, IntType);

            //wait for however long it the slider is set
            yield return new WaitForSeconds(interactionSpeedControl.value);

            //return characters to where they were
            characters[indexCharAinCharList].transform.position = charAgoPos;
            characters[indexCharBinCharList].transform.position = charBgoPos;
        }

        //remove interaction report panel and reset the text
        interactionReportPanel.SetActive(false);
        interactionReportText.text = "";

        //show the Interaction Summary button
        intnSummaryButton.SetActive(true);

        //done
        statusReport.SetActive(true);
        statusText.text = "Interactions done... load some new ones!";
        makeInteractionsPanel.SetActive(true);
        personalCharacterSummaryPanel.SetActive(true);
        roundIndicator.text = intround.ToString();
    }

    public void DownloadInteractions()
    {
        //start importing interactions
        StartCoroutine(ImportInteractions());
    }

    public void MakeRandomInteractions()
    {
        //flush all the lists
        interactionNos.Clear();
        charAs.Clear();
        charBs.Clear();
        interactionTypes.Clear();

        //within each lineitem, parse the fields and build lists
        for (int i = 0; i < 150; i++)
        {
            //creating the list with first field which is interactionID
            interactionNos.Add(i.ToString());

            //create a random int for CharA
            int randomCharA = Random.Range(IDListOffset, characters.Count + IDListOffset);

            //create a random int for CharB
            int randomCharB = GetDifferentCharacterIndex(randomCharA);

            //add the string of randomChars to list
            charAs.Add(randomCharA.ToString());
            charBs.Add(randomCharB.ToString());

            //creating the list with fourth field which is InteractionType
            interactionTypes.Add(Random.Range(0, 2).ToString());
        }

        //done
        statusText.text = "Interactions created... ready to run!";
        makeInteractionsPanel.SetActive(false);
        runInteractionsPanel.SetActive(true);
        makeInteractionsPanel.SetActive(false);
        infoItems.SetActive(true);

    }

    int GetDifferentCharacterIndex(int previousCharacterIndex)  // Get a random character index that is different from the specified index
    {
        int randomIndex;

        // Keep generating random indices until it's different from the specified one
        do
        {
            randomIndex = Random.Range(IDListOffset, characters.Count + IDListOffset);
        } while (randomIndex == previousCharacterIndex);

        return randomIndex;
    }

    int GetNonInfectedCharacterIndex()  // Get a random character index that is not infected
    {
        int randomIndex;
        int infectstatus;

        // Keep generating random indices while infection status of character is not infected
        do
        {
            randomIndex = Random.Range(0, characters.Count);
            infectstatus = characters[randomIndex].GetComponent<Character>().viralLoad;
        }
        while (infectstatus > 0);

        return randomIndex;
    }

    public void AdvanceDays()
    {
        foreach (GameObject chasser in characters)  //go through each character and see how many days infected
        {
            int currday = chasser.GetComponent<Character>().daysInfected;

            if (currday == 0 || currday == 9)  //if not infected OR at end of infection period
            {
                chasser.GetComponent<Character>().daysInfected = 0;
            }
            else //advance by one day
            {
                chasser.GetComponent<Character>().daysInfected = currday + 1;
            }

            //Now update viralLoad
            chasser.GetComponent<Character>().viralLoad = viralLoadTC[chasser.GetComponent<Character>().daysInfected];

            //update visual
            if (showMarkers)
            {
                chasser.GetComponent<Character>().UpdateVirusIndicator();
            }
        }

        //update days and display
        days += 1;
        daysIndicator.text = days.ToString();
    }

    private void ExtractInfectFromInputField(string input)
    {
        // Create a string to store the extracted numbers
        string extractedNumbers = "";

        // Iterate through each character in the input string
        foreach (char c in input)
        {
            // Check if the character is a digit (0-9)
            if (char.IsDigit(c))
            {
                // Append the digit to the extractedNumbers string
                extractedNumbers += c;
            }
        }

        // Try to parse the extractedNumbers string to an integer
        if (int.TryParse(extractedNumbers, out int parsedNumber))
        {
            // Update the resultText with the parsed integer value
            currInfectness.text = "Threshold: " + parsedNumber.ToString();
            infectivity = parsedNumber;
        }
        else
        {
            // Handle the case where parsing fails (e.g., display an error message)
            currInfectness.text = "Invalid Input";
        }
    }

    private void ExtractRandNoFromInputField(string input)
    {
        // Create a string to store the extracted numbers
        string extractedNumbers = "";

        // Iterate through each character in the input string
        foreach (char c in input)
        {
            // Check if the character is a digit (0-9)
            if (char.IsDigit(c))
            {
                // Append the digit to the extractedNumbers string
                extractedNumbers += c;
            }
        }

        // Try to parse the extractedNumbers string to an integer
        if (int.TryParse(extractedNumbers, out int parsedNumber))
        {
            noRandoms = parsedNumber;
        }
        else
        {
            // Handle the case where parsing fails (e.g., display an error message)
            Debug.Log("Invalid Input");
        }
    }

    IEnumerator RefreshTakenStatus()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/RefreshCharacters.php");

        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            statusText.text = "Error: " + request.error;
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            statusText.text = "Ready to go...";
        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    IEnumerator AllocateCharacter()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/AllocateCharacter.php");

        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            Debug.Log(responseText);

            //parse out the individual fields - theres only one line
            string[] fields = responseText.Split('|');
            personalCharacterText.text = fields[2];
            personalCharacter = int.Parse(fields[1]);

            int masky = int.Parse(fields[4]);
            int handsy = int.Parse(fields[5]);
            this.GetComponent<EditCharacterPanel>().maskChoices.value = masky;
            this.GetComponent<EditCharacterPanel>().handsChoices.value = handsy;
            maskStatus.text = "<b>Mask</b> " + GetComponent<EditCharacterPanel>().maskChoices.options[masky].text;
            handsStatus.text = "<b>Hands</b> " + GetComponent<EditCharacterPanel>().handsChoices.options[handsy].text;
        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();

        RetrieveCharBInteractions();
        RetrivePersonalInteractions();

        this.GetComponent<Timer>().timerIsRunning = true;
    }

    public void RetrivePersonalInteractions()
    {
        StartCoroutine(ViewPersonalCharacterInteractions());
    }

    IEnumerator ViewPersonalCharacterInteractions()
    {
        //get rid of all the current lineitems and reset list
        foreach (GameObject lineitem in chosenInteractions)
        {
            Destroy(lineitem);
        }
        chosenInteractions.Clear();

        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/GetCharSpecificInteractions.php" + "?charID=" + personalCharacter.ToString());

        Debug.Log("https://labdatagen.com/GetCharSpecificInteractions.php" + "?charID=" + personalCharacter.ToString());
        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            Debug.Log(responseText);

            //parse out the individual lines
            string[] lineitems = responseText.Split(';');

            //for each lineitem, make an interaction item, then populate the fields 
            for (int i = 0; i < lineitems.Length - 1; i++)
            {
                //parsing the fields within lineitem
                string[] fields = lineitems[i].Split('|');

                GameObject intlistitem = Instantiate(interactionItemPrefab);
                intlistitem.transform.SetParent(interactionsPanelContent.transform);
                chosenInteractions.Add(intlistitem);
                //bring across the numbers from the database
                intlistitem.GetComponent<InteractionItem>().charBDBindex = int.Parse(fields[3]);
                intlistitem.GetComponent<InteractionItem>().interactType = int.Parse(fields[4]);
                intlistitem.GetComponent<InteractionItem>().charAdbIndex = int.Parse(fields[2]);
                intlistitem.GetComponent<InteractionItem>().interactionno = int.Parse(fields[1]);
                if (fields[5] == "1")
                {
                    intlistitem.GetComponent<InteractionItem>().active = true;
                    intlistitem.GetComponent<InteractionItem>().activeMarker.GetComponent<Image>().color = new Color(0.1f, 0.9f, 0.1f, 0.3f);
                }
                else
                {
                    intlistitem.GetComponent<InteractionItem>().active = false;
                    intlistitem.GetComponent<InteractionItem>().activeMarker.GetComponent<Image>().color = new Color(0.9f, 0.1f, 0.1f, 0.3f);
                }

                //convert the numbers into text
                //for the chracter index we get what it is in teh database but subtract offset
                int indexofchar = int.Parse(fields[3]) - IDListOffset;
                intlistitem.GetComponent<InteractionItem>().otherCharacter.text = characterNames[indexofchar];

                if (fields[4] == "1")
                {
                    intlistitem.GetComponent<InteractionItem>().type.text = "Close";
                }
                else
                {
                    intlistitem.GetComponent<InteractionItem>().type.text = "Casual";
                }

            }

        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    public void RetrieveCharBInteractions()
    {
        StartCoroutine(ViewPersonalCharBInteractions());
    }

    IEnumerator ViewPersonalCharBInteractions()
    {
        //get rid of all the current lineitems for Charb and reset list
        foreach (GameObject lineitem in charBInteractions)
        {
            Destroy(lineitem);
        }
        charBInteractions.Clear();

        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/GetCharBSpecificInteractions.php" + "?charID=" + personalCharacter.ToString());

        Debug.Log("https://labdatagen.com/GetCharBSpecificInteractions.php" + "?charID=" + personalCharacter.ToString());
        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            Debug.Log(responseText);

            //parse out the individual lines
            string[] lineitems = responseText.Split(';');

            //for each lineitem, make an interaction item, then populate the fields 
            for (int i = 0; i < lineitems.Length - 1; i++)
            {
                //parsing the fields within lineitem
                string[] fields = lineitems[i].Split('|');

                GameObject intlistitem = Instantiate(interactionItemPrefab);
                intlistitem.transform.SetParent(interactionsPanelCharBContent.transform);
                charBInteractions.Add(intlistitem);
                //no submitting for this type
                intlistitem.GetComponent<InteractionItem>().submitButton.SetActive(false);
                //bring across the numbers from the database
                intlistitem.GetComponent<InteractionItem>().charBDBindex = int.Parse(fields[3]);
                intlistitem.GetComponent<InteractionItem>().interactType = int.Parse(fields[4]);
                intlistitem.GetComponent<InteractionItem>().charAdbIndex = int.Parse(fields[2]);
                intlistitem.GetComponent<InteractionItem>().interactionno = int.Parse(fields[1]);

                if (fields[5] == "1")
                {
                    intlistitem.GetComponent<InteractionItem>().active = true;
                    intlistitem.GetComponent<InteractionItem>().activeMarker.GetComponent<Image>().color = new Color(0.1f, 0.9f, 0.1f, 0.3f);
                }
                else
                {
                    intlistitem.GetComponent<InteractionItem>().active = false;
                    intlistitem.GetComponent<InteractionItem>().activeMarker.GetComponent<Image>().color = new Color(0.9f, 0.1f, 0.1f, 0.3f);
                }

                //convert the numbers into text
                //for the chracter index we get what it is in teh database but subtract 6 
                int indexofchar = int.Parse(fields[2]) - IDListOffset;
                intlistitem.GetComponent<InteractionItem>().otherCharacter.text = characterNames[indexofchar];

                if (fields[4] == "1")
                {
                    intlistitem.GetComponent<InteractionItem>().type.text = "Close";
                }
                else
                {
                    intlistitem.GetComponent<InteractionItem>().type.text = "Casual";
                }

            }

        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    public void UpdateCharAttributes()
    {
        StartCoroutine(UpdateMaskHands());
    }

    IEnumerator UpdateMaskHands()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/GetCharacters.php");

        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Retrieve the response as a string.
        responseText = request.downloadHandler.text;

        // Handle the response data here.

        //parse out the individual lines
        string[] lineitems = responseText.Split(';');

        //within each lineitem, parse the fields and build lists
        for (int i = 0; i < lineitems.Length - 1; i++)
        {
            //parsing the fields within lineitem
            string[] fields = lineitems[i].Split('|');

            //creating hte working list of mask status'
            masks[i] = fields[4];

            //creating the working list of hand status'
            hands[i] = fields[5];
        }

        for(int i = 0; i<characters.Count; i++)
        {
            characters[i].GetComponent<Character>().maskStatus = int.Parse(masks[i]);
            characters[i].GetComponent<Character>().handStatus = int.Parse(hands[i]);
        }

        Debug.Log("Confirming Mask Hands update");
        statusText.text = "Updated... make interactions";

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    public void ReclaimThisCharacter()
    {
        StartCoroutine(ReclaimCurrentCharacter());
    }

    IEnumerator ReclaimCurrentCharacter()
    {
        // Create a new UnityWebRequest with a GET request method.
        UnityWebRequest request = UnityWebRequest.Get("https://labdatagen.com/ReclaimCharacter.php" + "?charID=" + personalCharacter.ToString());

        Debug.Log("https://labdatagen.com/ReclaimCharacter.php" + "?charID=" + personalCharacter.ToString());
        // Send the request and yield until it's completed.
        yield return request.SendWebRequest();

        string responseText = "";

        // Check for any errors during the request.
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            // Retrieve the response as a string.
            responseText = request.downloadHandler.text;
            Debug.Log(responseText);
            this.GetComponent<Timer>().timeRemaining = 600f;
        }

        // Dispose of the UnityWebRequest object.
        request.Dispose();
    }

    IEnumerator MakeRecordofRun()
    {
        string currtime = System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

        List<IMultipartFormSection> theForm = new List<IMultipartFormSection>();
        theForm.Add(new MultipartFormDataSection("usernamePost", "Student"));
        theForm.Add(new MultipartFormDataSection("sessionPost", "Week 12"));
        theForm.Add(new MultipartFormDataSection("timedatePost", currtime));
        theForm.Add(new MultipartFormDataSection("lessonPost", "Pracdemic"));

        using (UnityWebRequest therequest = UnityWebRequest.Post("https://labdatagen.com/AddELMASession.php", theForm))
        {
            yield return therequest.SendWebRequest();
            therequest.Dispose();
            Debug.Log("PracdemicDipsosed");
        }
    }
}

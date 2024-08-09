using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using TMPro;

public class ExportTMPtoCSV : MonoBehaviour
{

    public void ExportData(string userText, string nameoffile, string intround)
    {
        // Capture the Data from Text Field
        string rawData = userText;

        // Split by newlines to get rows
        string[] rows = rawData.Split('\n');

        // Generate CSV String
        StringBuilder csv = new StringBuilder();

        foreach (string row in rows)
        {
            // Clean the row from any unwanted whitespaces
            string cleanRow = row.Trim();

            // If row is empty, continue to the next
            if (string.IsNullOrEmpty(cleanRow)) continue;

            // Split the row by the tab character to determine columns
            string[] columns = cleanRow.Split('\t');

            // Reconstruct the row for CSV, ensuring each column is appropriately formatted
            csv.AppendLine(string.Join(",", columns));
        }

        // Check if we are running in the Unity Editor
#if UNITY_EDITOR
        SaveToFile(csv.ToString(), nameoffile, intround);
#else
    SendDataToBrowser(csv.ToString(), nameoffile, intround);
#endif
    }

    private void SaveToFile(string csvString, string baseName, string intrnd)
    {
        // Use the GameObject's name to determine the base filename
        string directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string fullPath = directory + "/" + baseName + "_" + "Rnd" + intrnd + ".csv";

        // If a file with that name already exists, append an index to the name
        int counter = 1;
        while (File.Exists(fullPath))
        {
            fullPath = directory + "/" + baseName + "_" + "Rnd" + intrnd + "_" + counter + ".csv";
            counter++;
        }

        File.WriteAllText(fullPath, csvString);
        Debug.Log("Data saved to: " + fullPath);

        // Open the directory containing the file (optional)
        System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(fullPath));
    }

    private void SendDataToBrowser(string csvString, string baseName, string intrnd)
    {
        string filename = baseName + "_" + "Rnd" + intrnd + ".csv";

        // This creates a new anchor (<a>) tag, sets its href to the data URI, and clicks it to start the download.
        Application.ExternalEval(@"
        var csvData = new Blob(['" + csvString.Replace("\n", "\\n").Replace("\r", "").Replace("'", "\\'") + @"'], {type: 'text/csv'});
        var a = document.createElement('a');
        a.style.display = 'none';
        document.body.appendChild(a);
        a.href = window.URL.createObjectURL(csvData);
        a.download = '" + filename + @"';
        a.click();
        window.URL.revokeObjectURL(a.href);
        document.body.removeChild(a);
    ");
    }

}

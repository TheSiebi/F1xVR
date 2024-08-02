using UnityEngine;
using System.IO;

public class FileRenamer : MonoBehaviour
{
    // Directory path to the folder containing the files
    public string folderPath = "Assets/YourFolder";
    
    // Prefix to add to each file name
    public string prefix = "NewPrefix_";

    // Function to rename files
    public void RenameFiles()
    {
        // Ensure the folder path exists
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("The specified folder path does not exist: " + folderPath);
            return;
        }

        // Get all files in the directory
        string[] files = Directory.GetFiles(folderPath);

        // Iterate over each file and rename it
        foreach (string filePath in files)
        {
            // Get the file name without the directory path
            string fileName = Path.GetFileName(filePath);

            // Construct the new file name with the prefix
            string newFileName = prefix + fileName;

            // Combine the folder path with the new file name to get the full path
            string newFilePath = Path.Combine(folderPath, newFileName);

            // Check if the new file name already exists to avoid conflicts
            if (File.Exists(newFilePath))
            {
                Debug.LogWarning($"File '{newFileName}' already exists. Skipping renaming for '{fileName}'.");
                continue;
            }

            // Rename (move) the file
            File.Move(filePath, newFilePath);

            // Optionally, log the renaming action
            Debug.Log($"Renamed '{fileName}' to '{newFileName}'");
        }
    }

    // You can call this function from another script or via Unity's Inspector
    [ContextMenu("Rename Files")]
    public void Start()
    {
        RenameFiles();
    }
}

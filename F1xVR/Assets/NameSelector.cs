using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NameSelector : MonoBehaviour
{
    public TMP_Text nameText; // TextMeshPro text element to display the name
    public List<string> names; // List of names
    private int currentIndex = 0; // Index of the currently displayed name

    void Start()
    {
        // Display the first name when the script starts
        DisplayCurrentName();
    }

    void DisplayCurrentName()
    {
        // Ensure the current index is within bounds of the list
        currentIndex = Mathf.Clamp(currentIndex, 0, names.Count - 1);
        
        // Display the name at the current index
        nameText.text = names[currentIndex];
        PlayerPrefs.SetString("MeshPath", nameText.text);
    }

    public void NextName()
    {
        // Move to the next name in the list
        currentIndex++;
        
        // If reached the end of the list, loop back to the beginning
        if (currentIndex >= names.Count)
        {
            currentIndex = 0;
        }
        
        // Display the new name
        DisplayCurrentName();
    }

    public void BackName()
    {
        // Move to the next name in the list
        currentIndex--;
        
        // If reached the end of the list, loop back to the beginning
        if (currentIndex < 0)
        {
            currentIndex = names.Count;
        }
        
        // Display the new name
        DisplayCurrentName();
    }
}

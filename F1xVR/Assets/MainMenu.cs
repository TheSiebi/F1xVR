using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public int race_index = 0;
    public TMP_Text race_name;
    public TMP_Text session_name;
    public int session_index = 0;
    public static string session_id;
    public static string mesh_name;

    public static float x_offset = 0;
    public static float y_offset = 0;
    public static float z_offset = 0;
    private string race_options_path = "race_details";

    public AudioSource audioSource;
    void Start()
    {
        race_index = 0;
        Race(0);
        session_index = 0;
        Session(0);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);   
    }

    public void Menu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);   
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PreviousRace()
    {
        Race(-1);
    }

    public void NextRace()
    {
        Race(1);
    }

    public void Race(int direction)
    {
        if (race_name == null)
        {
            Debug.LogError("race_name TMP_Text is not assigned.");
            return;
        }

        TextAsset race_options = Resources.Load<TextAsset>($"{race_options_path}");
        if (race_options != null)
        {
            
            // Parse the JSON data
            JObject raceData = JObject.Parse(race_options.text);

            // Extract race names
            List<string> raceNames = new List<string>();
            foreach (var race in raceData)
            {
                raceNames.Add(race.Key);
            }

            // Cycle through race names
            race_index = race_index + direction;
            if (race_index >= raceNames.Count)
            {
                race_index = 0;
            }
            if (race_index < 0)
            {
                race_index = raceNames.Count - 1;
            }

            // Set the race name in the TextMeshPro text component
            race_name.text = raceNames[race_index];
            mesh_name = raceNames[race_index];
            session_index = 0;
            Session(0);
            // Get offset values
            JObject selectedRace = (JObject)raceData[raceNames[race_index]];
            if (selectedRace.ContainsKey("map_offset"))
            {
                JObject offset = (JObject)selectedRace["map_offset"];
                x_offset = offset.Value<float>("x");
                y_offset = offset.Value<float>("y");
                z_offset = offset.Value<float>("z");
            }
            else
            {
                Debug.Log($"No offset values found for race '{race_name.text}'.");
            }
        }
        else
        {
            race_name.text = "No race";
        }
    }

    public void PreviousSession()
    {
        Session(-1);
    }

    public void NextSession()
    {
        Session(1);
    }

    public void Session(int direction)
    {
        if (race_name == null)
        {
            Debug.Log("race_name TMP_Text is not assigned.");
            return;
        }

        TextAsset race_options = Resources.Load<TextAsset>(race_options_path);
        if (race_options != null)
        {
            // Parse the JSON data
            JObject raceData = JObject.Parse(race_options.text);

            if (raceData.ContainsKey(race_name.text))
            {
                JObject raceDetails = (JObject)raceData[race_name.text];
                if (raceDetails.ContainsKey("session"))
                {
                    JObject sessionDetails = (JObject)raceDetails["session"];

                    // Extract session keys
                    List<string> sessionNames = new List<string>();
                    List<string> sessionIDs = new List<string>();
                    foreach (var session in sessionDetails)
                    {
                        sessionNames.Add(session.Key);
                        sessionIDs.Add(session.Value.ToString());
                    }

                    // Cycle through session names
                    session_index = session_index + direction;
                    if (session_index >= sessionNames.Count)
                    {
                        session_index = 0;
                    }
                    if (session_index < 0)
                    {
                        session_index = sessionNames.Count - 1;
                    }

                    // Set the session name in the TextMeshPro text component
                    session_name.text = sessionNames[session_index];
                    session_id = sessionIDs[session_index];
                    // Debug.Log("Race session:");
                    // Debug.Log(session_id);
                    
                    
                }
                else
                {
                    session_name.text = "No session";
                    Debug.Log($"No session details found for race '{race_name.text}'.");
                }
            }
            else
            {
                session_name.text = "No race";
                Debug.Log($"Race name '{race_name.text}' not found.");
            }
        }
        else
        {
            session_name.text = "No race";
            Debug.Log("Race details file not found.");
        }
    }

    public void ToggleText()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        
        // Iterate through all GameObjects and check for the name "DriverNameText"
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "DriverNameText")
            {
                // Toggle the active state
                bool isActive = obj.activeSelf;
                obj.SetActive(!isActive);
            }
        }
    }

    

    public void CarSize(float sliderValue)
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        float newScale = Mathf.Lerp(1f, 5f, sliderValue);
        // Iterate through all GameObjects and check for the name "DriverNameText"
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "car_coloured(Clone)")
            {
                obj.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

    }
    
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Session(1);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Race(1);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayGame();
        }
    }

    public void OnSliderValueChanged(float value)
    {
        float newScale = Mathf.Lerp(0f, 0.25f, value);
        // Change the master volume based on the slider value
        audioSource.volume = newScale;
    }

}

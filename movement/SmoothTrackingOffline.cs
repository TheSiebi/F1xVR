using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// Keep class name the same as file name
public class SmoothTrackingOffline : MonoBehaviour
{
    // Variables for the race
    public GameObject car_coloured;
    string session_key = "9157";
    List<int> driver_numbers = new List<int> { 1, 2, 4, 10, 11, 14, 16, 18, 20, 22, 23, 27, 31, 40, 44, 55, 63, 77, 81 };
    List<Rigidbody> cars_rb = new List<Rigidbody>();
    Dictionary<int, string> driver_names = new Dictionary<int, string>(); // Dictionary to store driver numbers and names
    Dictionary<int, (Color, Color)> driver_colors = new Dictionary<int, (Color, Color)>();

    // Variables for the trajectories
    int current_tracking_time_idx = 0;
    List<DateTime> listTime = new List<DateTime>(); // List for times
    List<Queue<float>> listX = new List<Queue<float>>(); // List for x positions
    List<Queue<float>> listY = new List<Queue<float>>(); // List for y positions
    Vector3 up = new Vector3(0, 0, 1);

    // Variables for simulating the race
    bool isSimulate = false;
    DateTime startTime;
    DateTime endTime;
    float duration;
    float startTimeUnity;
    float endX, endY;
    List<float> startX = new List<float>();
    List<float> startY = new List<float>();
    List<Quaternion> startDir = new List<Quaternion>();

    LogLevel logLevel = LogLevel.All;

    void InstantiateCars()
    {
        current_tracking_time_idx = 0;
        for (int i = 0; i < driver_numbers.Count; i++)
        {
            // Position
            Vector3 position = new Vector3(listX[i].Peek(), listY[i].Peek(), 20);
            GameObject car = Instantiate(car_coloured, position, Quaternion.identity);
            car.transform.position = position;
            car.transform.localScale = new Vector3(1f, 1f, 1f);

            // Color
            Renderer renderer = car.GetComponent<Renderer>();
            foreach (Material material in renderer.materials)
            {
                if (material.name == "Ferrari_Red (Instance)")
                {
                    material.color = driver_colors[driver_numbers[i]].Item1;
                }
                else if (material.name == "Black (Instance)")
                {
                    material.color = driver_colors[driver_numbers[i]].Item2;
                }
            }

            // Rigidbody
            BoxCollider boxCollider = car.AddComponent<BoxCollider>();
            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 200;
            cars_rb.Add(rb);

            // Name text
            GameObject nameTextObject = new GameObject("DriverNameText");
            nameTextObject.transform.SetParent(car.transform); // Set parent to car
            nameTextObject.transform.localPosition = new Vector3(0, 5, 0); // Adjust position above the car
            TextMesh nameText = nameTextObject.AddComponent<TextMesh>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.text = driver_names.ContainsKey(driver_numbers[i]) ? driver_names[driver_numbers[i]] : "Unknown"; // Set driver name
            nameText.anchor = TextAnchor.MiddleCenter;
            nameText.fontSize = 30;
            nameText.color = Color.white;
        }
    }

    void LoadDriverNamesandColors()
    {
        string colorsFilePath = $"./{session_key}/colors.csv";
        string driversFilePath = $"./{session_key}/drivers.csv";

        Dictionary<string, (Color, Color)> team_colors = new Dictionary<string, (Color, Color)>();
        if (File.Exists(colorsFilePath))
        {
            using (StreamReader reader = new StreamReader(colorsFilePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        // Skip the first line
                        isFirstLine = false;
                        continue;
                    }
                    string[] fields = line.Split(',');
                    if (fields.Length >= 3)
                    {
                        string teamName = fields[0];
                        string primaryColorStr = "#" + fields[1];
                        string bgColorStr = "#" + fields[2];
                        Color primaryColor, bgColor;
                        if (ColorUtility.TryParseHtmlString(primaryColorStr, out primaryColor) && ColorUtility.TryParseHtmlString(bgColorStr, out bgColor))
                        {
                            team_colors.Add(teamName, (primaryColor, bgColor));
                        }
                        else
                        {
                            Debug.LogWarning("Invalid hex color string: " + primaryColorStr + " or " + bgColorStr);
                        }
                    }
                }
            }
        }
        else
        {
            if (logLevel >= LogLevel.Warning) Debug.LogWarning("Colors file not found!");
        }

        if (File.Exists(driversFilePath))
        {
            using (StreamReader reader = new StreamReader(driversFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    if (fields.Length >= 3 && int.TryParse(fields[0], out int driverNumber))
                    {
                        string driverName = fields[1];
                        string teamName = fields[2];
                        driver_names[driverNumber] = driverName;
                        driver_colors[driverNumber] = team_colors[teamName];
                    }
                }
            }
        }
        else
        {
            if (logLevel >= LogLevel.Warning) Debug.LogWarning("Drivers file not found!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Unload the scene with the specified name
        driver_numbers.Sort();
        
        // Initialize queues
        for (int i = 0; i < driver_numbers.Count; i++)
        {
            listX.Add(new Queue<float>());
            listY.Add(new Queue<float>());
            startX.Add(0);
            startY.Add(0);
            startDir.Add(Quaternion.identity);
        }

        if (logLevel == LogLevel.All) Debug.Log("Start");

        // Load trajectories
        foreach (int driverNumber in driver_numbers)
        {
            string filePath = $"./{session_key}/car_{driverNumber}_interpolated.csv";
            if (File.Exists(filePath))
            {
                if (listTime.Count == 0)
                {
                    LoadDateFromCSV(filePath);
                }
                LoadXYFromCSV(filePath, driverNumber);
            }
            else
            {
                if (logLevel >= LogLevel.Warning) Debug.LogWarning($"CSV file not found for driver number {driverNumber}");
            }
        }
        if (logLevel == LogLevel.All) Debug.Log("Finish loading trajectories");

        // Load driver names
        LoadDriverNamesandColors();
        if (logLevel == LogLevel.All) Debug.Log("Finish loading driver names and colors");

        // Instantiate cars
        InstantiateCars();
        if (logLevel == LogLevel.All) Debug.Log("Finish instantiation");
    }
    void LoadDateFromCSV(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(',');
                if (isFirstLine)
                {
                    // Skip the first line
                    isFirstLine = false;
                    continue;
                }
                DateTime date = GetDateTime(fields[0]);
                listTime.Add(date);
            }
        }
    }
    void LoadXYFromCSV(string filePath, int driverNumber)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(',');
                if (isFirstLine)
                {
                    // Skip the first line
                    isFirstLine = false;
                    continue;
                }
                int driver_index = driver_numbers.IndexOf(driverNumber);
                if (float.TryParse(fields[1], out float x) &&
                    float.TryParse(fields[2], out float y))
                {
                    listX[driver_index].Enqueue(-x * 0.1f);
                    listY[driver_index].Enqueue(y * 0.1f);
                }
            }
        }
    }

    private void Update()
    {
        if (isSimulate)
        {
            float sec_passed = Time.time - startTimeUnity;
            if (sec_passed < duration)
            {
                for (int i = 0; i < driver_numbers.Count; i++)
                {
                    endX = listX[i].Peek();
                    endY = listY[i].Peek();
                    Vector3 startPosition = new Vector3(startX[i], startY[i], cars_rb[i].position.z);
                    Vector3 targetPosition = new Vector3(endX, endY, cars_rb[i].position.z);
                    cars_rb[i].MovePosition(Vector3.Lerp(startPosition, targetPosition, sec_passed / duration));

                    if (logLevel == LogLevel.All)
                    {
                        Debug.Log("Lerp update driver " + driver_numbers[i].ToString() +
                            " x=" + cars_rb[i].position.x.ToString() + " y=" + cars_rb[i].position.y.ToString() +
                            "by time " + sec_passed.ToString() + "/" + duration.ToString() + "for destination time " + endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz"));
                    }
                    Vector3 direction = (targetPosition - startPosition).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction, up);
                        cars_rb[i].MoveRotation(Quaternion.Lerp(startDir[i], targetRotation, sec_passed / duration));
                    }
                }
            }
            else
            {
                // TODO set the final position to endX, endY
                current_tracking_time_idx++;
                isSimulate = false;
            }
        }
        else if (current_tracking_time_idx < listTime.Count - 1)
        {
            startTime = listTime[current_tracking_time_idx];
            endTime = listTime[current_tracking_time_idx + 1];
            startTimeUnity = Time.time;
            duration = (float)(endTime - startTime).TotalSeconds;
            for (int i = 0; i < driver_numbers.Count; i++)
            {
                startX[i] = cars_rb[i].position.x;
                listX[i].Dequeue();
                startY[i] = cars_rb[i].position.y;
                listY[i].Dequeue();
                startDir[i] = cars_rb[i].rotation;
            }
            isSimulate = true;
        }
    }

    // Return in format utcDateTime+durationSec
    public static string GetNextSecond(string utcDateTime, double durationSec = 10)
    {
        // Parse the input string to DateTime object
        if (DateTime.TryParseExact(utcDateTime, "yyyy-MM-dd HH:mm:ss.ffffffzzz",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AdjustToUniversal,
                                    out DateTime dt))
        {
            dt = dt.AddSeconds(durationSec);
            // Format the DateTime object to ISO 8601 format
            return dt.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz");
        }
        else
        {
            // Return an empty string if parsing fails
            return string.Empty;
        }
    }

    public DateTime GetDateTime(string utcDateTime)
    {
        // Have no idea why there's a fking '2023-09-03T13:48:36' without microsec in Monza date data
        // So I append 000000 to make it '2023-09-03T13:48:36.000000'
        int offsetIndex = utcDateTime.LastIndexOf('+'); // Find the index of the timezone offset
        if (offsetIndex != -1 && !utcDateTime.Contains('.'))
        {
            // Insert ".000000" before the timezone offset
            utcDateTime = utcDateTime.Substring(0, offsetIndex) + ".000000" + utcDateTime.Substring(offsetIndex);
        }

        // Parse the input string to DateTime object
        if (DateTime.TryParseExact(utcDateTime, "yyyy-MM-dd HH:mm:ss.ffffffzzz",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AdjustToUniversal,
                                    out DateTime dt))
        {
            return dt;
        }
        else
        {
            if (logLevel >= LogLevel.Warning)
            {
                Debug.LogWarning("Cannot get DateTime!" + utcDateTime.ToString());
            }
            return DateTime.MinValue;
        }
    }

    // Define a class to hold car data structure
    [System.Serializable]
    public class CarData
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public int driver_number;
        public string date;

        // You can add more fields as needed
    }

    [System.Serializable]
    public class SessionData
    {
        public string location;
        public int country_key;
        public string country_code;
        public string country_name;
        public int circuit_key;
        public string circuit_short_name;
        public string session_type;
        public string session_name;
        public string date_start;
        public string date_end;
    }

    // Helper class to parse JSON array
    public static class JsonHelper
    {
        public static T[] FromCustomJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    public enum LogLevel
    {
        None,
        Error,
        Warning,
        All
    }
}
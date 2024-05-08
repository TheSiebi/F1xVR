using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;


// Keep class name the same as file name
public class SmoothTrackingOffline : MonoBehaviour
{
    string session_key = "9157";
    List<int> driver_numbers = new List<int> { 1, 2, 4, 10, 11, 14, 16, 18, 20, 22, 23, 27, 31, 40, 44, 55, 63, 77, 81 };
    List<Rigidbody> cars_rb = new List<Rigidbody>();

    int current_tracking_time_idx = 0;
    List<DateTime> listTime = new List<DateTime>(); // List for times
    List<Queue<float>> listX = new List<Queue<float>>(); // List for x positions
    List<Queue<float>> listY = new List<Queue<float>>(); // List for y positions
    Vector3 up = new Vector3(0, 0, 1);

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
            Vector3 position = new Vector3(listX[i].Peek(), listY[i].Peek(), 20);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(5f, 5f, 5f);

            SphereCollider sphereCollider = sphere.AddComponent<SphereCollider>();
            Rigidbody rb = sphere.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 200;
            cars_rb.Add(rb);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
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
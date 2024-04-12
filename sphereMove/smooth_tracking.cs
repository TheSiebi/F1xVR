using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


public class SphereMove : MonoBehaviour
{
    string json; // Variable to store received JSON data
    CarData car = new CarData(); // the current car info
    float updateInterval = 1f / 3.7f; // x frequency of approximately 3.7Hz
    string url = "https://api.openf1.org/v1/location?session_key=9157&driver_number=81"; // An event at Monza

    List<float> listX = new List<float>(); // List for x positions
    List<float> listY = new List<float>(); // List for y positions
    List<DateTime> listTime = new List<DateTime>(); // List for times

    const int threshold = 3; // # number of non-zero data points required to start

    bool start_game = false;
    State currentState = State.Begin;

    DateTime current_tracking_time;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("State -> Begin");
        // this.gameObject.transform.localPosition = new Vector3(-320, -2052, -140); // Random pick of a initial point, can be deleted
        StartCoroutine("FetchandCacheData"); // Start the coroutine to retrieve data from openF1
        StartCoroutine("UpdateCarData");

    }

    IEnumerator FetchandCacheData()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Begin:
                    if (!start_game)
                    {
                        using (UnityWebRequest www = UnityWebRequest.Get(url))
                        {
                            yield return www.SendWebRequest();

                            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                            {
                                Debug.Log(www.error);
                            }
                            else
                            {
                                json = www.downloadHandler.text;
                                // Parse JSON array as an array of CarData {[x:,y:,z:,date:,...], [x:,y:,z:,date:,...], [x:,y:,z:,date:,...]}
                                CarData[] carDataArray = JsonHelper.FromCustomJson<CarData>(json);

                                // Check if any entry has 'x' or 'y' not zero, meaning car moves
                                // This is for real-time game, if the car has not moved, the x y will all be 0
                                int count = 0;
                                foreach (CarData data in carDataArray)
                                {
                                    if (data.x == 0 && data.y == 0) { continue; }
                                    if (count < threshold)
                                    {
                                        listTime.Add(GetDateTime(data.date));
                                        listX.Add((float)(-data.x * 0.1));
                                        listY.Add((float)(data.y * 0.1));
                                        count += 1;
                                    }
                                    else // Get N points
                                    {
                                        break;
                                    }
                                }
                                if (count >= threshold)
                                {
                                    start_game = true;
                                    car.x = listX[0];
                                    car.y = listY[0];
                                    car.date = listTime[0].ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
                                    current_tracking_time = listTime[0];
                                    Vector3 newPosition = new Vector3(car.x, car.y, this.gameObject.transform.localPosition.z);
                                    this.gameObject.transform.localPosition = newPosition;
                                    Debug.Log("Initial x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());
                                    currentState = State.Running;
                                    Debug.Log("State -> Running");
                                    yield return new WaitUntil( () => listX.Count < 3); // Wait for update interval before making the next request
                                }
                                else
                                {
                                    listTime.Clear();
                                    listX.Clear();
                                    listY.Clear();
                                    yield return null; // Wait for update interval second before making the next request
                                }
                            }
                        }
                    }
                    break;
                case State.Running:
                    url = "https://api.openf1.org/v1/location?session_key=9157&driver_number=81&date>" + listTime[listTime.Count - 1].ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
                        + "&date<" + GetNextSecond(listTime[listTime.Count - 1].ToString("yyyy-MM-ddTHH:mm:ss.ffffff"), 2);
                    Debug.Log("Try get url" + url); // Retrieve the next 2s car data
                    using (UnityWebRequest www = UnityWebRequest.Get(url))
                    {
                        yield return www.SendWebRequest();

                        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                        {
                            Debug.Log(www.error);
                        }
                        else
                        {
                            json = www.downloadHandler.text;
                            if (json != "[]")
                            {
                                // Parse JSON array
                                CarData[] carDataArray = JsonHelper.FromCustomJson<CarData>(json);

                                foreach (CarData data in carDataArray)
                                {
                                    listTime.Add(GetDateTime(data.date));
                                    listX.Add((float)(-data.x * 0.1));
                                    listY.Add((float)(data.y * 0.1));
                                }
                            }
                            else
                            {
                                Debug.Log("Empty measurement");
                            }
                        }
                    }
                    yield return new WaitUntil( () => listX.Count < 3);
                    break;
                default:
                    yield return null;
                    break;
            }

        }
    }

    bool isLerping = false; // Flag to track if lerping is in progress

    IEnumerator UpdateCarData()
    {
        // Run through interpolatePosX elements and update when the respective time is reached
        while (true)
        {
            if (start_game)
            {
                Debug.Log("UpdateCarData while called.");
                if (listX.Count >= 1)
                {
                    Debug.Log("UpdateCarData call Lerp.");
                    car.x = listX[0];
                    car.y = listY[0];
                    car.date = listTime[0].ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
                    Vector3 newPosition = new Vector3(car.x, car.y, this.gameObject.transform.localPosition.z);
                    isLerping = true;
                    StartCoroutine(LerpToPosition(newPosition, (float)(listTime[0] - current_tracking_time).TotalSeconds));
                    current_tracking_time = listTime[0];

                    // Remove the first element in the lists 
                    listX.RemoveAt(0);
                    listY.RemoveAt(0);
                    listTime.RemoveAt(0);
                    yield return new WaitUntil(() => (!isLerping) && listX.Count >= 1);
                }
                else
                {
                    Debug.Log("WARNING: Not enough points in listX.");
                    yield return new WaitForSeconds(updateInterval);
                }
            }
            else
            {
                yield return new WaitUntil(() => start_game);
            }
        }

        // -------------- TODO ------------------
        // StopCoroutine("GetCarData") when race finished
        // Either stop it here in update() or stop it in the IEnumerator
    }

    IEnumerator LerpToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = this.gameObject.transform.localPosition;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            this.gameObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            Debug.Log("Lerp update x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the final position is exactly the target position
        this.gameObject.transform.localPosition = targetPosition;
        isLerping = false;
    }

    // Return in format utcDateTime+durationSec
    public static string GetNextSecond(string utcDateTime, double durationSec = 10)
    {
        //Debug.Log("Input " + utcDateTime);
        // Parse the input string to DateTime object
        if (DateTime.TryParseExact(utcDateTime, "yyyy-MM-ddTHH:mm:ss.ffffff",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime dt))
        {
            // Add one second to the parsed DateTime object
            dt = dt.AddSeconds(durationSec);

            // Format the DateTime object to ISO 8601 format
            //Debug.Log("Output " + dt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
        }
        else
        {
            // Return an empty string if parsing fails
            return string.Empty;
        }
    }

    public static DateTime GetDateTime(string utcDateTime)
    {
        //Debug.Log("Input " + utcDateTime);
        // Parse the input string to DateTime object
        if (DateTime.TryParseExact(utcDateTime, "yyyy-MM-ddTHH:mm:ss.ffffff",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime dt))
        {
            return dt;
        }
        else
        {
            Debug.Log("Cannot get DateTime!");
            return DateTime.MinValue;
        }
    }

    public enum State
    {
        Begin,
        Running,
        Ending,
        Stopped
    }

    // Define a class to hold car data structure
    [System.Serializable]
    public class CarData
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public string date;

        // You can add more fields as needed
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
}

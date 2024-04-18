using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Globalization;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Threading;

using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


public class SphereMove : MonoBehaviour
=======

// Keep class name the same as file name
public class SmoothTracking : MonoBehaviour
>>>>>>> origin/lerping
{
    string json; // Variable to store received JSON data
    CarData car = new CarData(); // the current car info

<<<<<<< HEAD
    string url = "https://api.openf1.org/v1/location?session_key=9157&driver_number=81"; // An event at Monza
=======
    string url; // URL to fetch from
    const string session_key = "9157";
    const string driver_number = "81";
>>>>>>> origin/lerping

    Queue<float> listX = new Queue<float>(); // List for x positions
    Queue<float> listY = new Queue<float>(); // List for y positions
    Queue<DateTime> listTime = new Queue<DateTime>(); // List for times

<<<<<<< HEAD
    const int threshold = 3; // # number of non-zero data points required to start

    bool start_game = false;
    State currentState = State.Begin;
=======
    const int threshold = 15; // try to always keep at least 15 destinations in the listX

    bool start_game = false;
    bool is_fetching = false;
    bool isLerping = false; // Flag to track if lerping is in progress
>>>>>>> origin/lerping

    DateTime current_appending_time;
    DateTime current_tracking_time;

<<<<<<< HEAD
=======
    bool enableDebugLogs = false;

    Rigidbody rb; // Rigidbody component reference
>>>>>>> origin/lerping

    // Start is called before the first frame update
    void Start()
    {
<<<<<<< HEAD
        Debug.Log("State -> Begin");
        // this.gameObject.transform.localPosition = new Vector3(-320, -2052, -140); // Random pick of a initial point, can be deleted
        StartCoroutine("FetchandCacheData"); // Start the coroutine to retrieve data from openF1
        StartCoroutine("UpdateCarData");
=======
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        if (enableDebugLogs) Debug.Log("Start");
        StartCoroutine("WaitForGameBegin");
    }

    IEnumerator WaitForGameBegin()
    {
        while (!start_game)
        {
            url = "https://api.openf1.org/v1/location?session_key=" + session_key + "&driver_number=" + driver_number; // An event at Monza
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    if (enableDebugLogs) Debug.Log(www.error);
                }
                else
                {
                    json = www.downloadHandler.text;
                    // Parse JSON array as an array of CarData {[x:,y:,z:,date:,...], [x:,y:,z:,date:,...], [x:,y:,z:,date:,...]}
                    CarData[] carDataArray = JsonHelper.FromCustomJson<CarData>(json);

                    // Check if any entry has 'x' or 'y' not zero, meaning car moves
                    // This is for real-time game, if the car has not moved, the x y will all be 0
                    foreach (CarData data in carDataArray)
                    {
                        if (data.x == 0 && data.y == 0) { continue; }
                        if (listX.Count < threshold + 1)
                        {
                            listTime.Enqueue(GetDateTime(data.date));
                            listX.Enqueue((float)(-data.x * 0.1));
                            listY.Enqueue((float)(data.y * 0.1));
                            current_appending_time = GetDateTime(data.date);
                        }
                        else // Get threshold points
                        {
                            break;
                        }
                    }
                    if (listX.Count >= threshold + 1)
                    {
                        start_game = true;
                        car.x = listX.Dequeue();
                        car.y = listY.Dequeue();
                        current_tracking_time = listTime.Dequeue();
                        car.date = current_tracking_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");

                        Vector3 newPosition = new Vector3(car.x, car.y, this.gameObject.transform.localPosition.z);
                        rb.MovePosition(newPosition);
                        if (enableDebugLogs) Debug.Log("Initial x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());
                    }
                    else
                    {
                        listTime.Clear();
                        listX.Clear();
                        listY.Clear();
                        yield return new WaitForSeconds(1f); //while again to request after 1s
                    }
                }

            }
        }
>>>>>>> origin/lerping
    }

    IEnumerator FetchandCacheData()
    {
<<<<<<< HEAD

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
                                foreach (CarData data in carDataArray)
                                {
                                    if (data.x == 0 && data.y == 0) { continue; }
                                    if (listY.Count < threshold + 1)
                                    {
                                        listTime.Enqueue(GetDateTime(data.date));
                                        listX.Enqueue((float)(-data.x * 0.1));
                                        listY.Enqueue((float)(data.y * 0.1));
                                        current_appending_time = GetDateTime(data.date);
                                    }
                                    else // Get N points
                                    {
                                        break;
                                    }
                                }
                                if (listY.Count >= threshold + 1)
                                {
                                    start_game = true;
                                    car.x = listX.Dequeue();
                                    car.y = listY.Dequeue();
                                    current_tracking_time = listTime.Dequeue();
                                    car.date = current_tracking_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");

                                    Vector3 newPosition = new Vector3(car.x, car.y, this.gameObject.transform.localPosition.z);
                                    this.gameObject.transform.localPosition = newPosition;
                                    Debug.Log("Initial x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());
                                    currentState = State.Running;
                                    Debug.Log("State -> Running");
                                    yield return new WaitUntil(() => listX.Count < threshold); // Wait for update interval before making the next request
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
                    url = "https://api.openf1.org/v1/location?session_key=9157&driver_number=81&date>" + current_appending_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
                        + "&date<" + GetNextSecond(current_appending_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"), 20);
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
                                    listTime.Enqueue(GetDateTime(data.date));
                                    listX.Enqueue((float)(-data.x * 0.1));
                                    listY.Enqueue((float)(data.y * 0.1));
                                    current_appending_time = GetDateTime(data.date);
                                    yield return null;
                                }
                            }
                            else
                            {
                                Debug.Log("Empty measurement");
                            }
                        }
                    }
                    yield return new WaitUntil(() => listX.Count < threshold);
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
        yield return new WaitUntil(() => start_game && listX.Count >= 2);
        // Run through interpolatePosX elements and update when the respective time is reached
        while (true)
=======
        url = "https://api.openf1.org/v1/location?session_key=9157&driver_number=81&date>" + current_appending_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")
            + "&date<" + GetNextSecond(current_appending_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"), 2);
        if (enableDebugLogs) Debug.Log("Try get url" + url); // Retrieve the next 2s car data
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (enableDebugLogs) Debug.Log(www.error);
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
                        listTime.Enqueue(GetDateTime(data.date));
                        listX.Enqueue((float)(-data.x * 0.1));
                        listY.Enqueue((float)(data.y * 0.1));
                        current_appending_time = GetDateTime(data.date);
                        yield return null; // Append one every frame
                    }
                }
                else
                {
                    if (enableDebugLogs) Debug.LogWarning("Empty measurement");
                    // -------------- TODO ------------------
                    // 2s no data, race ends?
                    // StopCoroutine when race finished
                    // Either stop it here in update() or stop it in the IEnumerator
                }
            }
        }
        is_fetching = false;
    }

    
    private void Update()
    {
        if (!start_game) return;
        if ( (!isLerping) && listX.Count < 1 )
        {
            // If this warining doesn't show, then in this update frame the car will definitely be moved forward a bit
            if (enableDebugLogs) Debug.LogWarning("Update frame no lerping destination!");
        } 
        else if ( (!isLerping) && listX.Count >= 1 )
>>>>>>> origin/lerping
        {
            car.x = listX.Dequeue();
            car.y = listY.Dequeue();
            DateTime destination_time = listTime.Dequeue();
            car.date = destination_time.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
            Vector3 newPosition = new Vector3(car.x, car.y, this.gameObject.transform.localPosition.z);
            isLerping = true;
<<<<<<< HEAD
            StartCoroutine(LerpToPosition(newPosition, (float)(destination_time - current_tracking_time).TotalSeconds));
            current_tracking_time = destination_time;

            yield return new WaitUntil(() => (!isLerping) && listX.Count >= 2);
        }

        // -------------- TODO ------------------
        // StopCoroutine("GetCarData") when race finished
        // Either stop it here in update() or stop it in the IEnumerator
    }

=======
            // Start the coroutine to lerp to the first destination in list
            StartCoroutine(LerpToPosition(newPosition, (float)(destination_time - current_tracking_time).TotalSeconds));
            current_tracking_time = destination_time;
        }
        if ( (!is_fetching) && listX.Count < threshold)
        {
            is_fetching = true;
            StartCoroutine("FetchandCacheData"); // Start the coroutine to retrieve 2s data once
        }
    }


>>>>>>> origin/lerping
    IEnumerator LerpToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = this.gameObject.transform.localPosition;
        float startTime = Time.time;
<<<<<<< HEAD
        Vector3 up = new Vector3(0,0,1);
        Vector3 direction;
        while ((Time.time - startTime) < duration)
        {   
            direction = (targetPosition - startPosition).normalized;
            // Debug.Log("direction update" + direction.x.ToString() + "y=" + direction.y.ToString() + "z=" + direction.z.ToString());
            this.gameObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, (Time.time - startTime) / duration);
            Debug.Log("Lerp update x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());

            if (direction != Vector3.zero)
            {             
                Quaternion targetRotation = Quaternion.LookRotation(direction,up);
                this.gameObject.transform.rotation = Quaternion.Lerp(this.gameObject.transform.rotation, targetRotation,(Time.time - startTime) / duration);

            }
            yield return null;
        }
        // Ensure the final position is exactly the target position
        this.gameObject.transform.localPosition = targetPosition;
=======
        while ((Time.time - startTime) < duration)
        {
            rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, (Time.time - startTime) / duration));
            if (enableDebugLogs) Debug.Log("Lerp update x=" + this.gameObject.transform.localPosition.x.ToString() + "y=" + this.gameObject.transform.localPosition.y.ToString());
            yield return null;
        }
        // Ensure the final position is exactly the target position
        rb.MovePosition(targetPosition);
>>>>>>> origin/lerping
        isLerping = false;
    }

    // Return in format utcDateTime+durationSec
    public static string GetNextSecond(string utcDateTime, double durationSec = 10)
    {
<<<<<<< HEAD
        //Debug.Log("Input " + utcDateTime);
=======
>>>>>>> origin/lerping
        // Parse the input string to DateTime object
        if (DateTime.TryParseExact(utcDateTime, "yyyy-MM-ddTHH:mm:ss.ffffff",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime dt))
        {
<<<<<<< HEAD
            // Add one second to the parsed DateTime object
            dt = dt.AddSeconds(durationSec);

            // Format the DateTime object to ISO 8601 format
            //Debug.Log("Output " + dt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
=======
            dt = dt.AddSeconds(durationSec);
            // Format the DateTime object to ISO 8601 format
>>>>>>> origin/lerping
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
        }
        else
        {
            // Return an empty string if parsing fails
            return string.Empty;
        }
    }

<<<<<<< HEAD
    public static DateTime GetDateTime(string utcDateTime)
    {
        //Debug.Log("Input " + utcDateTime);
=======
    public DateTime GetDateTime(string utcDateTime)
    {
        // Have no idea why there's a fking '2023-09-03T13:48:36' without microsec in Monza date data
        // So I append 000000 to make it '2023-09-03T13:48:36.000000'
        if (!utcDateTime.Contains('.'))
        {
            utcDateTime += ".000000";
        }

>>>>>>> origin/lerping
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
<<<<<<< HEAD
            Debug.Log("Cannot get DateTime!");
=======
            if (enableDebugLogs) Debug.LogWarning("Cannot get DateTime!");
>>>>>>> origin/lerping
            return DateTime.MinValue;
        }
    }

<<<<<<< HEAD
    public enum State
    {
        Begin,
        Running,
        Ending,
        Stopped
    }

=======
>>>>>>> origin/lerping
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
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/lerping

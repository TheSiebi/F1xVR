using UnityEngine;

public class ObjectInstantiation : MonoBehaviour

{
    // Create a sphere primitive

    float spacing = 10f;

    public string[] driverNumbers = new string[] {"4","55","81","77","16"};

    void Start()
    {
        // Call Instantiate in the Start method
        InstantiateObject();
    }
    void InstantiateObject()
    {
        for (int i = 0; i < driverNumbers.Length; i++)
        {
            // Calculate the position for each sphere based on spacing
            Vector3 position = new Vector3(i * spacing, 0, 10);
            // Debug.Log("Position: " + i);
            // Debug.Log("Driver Number: " + driverNumbers);

            // Instantiate a sphere at the calculated position
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(5f, 10f, 10f);

            SmoothTracking smoothtrack = sphere.AddComponent<SmoothTracking>();
            // Debug.Log("Session Key: " + smoothtrack.session_key);
            // Debug.Log("Driver Number: " + smoothtrack.driver_number);
            smoothtrack.driver_number = driverNumbers[i];
            smoothtrack.session_key = "9157";

            // Add a Rigidbody component to the target object
            Rigidbody rb = sphere.AddComponent<Rigidbody>();

            // Enable gravity
            rb.useGravity = true;

            // Set isKinematic to false
            rb.isKinematic = true;

            // Set collision detection mode to continuous
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        }
        Debug.Log("Instantiated " + driverNumbers.Length + " spheres");
    }
}

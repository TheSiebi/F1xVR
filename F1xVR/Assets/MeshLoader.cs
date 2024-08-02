using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Reflection;

public class PrefabLoader : MonoBehaviour
{
    private string prefabName = MainMenu.mesh_name ; // Name of the prefab to load
    public Vector3 scale = new Vector3(1,1,1);
    public Vector3 position = new Vector3(0,0,0);

    private bool second_pass = false;
    private GameObject prefab;
    

    void Start()
    {
        // Load the prefab by name if it's not empty
        if (!string.IsNullOrEmpty(prefabName))
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(prefabName);

            // Check if there is exactly one prefab
            if (prefabs.Length == 1)
            {
                string map_name = prefabs[0].name;
                // prefab = Resources.Load<GameObject>(map_name);
                prefab = prefabs[0];
            }
            if (prefab != null)
            {
                GameObject game = new GameObject("Game");
                // game.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                GameObject instantiatedPrefab = Instantiate(prefab, game.transform);
                instantiatedPrefab.transform.rotation = Quaternion.Euler(-89.98f, 0f, 0f);
                instantiatedPrefab.transform.position += position;
                
                // Add a BoxCollider to the instantiated prefab
                BoxCollider collider = instantiatedPrefab.AddComponent<BoxCollider>();
                // Optionally, you can adjust the size of the collider
                collider.size = instantiatedPrefab.transform.rotation * instantiatedPrefab.GetComponent<Renderer>().bounds.size;

                instantiatedPrefab.transform.localScale = new Vector3(1,1,1);
                

                // Add a Rigidbody with gravity turned off
                Rigidbody rb = instantiatedPrefab.AddComponent<Rigidbody>();
                rb.useGravity = false;

                Grabbable grab = instantiatedPrefab.AddComponent<Grabbable>();

                HandGrabInteractable handGrabInteractable = instantiatedPrefab.AddComponent<HandGrabInteractable>();
                handGrabInteractable.InjectRigidbody(rb);
                handGrabInteractable.InjectOptionalPointableElement(grab);

                System.Type type = handGrabInteractable.GetType();
                FieldInfo field = type.GetField("_resetGrabOnGrabsUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(handGrabInteractable, false);
                FieldInfo handGrabInteractableField = type.GetField("_handAligment", BindingFlags.NonPublic | BindingFlags.Instance);
                handGrabInteractableField.SetValue(handGrabInteractable, HandAlignType.None);


                GrabInteractable grabIntractable = instantiatedPrefab.AddComponent<GrabInteractable>();
                System.Type grabIntractableType = grabIntractable.GetType();
                FieldInfo grabIntractableField = grabIntractableType.GetField("_resetGrabOnGrabsUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
                grabIntractableField.SetValue(grabIntractable, false);

                grabIntractable.InjectRigidbody(rb);
                grabIntractable.InjectOptionalPointableElement(grab);

                // TwoGrabFreeTransformer twoGrabFreeTransformer = instantiatedPrefab.AddComponent<TwoGrabFreeTransformer>();
                // System.Type twoGrabFreeTransformerType = twoGrabFreeTransformer.GetType();
                // FieldInfo twoGrabFreeTransformerField = twoGrabFreeTransformerType.GetField("_constraints", BindingFlags.NonPublic | BindingFlags.Instance);
                OneGrabFreeTransformer oneGrabFreeTransformer = instantiatedPrefab.AddComponent<OneGrabFreeTransformer>();
                System.Type oneGrabFreeTransformerType = oneGrabFreeTransformer.GetType();
                FieldInfo oneGrabFreeTransformerField = oneGrabFreeTransformerType.GetField("_rotationConstraints", BindingFlags.NonPublic | BindingFlags.Instance);
                // Create a new RotationConstraints object with specific constraints
                var rotationConstraints = new TransformerUtils.RotationConstraints()
                {
                    XAxis = new TransformerUtils.ConstrainedAxis
                    {
                        ConstrainAxis = true,
                        AxisRange = new TransformerUtils.FloatRange
                        {
                            Min = -90.0f,
                            Max = -90.0f
                        }
                    },
                    YAxis = new TransformerUtils.ConstrainedAxis
                    {
                        ConstrainAxis = true,
                        AxisRange = new TransformerUtils.FloatRange
                        {
                            Min = 0f,
                            Max = 0f
                        }
                    },
                    ZAxis = new TransformerUtils.ConstrainedAxis
                    {
                        ConstrainAxis = true,
                        AxisRange = new TransformerUtils.FloatRange
                        {
                            Min = 0f,
                            Max = 0f
                        }
                    }
                };
                var scaleConstraints = new Oculus.Interaction.TwoGrabFreeTransformer.TwoGrabFreeConstraints()
                {
                    ConstraintsAreRelative = false, // or true, depending on your requirements
                    MinScale = new Oculus.Interaction.FloatConstraint
                    {
                        Constrain = false,
                    },
                    MaxScale = new Oculus.Interaction.FloatConstraint
                    {
                        Constrain = false,
                    },
                    ConstrainXScale = false,
                    ConstrainYScale = false,
                    ConstrainZScale = false
                };

                // Assuming you have a twoGrabFreeTransformer object
                // twoGrabFreeTransformer.InjectOptionalConstraints(scaleConstraints);
                oneGrabFreeTransformerField.SetValue(oneGrabFreeTransformer,rotationConstraints );
                grab.InjectOptionalOneGrabTransformer(oneGrabFreeTransformer);
                // grab.InjectOptionalTwoGrabTransformer(twoGrabFreeTransformer);

                GameObject carprefab = Resources.Load<GameObject>("car_coloured");
                SmoothTrackingOffline cars = instantiatedPrefab.AddComponent<SmoothTrackingOffline>();

                cars.car_coloured = carprefab;
                cars.map = instantiatedPrefab;
                // game.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                
                        
            }
            else
            {
                Debug.LogError("Prefab '" + prefabName + "' not found in Resources folder!");
            }
        }
        else
        {
            Debug.LogError("Prefab name is empty!");
        }
    }

    void Update()
{
    if (!second_pass)
    {
        GameObject game = GameObject.Find("Game");
        if (game != null)
        {
            game.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
            // game.transform.position = Camera.main.transform.position  + new Vector3(0, -1, 1);
            Vector3 offset = new Vector3(0, -0.5f, 1);
            Transform cameraTransform = Camera.main.transform;
            game.transform.position = cameraTransform.TransformPoint(offset);
            GameObject instantiatedPrefab = game.transform.GetChild(0).gameObject;
            if (instantiatedPrefab != null)
            {
                BoxCollider collider = instantiatedPrefab.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.size = collider.size/100;
                    
                }
            }
            second_pass = true;
        }
    }
}
}
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DrawLinesOnMesh : MonoBehaviour
{
    private Camera mainCamera;
    private List<Vector3> points = new List<Vector3>();
    private string csvFilePath;

    void Start()
    {
        mainCamera = Camera.main;
        string directoryPath = Application.dataPath + "/CSV";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        csvFilePath = Path.Combine(directoryPath, "points.csv");
        Debug.Log("CSV file path: " + csvFilePath);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // The ray hit a collider
            HandleRaycastHit(hit);
        }
    }

    void HandleRaycastHit(RaycastHit hit)
    {
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider != null)
        {
            // The ray hit a mesh collider
            AddPointOnMesh(hit.point, meshCollider.sharedMesh);
        }
    }

    void AddPointOnMesh(Vector3 point, Mesh mesh)
    {
        points.Add(point);
        SavePointToCSV(point);

        if (points.Count > 1)
        {
            DrawLine(points[points.Count - 2], points[points.Count - 1], mesh);
        }
    }

    void DrawLine(Vector3 start, Vector3 end, Mesh mesh)
    {
        GameObject lineObject = new GameObject("Line");
        lineObject.transform.position = start;
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        // Configure the line renderer's material, width, and other properties as desired
    }

    void SavePointToCSV(Vector3 point)
    {
        string pointData = $"{point.x},{point.y},{point.z}";
        File.AppendAllText(csvFilePath, pointData + "\n");
    }
}
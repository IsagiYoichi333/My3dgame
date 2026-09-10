using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Tube.Color[] colors = { Tube.Color.Red, Tube.Color.Blue, Tube.Color.Green, Tube.Color.Yellow, Tube.Color.Purple, Tube.Color.Orange };
    public int tubeCapacity = 4;
    public int numberOfTubes = 8; // Total tubes (including empty ones)
    public int numberOfColors = 4; // Number of distinct colors to use

    public GameObject tubePrefab; // Assign in inspector
    public Transform tubesParent; // Assign in inspector (optional, can be null)

    // Grid settings for tube placement
    public int columns = 4; // Number of columns in the grid
    public float tubeSpacing = 2.0f; // Distance between tubes

    private List<Tube> tubes = new List<Tube>();
    private Tube selectedTube = null;

    void Start()
    {
        // Set up camera for 2D orthographic view
        SetupCamera();
        SetupLevel();
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            // Adjust orthographic size based on grid to see all tubes
            // We'll set it later after we know the grid dimensions in SetupLevel
        }
    }

    void SetupLevel()
    {
        // Clear any existing tubes
        if (tubesParent != null)
        {
            foreach (Transform child in tubesParent)
            {
                Destroy(child.gameObject);
            }
        }
        tubes.Clear();

        // Create the tubes
        for (int i = 0; i < numberOfTubes; i++)
        {
            GameObject tubeObj = Instantiate(tubePrefab);
            Tube tube = tubeObj.GetComponent<Tube>();
            if (tube == null)
            {
                tube = tubeObj.AddComponent<Tube>();
            }
            tubes.Add(tube);

            // Parent to tubesParent if provided
            if (tubesParent != null)
            {
                tubeObj.transform.SetParent(tubesParent);
            }
        }

        // Prepare the color list for this level
        List<Tube.Color> availableColors = new List<Tube.Color>();
        for (int i = 0; i < numberOfColors; i++)
        {
            availableColors.Add(colors[i]);
        }

        // Shuffle the colors
        for (int i = 0; i < availableColors.Count; i++)
        {
            Tube.Color temp = availableColors[i];
            int randomIndex = Random.Range(i, availableColors.Count);
            availableColors[i] = availableColors[randomIndex];
            availableColors[randomIndex] = temp;
        }

        // Assign colors to tubes (each color appears in tubeCapacity segments, distributed across tubes)
        List<Tube.Color> colorSegments = new List<Tube.Color>();
        foreach (Tube.Color color in availableColors)
        {
            for (int j = 0; j < tubeCapacity; j++)
            {
                colorSegments.Add(color);
            }
        }

        // Shuffle the segments
        for (int i = 0; i < colorSegments.Count; i++)
        {
            Tube.Color temp = colorSegments[i];
            int randomIndex = Random.Range(i, colorSegments.Count);
            colorSegments[i] = colorSegments[randomIndex];
            colorSegments[randomIndex] = temp;
        }

        // Fill the tubes (leave last two tubes empty for gameplay)
        int segmentIndex = 0;
        for (int i = 0; i < tubes.Count - 2; i++) // Leave last two tubes empty
        {
            List<Tube.Color> tubeSegments = new List<Tube.Color>();
            for (int j = 0; j < tubeCapacity; j++)
            {
                if (segmentIndex < colorSegments.Count)
                {
                    tubeSegments.Add(colorSegments[segmentIndex]);
                    segmentIndex++;
                }
                else
                {
                    tubeSegments.Add(Tube.Color.None); // Should not happen if we have enough segments
                }
            }
            tubes[i].Initialize(tubeSegments);
        }

        // The remaining tubes are already empty (initialized with empty list)

        // Now position the tubes in a grid
        PositionTubesInGrid();

        // Adjust camera to see all tubes
        AdjustCamera();
    }

    void PositionTubesInGrid()
    {
        if (tubes.Count == 0) return;

        float startX = -((columns - 1) * tubeSpacing) / 2f;
        float startY = 0f; // We'll place tubes along x and y (z=0)

        int row = 0;
        int col = 0;
        for (int i = 0; i < tubes.Count; i++)
        {
            float x = startX + col * tubeSpacing;
            float y = startY - row * tubeSpacing; // Negative so first row is at top
            tubes[i].transform.position = new Vector3(x, y, 0f);

            col++;
            if col >= columns
            {
                col = 0;
                row++;
            }
        }
    }

    void AdjustCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Calculate bounds of tubes
        float minX = Mathf.Infinity, maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity, maxY = -Mathf.Infinity;
        foreach (Tube tube in tubes)
        {
            Vector3 pos = tube.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        // Add some padding
        float padding = 2f;
        float width = (maxX - minX) + padding;
        float height = (maxY - minY) + padding;

        // Set orthographic size to see the height (with some margin)
        // Orthographic size is half the vertical size visible
        mainCamera.orthographicSize = height / 2f + 1f; // Extra 1 unit margin

        // Center the camera
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, mainCamera.transform.position.z);
        mainCamera.transform.position = center;
    }

    void Update()
    {
        // Handle mouse click to select tubes
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Tube tube = hit.collider.GetComponent<Tube>();
                if (tube != null)
                {
                    if (selectedTube == null)
                    {
                        selectedTube = tube;
                        // Highlight selected tube (you can add visual feedback here)
                    }
                    else
                    {
                        // Try to pour from selectedTube to this tube
                        int poured = selectedTube.PourTo(tube);
                        if (poured > 0)
                        {
                            // Successful pour
                        }
                        else
                        {
                            // Invalid pour (maybe play error sound)
                        }
                        selectedTube = null;
                        // Clear highlight
                    }
                }
            }
        }
    }

    // Check if the player has won (each tube is either empty or full of a single color)
    public bool CheckWinCondition()
    {
        foreach (Tube tube in tubes)
        {
            if (!tube.IsEmpty() && !tube.IsFull())
            {
                return false;
            }
            if (!tube.IsEmpty())
            {
                Tube.Color topColor = tube.GetTopColor();
                // Check if all segments in the tube are the same color
                foreach (Tube.Color color in tube.segments)
                {
                    if (color != topColor)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}
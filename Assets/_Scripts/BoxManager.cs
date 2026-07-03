using UnityEngine;
using UnityEngine.UI;

public class BoxManager : Manager // Changed from Manager to MonoBehaviour (revert if you have a custom Manager base class)
{
    [Header("UI References")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject rowPrefab;

    [Header("Visual Settings")]
    [Tooltip("Distinct pastel colors for up to 12 regions.")]
    [SerializeField]
    private Color[] regionColors = new Color[]
    {
        new Color(0.95f, 0.65f, 0.65f), // Pastel Red
        new Color(0.65f, 0.85f, 0.95f), // Pastel Blue
        new Color(0.65f, 0.95f, 0.75f), // Pastel Green
        new Color(0.95f, 0.90f, 0.60f), // Pastel Yellow
        new Color(0.85f, 0.65f, 0.95f), // Pastel Purple
        new Color(0.95f, 0.75f, 0.60f), // Pastel Orange
        new Color(0.60f, 0.90f, 0.90f), // Pastel Cyan
        new Color(0.90f, 0.60f, 0.80f), // Pastel Pink
        new Color(0.75f, 0.85f, 0.65f), // Pastel Lime
        new Color(0.80f, 0.75f, 0.90f)   // Pastel Lavender
    };

    /// <summary>
    /// Generates the visual UI grid based on the Meowdoku region matrix.
    /// </summary>
    public bool GenerateBoxFromMatrix(int[,] matrix)
    {
        if (matrix == null || matrix.Length == 0)
        {
            Debug.LogError("Matrix is null or empty.");
            return false;
        }

        // 1. Clear existing rows and boxes
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        // 2. Instantiate the grid
        for (int i = 0; i < rows; i++)
        {
            GameObject row = Instantiate(rowPrefab, content);
            row.name = $"Row_{i}";

            for (int j = 0; j < cols; j++)
            {
                GameObject boxObj = Instantiate(boxPrefab, row.transform);
                boxObj.name = $"Box_{i}_{j}";

                int regionId = matrix[i, j];

                // Determine border highlights by comparing with orthogonal neighbors
                bool borderTop = (i == 0) || (matrix[i - 1, j] != regionId);
                bool borderBottom = (i == rows - 1) || (matrix[i + 1, j] != regionId);
                bool borderLeft = (j == 0) || (matrix[i, j - 1] != regionId);
                bool borderRight = (j == cols - 1) || (matrix[i, j + 1] != regionId);

                // 3. Apply Visuals & Data to the Box
                BoxController box = boxObj.GetComponent<BoxController>();
                if (box != null)
                {
                    Color regionColor = GetRegionColor(regionId);
                    box.Initialize(i, j, regionId, regionColor);
                }
                else
                {
                    // Fallback: If no BoxController script exists, just try setting the UI Image color directly
                    Image img = boxObj.GetComponent<Image>();
                    if (img != null) img.color = GetRegionColor(regionId);
                }
            }
        }

        return true;
    }

    private Color GetRegionColor(int regionId)
    {
        if (regionId >= 0 && regionId < regionColors.Length)
        {
            return regionColors[regionId];
        }
        // Fallback color dynamically generated if regionId exceeds array length
        return Color.HSVToRGB((regionId * 0.15f) % 1f, 0.4f, 0.95f);
    }
}
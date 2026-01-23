using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridSize = new Vector3(1f, 1f, 0.5f);
    public int gridWidth = 10;
    public int gridDepth = 10;

    [Header("Visual Settings")]
    public Color gridColor = new Color(1, 1, 1, 0.3f);
    public Color centerColor = Color.green;
    public bool showGrid = true;

    void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;

        // Gambar grid horizontal (X-Z plane)
        for (int x = -gridWidth; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(x * gridSize.x, 0, -gridDepth * gridSize.z);
            Vector3 end = new Vector3(x * gridSize.x, 0, gridDepth * gridSize.z);
            Gizmos.DrawLine(start, end);
        }

        for (int z = -gridDepth; z <= gridDepth; z++)
        {
            Vector3 start = new Vector3(-gridWidth * gridSize.x, 0, z * gridSize.z);
            Vector3 end = new Vector3(gridWidth * gridSize.x, 0, z * gridSize.z);
            Gizmos.DrawLine(start, end);
        }

        // Tanda center (0,0,0)
        Gizmos.color = centerColor;
        Gizmos.DrawSphere(Vector3.zero, 0.1f);

        // Tulis label grid coordinates
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 8;

        for (int x = -gridWidth; x <= gridWidth; x++)
        {
            for (int z = -gridDepth; z <= gridDepth; z++)
            {
                Vector3 pos = new Vector3(x * gridSize.x, 0.1f, z * gridSize.z);
                UnityEditor.Handles.Label(pos, $"({x},{z})", style);
            }
        }
#endif
    }

    // Method untuk konversi world pos ke grid pos
    public Vector3Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPosition.x / gridSize.x),
            Mathf.RoundToInt(worldPosition.y / gridSize.y),
            Mathf.RoundToInt(worldPosition.z / gridSize.z)
        );
    }

    // Method untuk konversi grid pos ke world pos
    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * gridSize.x,
            gridPosition.y * gridSize.y,
            gridPosition.z * gridSize.z
        );
    }
}
using UnityEngine;

public class BrickGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridSize = new Vector3(1f, 1f, 0.5f);
    public bool autoSnap = true;

    void Start()
    {
        if (autoSnap)
            SnapToGrid();
    }

    [ContextMenu("Snap to Grid")]
    void SnapToGrid()
    {
        Vector3 pos = transform.position;

        // Snap ke grid
        pos.x = Mathf.Round(pos.x / gridSize.x) * gridSize.x;
        pos.y = Mathf.Round(pos.y / gridSize.y) * gridSize.y;
        pos.z = Mathf.Round(pos.z / gridSize.z) * gridSize.z;

        transform.position = pos;

    }

    // Method untuk mendapatkan posisi grid brick
    public Vector3Int GetGridPosition()
    {
        Vector3 pos = transform.position;
        return new Vector3Int(
            Mathf.RoundToInt(pos.x / gridSize.x),
            Mathf.RoundToInt(pos.y / gridSize.y),
            Mathf.RoundToInt(pos.z / gridSize.z)
        );
    }

    // Visual debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Orange transparan
        Gizmos.DrawWireCube(transform.position, gridSize);

        // Tampilkan posisi grid
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 10;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.6f,
            $"Grid: {GetGridPosition()}",
            style
        );
#endif
    }
}
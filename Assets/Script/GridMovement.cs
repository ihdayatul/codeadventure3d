using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridSize = new Vector3(2f, 1f, 2f); // Sesuai ukuran bata
    public float moveDuration = 0.5f;
    public float rotationDuration = 0.3f;

    [Header("Animation")]
    public Animator animator;

    // Posisi grid saat ini
    private Vector3Int currentGridPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;

    void Start()
    {
        // Snap ke grid terdekat saat start
        SnapToGrid();

        // Auto-find animator
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void SnapToGrid()
    {
        Vector3 worldPos = transform.position;

        // Hitung posisi grid
        currentGridPosition = new Vector3Int(
            Mathf.RoundToInt(worldPos.x / gridSize.x),
            Mathf.RoundToInt(worldPos.y / gridSize.y),
            Mathf.RoundToInt(worldPos.z / gridSize.z)
        );

        // Update posisi ke grid
        transform.position = new Vector3(
            currentGridPosition.x * gridSize.x,
            currentGridPosition.y * gridSize.y,
            currentGridPosition.z * gridSize.z
        );

        // Snap rotasi ke 90 derajat
        float currentYRotation = Mathf.Round(transform.eulerAngles.y / 90) * 90;
        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);

        Debug.Log($"Player snapped to grid: {currentGridPosition}");
    }

    public void MoveForward()
    {
        if (isMoving) return;

        // Hitung posisi baru berdasarkan arah hadap
        Vector3Int moveDirection = GetForwardDirection();
        Vector3Int targetGridPos = currentGridPosition + moveDirection;

        StartCoroutine(MoveToGridPosition(targetGridPos));
    }

    Vector3Int GetForwardDirection()
    {
        float angle = transform.eulerAngles.y;

        // Konversi derajat ke arah grid
        if (Mathf.Approximately(angle, 0f))      // Menghadap +Z
            return new Vector3Int(0, 0, 1);
        else if (Mathf.Approximately(angle, 90f))  // Menghadap +X
            return new Vector3Int(1, 0, 0);
        else if (Mathf.Approximately(angle, 180f)) // Menghadap -Z
            return new Vector3Int(0, 0, -1);
        else if (Mathf.Approximately(angle, 270f)) // Menghadap -X
            return new Vector3Int(-1, 0, 0);
        else
            return new Vector3Int(0, 0, 1); // Default
    }

    IEnumerator MoveToGridPosition(Vector3Int targetGridPos)
    {
        isMoving = true;

        // Set animasi jalan
        if (animator != null)
            animator.SetBool("IsWalking", true);

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(
            targetGridPos.x * gridSize.x,
            targetGridPos.y * gridSize.y,
            targetGridPos.z * gridSize.z
        );

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;

            // Smooth movement dengan easing
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Pastikan posisi tepat
        transform.position = targetPos;
        currentGridPosition = targetGridPos;

        // Stop animasi
        if (animator != null)
            animator.SetBool("IsWalking", false);

        isMoving = false;

        Debug.Log($"Moved to grid: {currentGridPosition}");
    }

    public void RotateLeft()
    {
        if (isMoving) return;

        StartCoroutine(RotateCoroutine(-90f));
    }

    public void RotateRight()
    {
        if (isMoving) return;

        StartCoroutine(RotateCoroutine(90f));
    }

    IEnumerator RotateCoroutine(float angle)
    {
        isMoving = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, startRot.eulerAngles.y + angle, 0);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
        isMoving = false;

        Debug.Log($"Rotated to: {transform.eulerAngles.y}°");
    }

    // Method untuk blok kode
    public void ExecuteMoveForward()
    {
        MoveForward();
    }

    public void ExecuteRotateLeft()
    {
        RotateLeft();
    }

    public void ExecuteRotateRight()
    {
        RotateRight();
    }

    // Visual debug grid
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            currentGridPosition.x * gridSize.x,
            currentGridPosition.y * gridSize.y + 0.5f,
            currentGridPosition.z * gridSize.z
        );

        Gizmos.DrawWireCube(center, new Vector3(gridSize.x, 0.1f, gridSize.z));

        // Draw forward direction
        Gizmos.color = Color.red;
        Vector3 forwardPos = center + transform.forward * gridSize.z;
        Gizmos.DrawLine(center, forwardPos);
        Gizmos.DrawSphere(forwardPos, 0.1f);
    }
}
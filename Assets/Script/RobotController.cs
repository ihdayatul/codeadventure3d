using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 120f;
    public float moveDistance = 1f; // Jarak per gerakan

    [Header("Animation")]
    public Animator animator;

    // Queue untuk menyimpan perintah
    private Queue<string> commandQueue = new Queue<string>();
    private bool isExecuting = false;

    void Start()
    {
        // Cari animator jika belum diassign
        if (animator == null)
            animator = GetComponent<Animator>();
    }

   
    public void MoveForward()
    {
        commandQueue.Enqueue("MOVE");
        if (!isExecuting)
            StartCoroutine(ProcessCommands());
    }

    public void RotateLeft()
    {
        commandQueue.Enqueue("ROTATE_LEFT");
        if (!isExecuting)
            StartCoroutine(ProcessCommands());
    }

    public void RotateRight()
    {
        commandQueue.Enqueue("ROTATE_RIGHT");
        if (!isExecuting)
            StartCoroutine(ProcessCommands());
    }

    IEnumerator ProcessCommands()
    {
        isExecuting = true;

        while (commandQueue.Count > 0)
        {
            string command = commandQueue.Dequeue();

            switch (command)
            {
                case "MOVE":
                    yield return StartCoroutine(MoveOneStep());
                    break;

                case "ROTATE_LEFT":
                    yield return StartCoroutine(RotateBy(-90f));
                    break;

                case "ROTATE_RIGHT":
                    yield return StartCoroutine(RotateBy(90f));
                    break;
            }

            // Delay antar perintah
            yield return new WaitForSeconds(0.1f);
        }

        isExecuting = false;
    }

    IEnumerator MoveOneStep()
    {
        // Set animasi jalan
        if (animator != null)
            animator.SetBool("IsWalking", true);

        // Hitung posisi target berdasarkan arah
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * moveDistance;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Gunakan SmoothStep untuk pergerakan lebih mulus
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Pastikan posisi tepat
        transform.position = targetPos;

        // Stop animasi
        if (animator != null)
            animator.SetBool("IsWalking", false);

        Debug.Log($"Moved to: {transform.position}");
    }

    IEnumerator RotateBy(float angle)
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, startRot.eulerAngles.y + angle, 0);

        float duration = Mathf.Abs(angle) / rotationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        Debug.Log($"Rotated to: {transform.eulerAngles.y}°");
    }

    public void ClearCommands()
    {
        commandQueue.Clear();
        StopAllCoroutines();
        isExecuting = false;

        if (animator != null)
            animator.SetBool("IsWalking", false);
    }

    // Method untuk mendapatkan posisi grid (opsional, jika masih dibutuhkan)
    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 120f;
    public float moveDistance = 1f; // Jarak per gerakan

    [Header("Falling Settings")]
    public float fallSpeed = 5f; // Kecepatan jatuh
    public float fallDistance = 10f; // Jarak jatuh maksimum
    public LayerMask groundLayer; // Layer untuk tile/dasar
    public float groundCheckOffset = 0.1f; // Offset untuk raycast
    public float groundCheckDistance = 0.2f; // Jarak pengecekan ground

    

    [Header("Animation")]
    public Animator animator;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Queue untuk menyimpan perintah
    private Queue<string> commandQueue = new Queue<string>();
    private bool isExecuting = false;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    public void Restart()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }
   
    public IEnumerator MoveForward()
    {
         commandQueue.Enqueue("MOVE");
        if (!isExecuting)
            yield return StartCoroutine(ProcessCommands());
    }



    public IEnumerator RotateLeft()
    {
        commandQueue.Enqueue("ROTATE_LEFT");
        if (!isExecuting)
           yield return StartCoroutine(ProcessCommands());
    }

    public IEnumerator RotateRight()
    {
        commandQueue.Enqueue("ROTATE_RIGHT");
        if (!isExecuting)
            yield return StartCoroutine(ProcessCommands());
    }

    public IEnumerator Stop()
    {
        commandQueue.Enqueue("STOP");
        if (!isExecuting)
            yield return StartCoroutine(ProcessCommands());
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
                    audioManager.PlaySFX(audioManager.Walk);
                    yield return StartCoroutine(MoveOneStep());
                    break;

                case "ROTATE_LEFT":
                    yield return StartCoroutine(RotateBy(-90f));
                    break;

                case "ROTATE_RIGHT":
                    yield return StartCoroutine(RotateBy(90f));
                    break;

                case "STOP":
                    yield return StartCoroutine(StopMove());
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

        Debug.Log($"Moved to: {transform.position}");
    }

    IEnumerator RotateBy(float angle)
    {
        if (animator != null)
            animator.SetBool("IsWalking", false);

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

    IEnumerator StopMove()
    {
        if (animator != null)
            animator.SetBool("IsWalking", false);

        yield return null;
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
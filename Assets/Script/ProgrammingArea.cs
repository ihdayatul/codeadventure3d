using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProgrammingArea : MonoBehaviour, IDropHandler
{
    [Header("References")]
    public RobotController robot;
    public Transform codeBlocksPanel;

    [Header("Settings")]
    public List<CodeBlock> codeBlocks = new List<CodeBlock>();
    public float executionDelay = 0.5f;
    public int maxBlocks = 10;

    [Header("Visual")]
    public Color validDropColor = new Color(0, 1, 0, 0.3f);
    public Color invalidDropColor = new Color(1, 0, 0, 0.3f);

    // HAPUS baris-baris berikut (event tidak digunakan):
    // public delegate void ProgramComplete();
    // public event ProgramComplete OnProgramComplete;

    private Dictionary<CodeBlock, Transform> blockOriginalParents = new Dictionary<CodeBlock, Transform>();
    private bool isExecuting = false;
    private Image areaImage;

    void Start()
    {
        areaImage = GetComponent<Image>();

        if (codeBlocksPanel == null)
        {
            GameObject panel = GameObject.Find("CodeBlocksPanel");
            if (panel != null)
                codeBlocksPanel = panel.transform;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock codeBlock = dropped.GetComponent<CodeBlock>();
        if (codeBlock != null)
        {
            // Cek batas maksimum
            if (codeBlocks.Count >= maxBlocks)
            {
                Debug.LogWarning($"Maksimum {maxBlocks} blok tercapai!");
                StartCoroutine(ShowInvalidDropFeedback());
                ReturnBlockToPanel(codeBlock);
                return;
            }

            // Simpan parent asli
            if (!blockOriginalParents.ContainsKey(codeBlock))
            {
                blockOriginalParents[codeBlock] = codeBlock.transform.parent;
            }

            // Tambahkan ke list
            if (!codeBlocks.Contains(codeBlock))
            {
                codeBlocks.Add(codeBlock);
                codeBlock.transform.SetParent(transform);

                // Reset posisi
                RectTransform rt = codeBlock.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(150, 80);

                Debug.Log($"Blok ditambahkan: {codeBlock.blockType} (Total: {codeBlocks.Count})");

                StartCoroutine(ShowValidDropFeedback());
            }
        }
    }

    public void ExecuteProgram()
    {
        if (isExecuting)
            return;

        if (robot == null || codeBlocks.Count == 0)
            return;

        StartCoroutine(ExecuteProgramCoroutine());
    }

    private IEnumerator ExecuteProgramCoroutine()
    {
        isExecuting = true;

        foreach (CodeBlock block in codeBlocks)
        {
            ExecuteBlockCommand(block);
            yield return new WaitForSeconds(executionDelay);
        }

        isExecuting = false;

        // PROGRAM SELESAI DI SINI (BUKAN DI AWAL)
        GameManager.instance.isProgramFinished = true;
        GameManager.instance.CheckWinCondition();
    }


    private void ExecuteBlockCommand(CodeBlock block)
    {
        switch (block.blockType.ToLower())
        {
            case "move":
            case "forward":
            case "maju":
                Debug.Log("Eksekusi: MoveForward");
                robot.MoveForward();
                break;

            case "turn_left":
            case "rotate_left":
            case "kiri":
                Debug.Log("Eksekusi: RotateLeft");
                robot.RotateLeft();
                break;

            case "turn_right":
            case "rotate_right":
            case "kanan":
                Debug.Log("Eksekusi: RotateRight");
                robot.RotateRight();
                break;
        }
    }

    public void ClearProgram()
    {
        // Hentikan eksekusi
        if (isExecuting)
        {
            StopAllCoroutines();
            isExecuting = false;
        }

        // Reset robot jika ada
        if (robot != null)
        {
            // Coba panggil ClearCommands atau ResetPosition
            RobotController robotCtrl = robot.GetComponent<RobotController>();
            if (robotCtrl != null)
            {
                robotCtrl.ClearCommands();
            }

            GridMovement gridMove = robot.GetComponent<GridMovement>();
            if (gridMove != null)
            {
                // Tambahkan method reset di GridMovement jika perlu
            }
        }

        // Kembalikan semua blok ke panel asal atau destroy jika duplikat
        foreach (CodeBlock block in codeBlocks.ToArray()) // Gunakan ToArray untuk menghindari modifikasi selama iterasi
        {
            if (block != null)
            {
                // Jika blok adalah duplikat (bukan template), hancurkan
                if (!block.isTemplate && block.originalTemplate != null)
                {
                    Destroy(block.gameObject);
                }
                else
                {
                    ReturnBlockToPanel(block);
                }
            }
        }

        // Kosongkan collections
        codeBlocks.Clear();
        blockOriginalParents.Clear();

        Debug.Log("Program dibersihkan!");
    }

    void ReturnBlockToPanel(CodeBlock block)
    {
        if (blockOriginalParents.ContainsKey(block))
        {
            block.transform.SetParent(blockOriginalParents[block]);
            block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else if (codeBlocksPanel != null)
        {
            block.transform.SetParent(codeBlocksPanel);
            block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
    
    private IEnumerator ShowValidDropFeedback()
    {
        if (areaImage != null)
        {
            Color originalColor = areaImage.color;
            areaImage.color = validDropColor;
            yield return new WaitForSeconds(0.3f);
            areaImage.color = originalColor;
        }
    }

    private IEnumerator ShowInvalidDropFeedback()
    {
        if (areaImage != null)
        {
            Color originalColor = areaImage.color;
            areaImage.color = invalidDropColor;
            yield return new WaitForSeconds(0.5f);
            areaImage.color = originalColor;
        }
    }
}
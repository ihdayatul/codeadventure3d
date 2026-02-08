using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ProgrammingArea : MonoBehaviour, IDropHandler
{
    [Header("References")]
    public RobotController robot;
    public Transform codeBlocksPanel;
    public Transform codeBlockContainer;

    [Header("Settings")]
    public List<CodeBlock> codeBlocks = new List<CodeBlock>();
    public int maxBlocks = 10;

    [Header("Visual")]
    public Color validDropColor = new Color(0, 1, 0, 0.3f);
    public Color invalidDropColor = new Color(1, 0, 0, 0.3f);

    [Header("UI")]
    public Button runButton;
    public Button restartButton;

    // HAPUS baris-baris berikut (event tidak digunakan):
    // public delegate void ProgramComplete();
    // public event ProgramComplete OnProgramComplete;

    private Dictionary<CodeBlock, Transform> blockOriginalParents = new Dictionary<CodeBlock, Transform>();
    private bool isExecuting = false;
    private Image areaImage;

    void Start()
    {
        areaImage = GetComponent<Image>();

        runButton.enabled = false;
        runButton.gameObject.SetActive(true);

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
                codeBlock.transform.SetParent(codeBlockContainer);

                // Reset posisi
                RectTransform rt = codeBlock.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(150, 80);

                //Debug.Log($"Blok ditambahkan: {codeBlock.blockType} (Total: {codeBlocks.Count})");

                runButton.enabled = codeBlocks.Count > 0;

                StartCoroutine(ShowValidDropFeedback());
            }
        }
    }
    
    public void RestartCharacter()
    {
        restartButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(true);
        runButton.enabled = codeBlocks.Count > 0;
        robot.Restart();
        GameManager.instance.ResetData();
    }

    public void ExecuteProgram()
    {
        if (isExecuting)
            return;

        if (robot == null || codeBlocks.Count == 0)
            return;
        //if (GameManager.instance.isPlayerFallen)
           // return;

        runButton.enabled = false;

        StartCoroutine(ExecuteProgramCoroutine());
    }

    private IEnumerator ExecuteProgramCoroutine()
    {
        isExecuting = true;

        foreach (CodeBlock block in codeBlocks)
        {
            yield return StartCoroutine(ExecuteBlockCommandCoroutine(block));
           // STOP TOTAL JIKA JATUH
        if (GameManager.instance.isPlayerFallen)
            {
                Debug.Log("Eksekusi dihentikan: Player jatuh");
                isExecuting = false;
                break;
            }
        }

        yield return StartCoroutine(robot.Stop());

        isExecuting = false;

        ShowRestartButton(true);

        // PROGRAM SELESAI DI SINI (BUKAN DI AWAL)
        GameManager.instance.isProgramFinished = true;
        GameManager.instance.CheckWinCondition();
    }

    private void ShowRestartButton(bool show)
    {
        runButton.gameObject.SetActive(!show);
        restartButton.gameObject.SetActive(show);
    }


    private IEnumerator ExecuteBlockCommandCoroutine(CodeBlock block)
    {

        switch (block.blockType.ToLower())
        {
            case "move":
                Debug.Log("Eksekusi: MoveForward");
                yield return StartCoroutine(robot.MoveForward());
                break;

            case "rotate_left":
                Debug.Log("Eksekusi: RotateLeft");
                yield return StartCoroutine(robot.RotateLeft());
                break;

            case "rotate_right":
                Debug.Log("Eksekusi: RotateRight");
                yield return StartCoroutine(robot.RotateRight());
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CodeBlock;  // Agar bisa langsung pakai BlockType

public class ProgrammingArea : MonoBehaviour, IDropHandler
{
    [Header("References")]
    public RobotController robot;
    public Transform codeBlocksPanel;
    public RectTransform codeBlockContainer;

    [Header("Settings")]
    public List<CodeBlock> codeBlocks = new List<CodeBlock>();
    public int maxBlocks = 10;

    [Header("Visual")]
    public Color validDropColor = new Color(0, 1, 0, 0.3f);
    public Color invalidDropColor = new Color(1, 0, 0, 0.3f);

    [Header("UI")]
    public Button runButton;
    public Button restartButton;

    private Dictionary<CodeBlock, Transform> blockOriginalParents = new Dictionary<CodeBlock, Transform>();
    private bool isExecuting = false;
    private Image areaImage;

    // ============= PROPERTIES =============
    public int BlocksCount
    {
        get
        {
            var count = codeBlocks.Count;
            var loopBlocks = codeBlockContainer.GetComponentsInChildren<LoopBlock>();
            foreach (var loop in loopBlocks)
            {
                count += loop.CodeBlocks.Count;
            }
            return count;
        }
    }

    // ============= UNITY LIFECYCLE =============
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

    // ============= HANDLE DROP =============
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock codeBlock = dropped.GetComponent<CodeBlock>();
        if (codeBlock == null) return;

        // Cegah duplikat di list
        if (codeBlocks.Contains(codeBlock))
            return;

        // Cek batas maksimum blok
        if (codeBlocks.Count >= maxBlocks)
        {
            Debug.LogWarning($"Maksimum {maxBlocks} blok tercapai!");
            StartCoroutine(ShowInvalidDropFeedback());
            ReturnBlockToPanel(codeBlock);
            return;
        }

        // Simpan parent asli (untuk dikembalikan nanti)
        if (!blockOriginalParents.ContainsKey(codeBlock))
        {
            blockOriginalParents[codeBlock] = codeBlock.transform.parent;
        }

        // Tambahkan ke list program
        codeBlocks.Add(codeBlock);
        codeBlock.transform.SetParent(codeBlockContainer);

        // Reset posisi dan ukuran
        RectTransform rt = codeBlock.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(150, 80);
        rt.localScale = Vector3.one;

        // ============ HANDLE JENIS BLOK KHUSUS ============

        // --- BLOK DEFINISI FUNGSI ---
        if (codeBlock.blockType == BlockType.FunctionDefinition)
        {
            FunctionDefinitionBlock funcDef = codeBlock.GetComponent<FunctionDefinitionBlock>();
            if (funcDef != null)
            {
                funcDef.isTemplate = false;
                // Daftarkan ke FunctionManager
                if (FunctionManager.Instance != null)
                    FunctionManager.Instance.RegisterFunction(funcDef);
                Debug.Log($"📦 Fungsi '{funcDef.functionName}' ditambahkan ke program.");
            }
        }
        // --- BLOK PANGGIL FUNGSI ---
        else if (codeBlock.blockType == BlockType.FunctionCall)
        {
            FunctionCallBlock funcCall = codeBlock.GetComponent<FunctionCallBlock>();
            if (funcCall != null)
            {
                funcCall.isTemplate = false;
                // Refresh dropdown setelah 1 frame (agar FunctionManager siap)
                StartCoroutine(DelayedRefreshDropdown(funcCall));
            }
        }

        // Update UI
        runButton.enabled = codeBlocks.Count > 0;
        RefreshLayout();
        StartCoroutine(ShowValidDropFeedback());

        Debug.Log($"✅ Blok {codeBlock.blockType} ditambahkan. Total: {codeBlocks.Count}");
    }

    // Helper untuk refresh dropdown setelah 1 frame
    private IEnumerator DelayedRefreshDropdown(FunctionCallBlock funcCall)
    {
        yield return null;
        if (funcCall != null)
            funcCall.RefreshDropdown();
    }

    // ============= REFRESH LAYOUT =============
    public void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(codeBlockContainer);
    }

    // ============= RESTART CHARACTER =============
    public void RestartCharacter()
    {
        restartButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(true);
        runButton.enabled = codeBlocks.Count > 0;
        robot.Restart();
        GameManager.instance.ResetData();
    }

    // ============= EKSEKUSI PROGRAM =============
    public void ExecuteProgram()
    {
        if (isExecuting) return;
        if (robot == null || codeBlocks.Count == 0) return;

        runButton.enabled = false;
        StartCoroutine(ExecuteProgramCoroutine());
    }

    private IEnumerator ExecuteProgramCoroutine()
    {
        isExecuting = true;

        foreach (CodeBlock block in codeBlocks)
        {
            yield return StartCoroutine(ExecuteBlockCommandCoroutine(block));

            if (GameManager.instance.isPlayerFallen)
            {
                Debug.Log("⛔ Eksekusi dihentikan: Player jatuh");
                isExecuting = false;
                break;
            }
        }

        yield return StartCoroutine(robot.Stop());

        isExecuting = false;
        ShowRestartButton(true);

        GameManager.instance.isProgramFinished = true;
        GameManager.instance.CheckWinCondition();
    }

    // Eksekusi per blok (ENUM)
    private IEnumerator ExecuteBlockCommandCoroutine(CodeBlock block)
    {
        switch (block.blockType)
        {
            case BlockType.Move:
                yield return robot.MoveForward();
                break;

            case BlockType.RotateLeft:
                yield return robot.RotateLeft();
                break;

            case BlockType.RotateRight:
                yield return robot.RotateRight();
                break;

            case BlockType.Loop:
                // ✅ EKSEKUSI LOOP
                LoopBlock loop = block.GetComponent<LoopBlock>();
                if (loop != null)
                {
                    yield return loop.ExecuteLoop(robot);        // ✅ langsung panggil method
                }
                else
                {
                    Debug.LogWarning("LoopBlock tidak memiliki method ExecuteLoop atau komponen tidak ditemukan.");
                }
                break;

            case BlockType.FunctionCall:
                // ✅ EKSEKUSI PANGGIL FUNGSI
                FunctionCallBlock call = block.GetComponent<FunctionCallBlock>();
                if (call != null)
                    yield return call.Execute(robot);
                else
                    Debug.LogWarning("FunctionCallBlock tidak ditemukan.");
                break;

            // FunctionDefinition tidak dieksekusi
            case BlockType.FunctionDefinition:
                Debug.Log("ℹ️ Blok definisi fungsi diabaikan saat eksekusi.");
                break;
        }
    }

    // ============= TAMPILAN TOMBOL RESTART =============
    private void ShowRestartButton(bool show)
    {
        runButton.gameObject.SetActive(!show);
        restartButton.gameObject.SetActive(show);
    }

    // ============= CLEAR PROGRAM =============
    public void ClearProgram()
    {
        // Hentikan eksekusi
        if (isExecuting)
        {
            StopAllCoroutines();
            isExecuting = false;
        }

        // Reset robot
        if (robot != null)
        {
            robot.ClearCommands();
        }

        // Kembalikan semua blok ke panel asal atau destroy
        foreach (CodeBlock block in codeBlocks.ToArray())
        {
            if (block == null) continue;

            // 🔥 Hapus fungsi dari FunctionManager jika blok definisi fungsi
            if (block.blockType == BlockType.FunctionDefinition)
            {
                FunctionDefinitionBlock funcDef = block.GetComponent<FunctionDefinitionBlock>();
                if (funcDef != null && !funcDef.isTemplate && FunctionManager.Instance != null)
                {
                    FunctionManager.Instance.UnregisterFunction(funcDef);
                }
            }

            if (!block.isTemplate && block.originalTemplate != null)
            {
                Destroy(block.gameObject);
            }
            else
            {
                ReturnBlockToPanel(block);
            }
        }

        // Kosongkan collections
        codeBlocks.Clear();
        blockOriginalParents.Clear();

        Debug.Log("🧹 Program dibersihkan!");
    }

    // ============= MENGEMBALIKAN BLOK KE PANEL =============
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

    // ============= FEEDBACK VISUAL DROP =============
    private IEnumerator ShowValidDropFeedback()
    {
        if (areaImage != null)
        {
            Color original = areaImage.color;
            areaImage.color = validDropColor;
            yield return new WaitForSeconds(0.3f);
            areaImage.color = original;
        }
    }

    private IEnumerator ShowInvalidDropFeedback()
    {
        if (areaImage != null)
        {
            Color original = areaImage.color;
            areaImage.color = invalidDropColor;
            yield return new WaitForSeconds(0.5f);
            areaImage.color = original;
        }
    }

    // Public wrapper untuk dipanggil dari luar
    public void DoShowValidDropFeedback()
    {
        StartCoroutine(ShowValidDropFeedback());
    }

    public void DoShowInvalidDropFeedback()
    {
        StartCoroutine(ShowInvalidDropFeedback());
    }

    // ============= OPSIONAL: SINKRONISASI SAAT END DRAG =============
    public void OnEndDrag(PointerEventData eventData)
    {
        // Tidak diperlukan untuk fungsionalitas utama, bisa dihapus atau dikosongkan
        // Jika ingin memastikan blok yang di-drag tetap tercatat, bisa diabaikan.
    }
}
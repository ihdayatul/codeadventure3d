using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using static CodeBlock;   // <--- TAMBAHKAN INI

public class FunctionDefinitionBlock : MonoBehaviour, IDropHandler
{
    [Header("Function Settings")]
    public string functionName = "F";           // Nama fungsi default "F"
    public Transform commandContainer;         // Tempat blok perintah dalam fungsi
    public bool isTemplate = false;           // True jika di panel FUNCTION (template)

    [Header("UI References")]
    public Text functionNameText;             // Teks nama fungsi
    public Image blockBackground;            // Background blok
    public Color functionColor = new Color(0.2f, 0.6f, 1f, 1f); // Biru

    [Header("Animation")]
    public Color highlightColor = Color.yellow;
    private Color originalColor;

    // Private
    private CodeBlock codeBlock;
    private List<CodeBlock> commandList = new List<CodeBlock>();

    void Awake()
    {
        codeBlock = GetComponent<CodeBlock>();
        if (codeBlock != null)
        {
            codeBlock = GetComponent<CodeBlock>();
            codeBlock.blockType = BlockType.FunctionDefinition;
        }

        if (blockBackground == null)
            blockBackground = GetComponent<Image>();

        if (blockBackground != null)
        {
            originalColor = blockBackground.color;
            blockBackground.color = functionColor;
        }

        UpdateUI();
    }

    void Start()
    {
        // Jika ini instance (bukan template), daftarkan ke FunctionManager
        if (!isTemplate && FunctionManager.Instance != null)
        {
            FunctionManager.Instance.RegisterFunction(this);
        }

        // Pastikan commandContainer punya FunctionDropZone
        EnsureDropZone();
    }

    void EnsureDropZone()
    {
        if (commandContainer != null && commandContainer.GetComponent<FunctionDropZone>() == null)
        {
            var dropZone = commandContainer.gameObject.AddComponent<FunctionDropZone>();
            dropZone.targetFunction = this;
        }
    }

    void UpdateUI()
    {
        if (functionNameText != null)
            functionNameText.text = functionName;

        if (codeBlock != null)
            codeBlock.displayText = functionName;
    }

    // ============= HANDLE DROP =============
    public void OnDrop(PointerEventData eventData)
    {
        if (isTemplate) return; // Template tidak menerima drop

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock block = dropped.GetComponent<CodeBlock>();
        if (block == null) return;

        // Hanya izinkan blok perintah (Move, Rotate, Loop, FunctionCall) masuk
        if (block.blockType == BlockType.Move ||
            block.blockType == BlockType.RotateLeft ||
            block.blockType == BlockType.RotateRight ||
            block.blockType == BlockType.Loop ||
            block.blockType == BlockType.FunctionCall)
        {
            AddCommand(block);
        }
    }

    // Method untuk menambah blok perintah ke dalam fungsi
    public void AddCommand(CodeBlock block)
    {
        if (commandContainer == null) return;

        // Set parent ke commandContainer
        block.transform.SetParent(commandContainer);

        // Reset transform
        RectTransform rt = block.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * 0.9f; // Sedikit lebih kecil

        // Pastikan bukan template lagi
        var blockComponent = block.GetComponent<CodeBlock>();
        if (blockComponent != null)
            blockComponent.isTemplate = false;

        // Tambahkan ke list
        if (!commandList.Contains(block))
        {
            commandList.Add(block);
            Debug.Log($"➕ Blok {block.blockType} ditambahkan ke fungsi {functionName}");
        }

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandContainer.GetComponent<RectTransform>());
    }

    // Hapus blok dari fungsi
    public void RemoveCommand(CodeBlock block)
    {
        if (commandList.Contains(block))
        {
            commandList.Remove(block);
        }
    }

    // Mendapatkan semua perintah dalam fungsi
    public List<CodeBlock> GetCommands()
    {
        // Refresh list dari container
        commandList.Clear();
        foreach (Transform child in commandContainer)
        {
            CodeBlock b = child.GetComponent<CodeBlock>();
            if (b != null) commandList.Add(b);
        }
        return commandList;
    }

    // Validasi: fungsi tidak boleh kosong
    public bool IsEmpty()
    {
        return commandContainer == null || commandContainer.childCount == 0;
    }

    // ============= EKSEKUSI FUNGSI =============
    public IEnumerator Execute(RobotController robot)
    {
        Debug.Log($"▶️ Menjalankan fungsi: {functionName}");

        // Validasi: fungsi tidak boleh kosong
        if (IsEmpty())
        {
            Debug.LogWarning($"⚠️ Fungsi {functionName} kosong! Tidak ada yang dieksekusi.");
            yield break;
        }

        // Highlight blok fungsi
        StartCoroutine(HighlightBlock());

        var commands = GetCommands();

        foreach (CodeBlock cmd in commands)
        {
            if (cmd == null) continue;

            // Highlight blok yang sedang dijalankan
            var highlighter = cmd.GetComponent<BlockHighlighter>();
            if (highlighter != null)
            {
                yield return StartCoroutine(highlighter.Highlight());
            }

            // Eksekusi berdasarkan tipe blok
            switch (cmd.blockType)
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
                    var loopBlock = cmd.GetComponent<LoopBlock>();
                    if (loopBlock != null)
                    {
                        // Asumsikan LoopBlock punya method Execute
                        // yield return loopBlock.Execute(robot);
                    }
                    break;
                case BlockType.FunctionCall:
                    var callBlock = cmd.GetComponent<FunctionCallBlock>();
                    if (callBlock != null)
                    {
                        yield return callBlock.Execute(robot);
                    }
                    break;
            }

            // Cek jika robot jatuh
            if (GameManager.instance != null && GameManager.instance.isPlayerFallen)
                yield break;
        }
    }

    // Animasi highlight blok fungsi
    private IEnumerator HighlightBlock()
    {
        if (blockBackground == null) yield break;

        Color original = blockBackground.color;
        blockBackground.color = highlightColor;
        yield return new WaitForSeconds(0.3f);
        blockBackground.color = original;
    }

    private void OnDestroy()
    {
        // Hapus dari FunctionManager jika ini instance
        if (!isTemplate && FunctionManager.Instance != null)
        {
            FunctionManager.Instance.UnregisterFunction(this);
        }
    }
}
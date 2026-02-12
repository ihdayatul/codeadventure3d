using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CodeBlock;

public class LoopBlock : MonoBehaviour, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image blockImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private ContentSizeFitter contentFitter;
    [SerializeField] private HorizontalLayoutGroup layoutGroup; // HARUS Vertical, bukan Horizontal
    [SerializeField] private InputFieldButton repeatInput;
    [SerializeField] private Button addBlockButton;           // Tombol untuk mode tambah blok
    [SerializeField] private Transform commandContainer;      // Tempat blok di dalam loop

    [Header("Settings")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.4f, 1f, 1f); // Ungu
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private int maxBlocksInLoop = 5;

    [Header("State")]
    public bool isTemplate = false;
    public bool isCodeBlock = false;

    // Daftar blok di dalam loop
    private List<CodeBlock> codeBlocks = new List<CodeBlock>();
    public List<CodeBlock> CodeBlocks => codeBlocks;

    // Jumlah pengulangan dari InputFieldButton
    public int RepeatCount => repeatInput != null ? repeatInput.CurrentValue : 2;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Setup tombol
        if (addBlockButton != null)
            addBlockButton.onClick.AddListener(AllowAddBlock);

        // Sembunyikan elemen UI jika masih template
        if (isTemplate && !isCodeBlock)
        {
            if (repeatInput != null) repeatInput.gameObject.SetActive(false);
            if (addBlockButton != null) addBlockButton.gameObject.SetActive(false);
            if (commandContainer != null) commandContainer.gameObject.SetActive(false);
        }
    }

    // ============= SETUP KETIKA DI-DROP KE PROGRAMMING AREA =============
    public void SetAsCodeBlock()
    {
        isCodeBlock = true;
        isTemplate = false;

        // Aktifkan UI yang diperlukan
        if (repeatInput != null) repeatInput.gameObject.SetActive(true);
        if (addBlockButton != null) addBlockButton.gameObject.SetActive(true);
        if (commandContainer != null) commandContainer.gameObject.SetActive(true);
        if (layoutGroup != null) layoutGroup.enabled = true;
        if (contentFitter != null) contentFitter.enabled = true;
        iconImage.gameObject.SetActive(false);

        // Warna latar
        if (blockImage != null)
            blockImage.color = normalColor;
    }

    // ============= AKTIFKAN MODE TERIMA BLOK =============
    private void AllowAddBlock()
    {
        StartCoroutine(ActivateDropMode());
    }

    private IEnumerator ActivateDropMode()
    {
        // Ubah warna sebagai indikasi siap menerima blok
        if (blockImage != null)
            blockImage.color = highlightColor;

        // Tunggu selama 2 detik untuk menerima drop
        float timer = 2f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Kembalikan warna normal
        if (blockImage != null && isCodeBlock)
            blockImage.color = normalColor;
    }

    // ============= HANDLE DROP BLOK KE DALAM LOOP =============
    public void OnDrop(PointerEventData eventData)
    {
        // Hanya bisa drop jika sudah menjadi code block dan dalam mode aktif (warna hijau)
        if (!isCodeBlock)
            return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock codeBlock = dropped.GetComponent<CodeBlock>();
        if (codeBlock == null) return;

        // Hanya blok perintah yang bisa masuk (bukan loop atau definisi fungsi)
        if (codeBlock.blockType != BlockType.Move &&
            codeBlock.blockType != BlockType.RotateLeft &&
            codeBlock.blockType != BlockType.RotateRight &&
            codeBlock.blockType != BlockType.FunctionCall)
        {
            Debug.Log("❌ Hanya blok Move, Rotate, atau FunctionCall yang bisa masuk ke loop.");
            return;
        }

        // Cek batas maksimum blok dalam loop
        if (codeBlocks.Count >= maxBlocksInLoop)
        {
            Debug.LogWarning($"Maksimum {maxBlocksInLoop} blok dalam loop tercapai!");
            return;
        }

        // Cek duplikat
        if (codeBlocks.Contains(codeBlock))
            return;

        // Tambahkan ke list
        codeBlocks.Add(codeBlock);

        // Pindahkan parent ke commandContainer
        codeBlock.transform.SetParent(commandContainer);

        // Reset posisi dan ukuran
        RectTransform rt = codeBlock.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(130, 40); // Lebih kecil dari blok utama
        rt.localScale = Vector3.one * 0.9f;

        // Force rebuild layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandContainer.GetComponent<RectTransform>());

        Debug.Log($"➕ Blok {codeBlock.blockType} ditambahkan ke loop. Total: {codeBlocks.Count}");
    }

    // ============= EKSEKUSI LOOP =============
    public IEnumerator ExecuteLoop(RobotController robot)
    {
        if (codeBlocks.Count == 0)
        {
            Debug.LogWarning("⚠️ Loop kosong, tidak ada yang dieksekusi.");
            yield break;
        }

        Debug.Log($"🔄 Loop dimulai: {RepeatCount}x pengulangan, {codeBlocks.Count} blok perintah.");

        for (int i = 0; i < RepeatCount; i++)
        {
            Debug.Log($"🔄 Iterasi ke-{i + 1}");

            foreach (CodeBlock block in codeBlocks)
            {
                if (block == null) continue;

                // Highlight blok yang sedang dijalankan (opsional)
                BlockHighlighter highlighter = block.GetComponent<BlockHighlighter>();
                if (highlighter != null)
                    yield return highlighter.Highlight();

                // Eksekusi perintah
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
                    case BlockType.FunctionCall:
                        FunctionCallBlock call = block.GetComponent<FunctionCallBlock>();
                        if (call != null)
                            yield return call.Execute(robot);
                        break;
                }

                // Hentikan jika robot jatuh
                if (GameManager.instance != null && GameManager.instance.isPlayerFallen)
                    yield break;
            }
        }

        Debug.Log("✅ Loop selesai.");
    }

    // ============= HAPUS BLOK DARI LOOP =============
    public void RemoveBlock(CodeBlock block)
    {
        if (codeBlocks.Contains(block))
        {
            codeBlocks.Remove(block);
            Destroy(block.gameObject);
        }
    }

    // ============= BERSIHKAN SEMUA BLOK DI LOOP =============
    public void ClearLoop()
    {
        foreach (var block in codeBlocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        codeBlocks.Clear();
    }

    private void OnDestroy()
    {
        if (addBlockButton != null)
            addBlockButton.onClick.RemoveListener(AllowAddBlock);
    }
}
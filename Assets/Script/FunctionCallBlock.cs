using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CodeBlock;

public class FunctionCallBlock : MonoBehaviour
{
    [Header("Function Call Settings")]
    public string functionName = "F";           // Nama fungsi yang dipanggil
    public bool isTemplate = false;            // True jika di panel template

    [Header("UI References")]
    public TMP_Dropdown functionDropdown;      // Dropdown pilih fungsi
    public Text displayText;                  // Teks pada blok
    public Image blockBackground;            // Background blok
    public Color callColor = new Color(0.4f, 0.8f, 0.9f, 1f); // Cyan

    [Header("Animation")]
    public Color highlightColor = Color.yellow;
    private Color originalColor;

    // Private
    private CodeBlock codeBlock;
    private FunctionDefinitionBlock targetFunction;

    void Awake()
    {
        codeBlock = GetComponent<CodeBlock>();
        if (codeBlock != null)
        {
            codeBlock.blockType = BlockType.FunctionCall;
            codeBlock.displayText = "Panggil " + functionName;
        }

        if (blockBackground == null)
            blockBackground = GetComponent<Image>();

        if (blockBackground != null)
        {
            originalColor = blockBackground.color;
            blockBackground.color = callColor;
        }

        UpdateUI();
    }

    void Start()
    {
        if (!isTemplate)
        {
            RefreshDropdown();  // Isi dropdown dengan daftar fungsi
        }
    }

    // Memperbarui UI
    void UpdateUI()
    {
        if (displayText != null)
            displayText.text = "Panggil " + functionName;

        if (codeBlock != null)
            codeBlock.displayText = "Panggil " + functionName;
    }

    // ============= DROPDOWN FUNGSI =============
    public void RefreshDropdown()
    {
        if (functionDropdown == null) return;
        if (FunctionManager.Instance == null)
        {
            Debug.LogError("FunctionManager.Instance tidak ditemukan!");
            return;
        }

        // Ambil semua nama fungsi
        var names = FunctionManager.Instance.GetAllFunctionNames();
        functionDropdown.ClearOptions();

        if (names.Count == 0)
        {
            names.Add("(tidak ada fungsi)");
            functionDropdown.AddOptions(names);
            functionDropdown.interactable = false;
            return;
        }

        functionDropdown.AddOptions(names);
        functionDropdown.interactable = true;
        functionDropdown.onValueChanged.RemoveAllListeners();
        functionDropdown.onValueChanged.AddListener(OnFunctionSelected);

        // Coba pilih fungsi "F" jika ada, jika tidak pilih index 0
        int index = names.IndexOf("F");
        if (index >= 0)
        {
            functionDropdown.value = index;
            OnFunctionSelected(index);
        }
        else
        {
            functionDropdown.value = 0;
            OnFunctionSelected(0);
        }
    }

    void OnFunctionSelected(int index)
    {
        var names = FunctionManager.Instance.GetAllFunctionNames();
        if (index >= 0 && index < names.Count)
        {
            functionName = names[index];
            targetFunction = FunctionManager.Instance.GetFunction(functionName);
            UpdateUI();
            Debug.Log($"📞 Blok panggil memilih fungsi: {functionName}");
        }
    }

    // ============= EKSEKUSI =============
    public IEnumerator Execute(RobotController robot)
    {
        // Validasi: apakah target fungsi ada?
        if (targetFunction == null)
        {
            // Coba cari ulang berdasarkan nama
            targetFunction = FunctionManager.Instance.GetFunction(functionName);
        }

        if (targetFunction == null)
        {
            Debug.LogWarning($"❌ Fungsi '{functionName}' tidak ditemukan!");
            yield break;
        }

        // Validasi: fungsi tidak boleh kosong
        if (targetFunction.IsEmpty())
        {
            Debug.LogWarning($"⚠️ Fungsi '{functionName}' kosong! Tidak ada yang dieksekusi.");
            yield break;
        }

        // Highlight blok panggil
        StartCoroutine(HighlightBlock());

        // Eksekusi fungsi
        yield return targetFunction.Execute(robot);
    }

    // Animasi highlight
    private IEnumerator HighlightBlock()
    {
        if (blockBackground == null) yield break;

        Color original = blockBackground.color;
        blockBackground.color = highlightColor;
        yield return new WaitForSeconds(0.3f);
        blockBackground.color = original;
    }
}
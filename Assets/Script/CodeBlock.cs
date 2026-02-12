using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ============= ENUM JENIS BLOK =============
    public enum BlockType
    {
        Move,
        RotateLeft,
        RotateRight,
        Loop,
        FunctionDefinition,  // Blok definisi fungsi (di panel FUNCTION)
        FunctionCall         // Blok panggil fungsi (di panel PROGRAM)
    }

    [Header("Block Configuration")]
    public BlockType blockType = BlockType.Move;
    public string displayText = "Blok Kode";

    [Header("Template Reference")]
    [Tooltip("Referensi ke template asal blok ini")]
    public CodeBlock originalTemplate;

    [Header("Block Settings")]
    public bool isTemplate = false; // True jika di panel palette, false jika di program area

    // Private references
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image blockImage;
    private Text uiText;
    private Transform parentBeforeDrag;
    private Vector2 positionBeforeDrag;

    // ============= INITIALIZATION =============
    void Awake()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        blockImage = GetComponent<Image>();
        uiText = GetComponentInChildren<Text>();

        // Pastikan CanvasGroup tersedia untuk mengontrol raycast saat drag
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Set teks awal
        if (uiText != null && !string.IsNullOrEmpty(displayText))
            uiText.text = displayText;
    }

    // ============= DRAG & DROP HANDLERS =============
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Jika ini adalah TEMPLATE (blok di panel palette), buat duplikat
        if (isTemplate)
        {
            CreateDuplicateForDrag(eventData);
            return;
        }

        // Non-aktifkan raycast pada gambar agar drop target bisa mendeteksi
        if (blockImage != null)
            blockImage.raycastTarget = false;

        // Simpan posisi dan parent asli
        parentBeforeDrag = transform.parent;
        positionBeforeDrag = rectTransform.anchoredPosition;

        // Pindahkan ke root canvas agar berada di atas semua elemen
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Atur visual saat drag
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
        rectTransform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Gerakkan blok mengikuti posisi mouse
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Kembalikan visual ke normal
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;

        // Aktifkan kembali raycast pada image
        if (blockImage != null)
            blockImage.raycastTarget = true;

        // Jika tidak di-drop di area yang valid, kembalikan ke posisi asal
        if (transform.parent == parentBeforeDrag ||
            transform.parent == transform.root ||
            transform.parent == null)
        {
            ReturnToOriginalPosition();
        }
    }

    // ============= DUPLIKAT TEMPLATE =============
    private void CreateDuplicateForDrag(PointerEventData eventData)
    {
        // Buat salinan baru dari prefab
        GameObject newBlockObj = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
        CodeBlock newBlock = newBlockObj.GetComponent<CodeBlock>();

        // Set sebagai instance (bukan template)
        newBlock.isTemplate = false;
        newBlock.originalTemplate = this;

        // Salin tampilan visual (sprite dan warna)
        if (blockImage != null)
        {
            Image newImage = newBlockObj.GetComponent<Image>();
            if (newImage != null)
            {
                newImage.sprite = blockImage.sprite;
                newImage.color = blockImage.color;
            }
        }

        // Alihkan event drag ke duplikat dan mulai drag untuk duplikat
        eventData.pointerDrag = newBlockObj;
        newBlock.OnBeginDrag(eventData);
    }

    // ============= KEMBALI KE POSISI ASAL =============
    public void ReturnToOriginalPosition()
    {
        if (parentBeforeDrag != null)
        {
            transform.SetParent(parentBeforeDrag);
            rectTransform.anchoredPosition = positionBeforeDrag;
        }
        else if (originalTemplate != null && originalTemplate.transform.parent != null)
        {
            // Kembali ke panel blok kode
            transform.SetParent(originalTemplate.transform.parent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    // ============= METHOD TAMBAHAN =============
    /// <summary>
    /// Mengembalikan blok ke panel blok (untuk reset program).
    /// </summary>
    public void ReturnToBlockPanel()
    {
        if (originalTemplate != null && originalTemplate.transform.parent != null)
        {
            transform.SetParent(originalTemplate.transform.parent);
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false); // Sembunyikan sementara
        }
    }

    /// <summary>
    /// Menghancurkan blok (digunakan saat clear program).
    /// </summary>
    public void Dispose()
    {
        Destroy(gameObject);
    }
}
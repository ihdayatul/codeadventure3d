using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Block Configuration")]
    public string blockType = "move";
    public string displayText = "Blok Kode";

    [Header("Template Reference")]
    [Tooltip("Referensi ke template asal blok ini")]
    public CodeBlock originalTemplate; // Ganti dari private ke public

    [Header("Block Settings")]
    public bool isTemplate = false; // Apakah ini blok template di panel?

    // Private references
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image blockImage;
    private Text uiText;
    private Transform parentBeforeDrag;
    private Vector2 positionBeforeDrag;

    void Awake()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        blockImage = GetComponent<Image>();
        uiText = GetComponentInChildren<Text>();

        // Add CanvasGroup if not exist
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Setup text
        if (uiText != null && !string.IsNullOrEmpty(displayText))
            uiText.text = displayText;

        // Setup initial color

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Jika ini adalah TEMPLATE (blok di panel asal), buat duplikat
        if (isTemplate)
        {
            // Buat salinan baru dari prefab
            GameObject newBlockObj = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
            CodeBlock newBlock = newBlockObj.GetComponent<CodeBlock>();
            newBlock.isTemplate = false; // Ini bukan template
            newBlock.originalTemplate = this; // Simpan referensi ke template asal

            // Copy semua komponen visual
            Image originalImage = GetComponent<Image>();
            Image newImage = newBlockObj.GetComponent<Image>();
            if (originalImage != null && newImage != null)
            {
                newImage.sprite = originalImage.sprite;
                newImage.color = originalImage.color;
            }

            // Pindahkan event drag ke duplikat
            eventData.pointerDrag = newBlockObj;
            newBlock.OnBeginDrag(eventData);
            return;

            
        }

        // Simpan posisi asli
        parentBeforeDrag = transform.parent;
        positionBeforeDrag = rectTransform.anchoredPosition;

        // Pindahkan ke canvas root untuk dragging
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Setup visual untuk dragging
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f; // Sedikit transparan

        // Perbesar sedikit saat dragging
        rectTransform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

    }

    void SaveOriginalPosition()
    {
        parentBeforeDrag = transform.parent;
        positionBeforeDrag = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update position with mouse
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
        // Restore visual
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;


        // Jika tidak di-drop di area valid, kembalikan ke posisi asal
        if (transform.parent == parentBeforeDrag ||
            transform.parent == transform.root ||
            transform.parent == null)
        {
            ReturnToOriginalPosition();
        }

    }

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





    public void ReturnToBlockPanel()
    {
        // Kembalikan ke panel blok kode (destroy atau reset)
        if (originalTemplate != null && originalTemplate.transform.parent != null)
        {
            transform.SetParent(originalTemplate.transform.parent);
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false); // Sembunyikan sementara
        }
    }

    // Method untuk menghapus blok setelah selesai
    public void Dispose()
    {
        Destroy(gameObject);
    }
}
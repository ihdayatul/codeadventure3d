using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashCan : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public ProgrammingArea programmingArea; // Drag ProgrammingArea ke sini di Inspector
    public Image trashImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.red;

    private void Start()
    {
        if (programmingArea == null)
            programmingArea = FindFirstObjectByType<ProgrammingArea>();

        if (trashImage == null)
            trashImage = GetComponent<Image>();
    }

    // ============= HANDLE DROP =============
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock block = dropped.GetComponent<CodeBlock>();
        if (block == null) return;

        // ❌ Jangan hapus blok template (yang masih di panel palette)
        if (block.isTemplate) return;

        // ✅ Hapus blok dari Programming Area
        if (programmingArea != null)
        {
            programmingArea.RemoveBlock(block);
        }

        // 💥 Hancurkan GameObject blok
        block.Dispose();

        Debug.Log($"🗑️ Blok {block.blockType} dihapus");
    }

    // ============= FEEDBACK VISUAL SAAT DRAG DI ATAS TONG =============
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (trashImage != null)
            trashImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (trashImage != null)
            trashImage.color = normalColor;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using static CodeBlock;

public class FunctionDropZone : MonoBehaviour, IDropHandler
{
    [HideInInspector]
    public FunctionDefinitionBlock targetFunction;

    void Awake()
    {
        // Cari fungsi induk
        if (targetFunction == null)
            targetFunction = GetComponentInParent<FunctionDefinitionBlock>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (targetFunction == null || targetFunction.isTemplate) return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock block = dropped.GetComponent<CodeBlock>();
        if (block == null) return;

        // Hanya blok perintah yang bisa masuk
        if (block.blockType == BlockType.Move ||
            block.blockType == BlockType.RotateLeft ||
            block.blockType == BlockType.RotateRight ||
            block.blockType == BlockType.Loop ||
            block.blockType == BlockType.FunctionCall)
        {
            targetFunction.AddCommand(block);
        }
    }
}
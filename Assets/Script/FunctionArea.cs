using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CodeBlock;

public class FunctionArea : MonoBehaviour, IDropHandler
{

    [SerializeField]
    private int maxBlocks = 5;
    [SerializeField]
    private RectTransform commandContainer;
    [SerializeField]
    private CodeBlock loopBlock;
    [SerializeField]
    private Image areaImage;

    [Header("Visual")]
    [SerializeField] Color validDropColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] Color invalidDropColor = new Color(1, 0, 0, 0.3f);

    private FunctionBlock selectedFunction;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetSelectedFunction(FunctionBlock functionBlock)
    {
        foreach (Transform child in commandContainer)
        {
            child.gameObject.SetActive(false);
        }

        if (selectedFunction == functionBlock)
        {
            selectedFunction = null;
            gameObject.SetActive(false);
            return;
        }
        selectedFunction = functionBlock;
        foreach (var codeBlock in selectedFunction.CodeBlocks)
        {
            codeBlock.gameObject.SetActive(true);
        }
        gameObject.SetActive(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (GameManager.instance.IsExecutingProgram)
        {
            Debug.LogWarning("Program sedang berjalan. Tidak bisa menambahkan blok.");
            StartCoroutine(ShowInvalidDropFeedback());
            return;
        }

        if (selectedFunction == null)
        {
            return;
        }

        var codeBlocks = selectedFunction.CodeBlocks;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock codeBlock = dropped.GetComponent<CodeBlock>();
        if (codeBlock == null) return;

        // Hanya blok perintah yang bisa masuk (bukan loop atau definisi fungsi)
        if (codeBlock.blockType != BlockType.Move &&
            codeBlock.blockType != BlockType.RotateLeft &&
            codeBlock.blockType != BlockType.RotateRight &&
            codeBlock.blockType != BlockType.FunctionDefinition)
        {
            Debug.Log("❌ Hanya blok Move, Rotate, atau FunctionCall yang bisa masuk ke loop.");
            StartCoroutine(ShowInvalidDropFeedback());
            return;
        }

        // Cek batas maksimum blok dalam loop
        if (codeBlocks.Count >= maxBlocks)
        {
            Debug.LogWarning($"Maksimum {maxBlocks} blok dalam loop tercapai!");
            StartCoroutine(ShowInvalidDropFeedback());
            return;
        }

        // Cek duplikat
        if (codeBlocks.Contains(codeBlock))
            return;

        // Tambahkan ke list
        codeBlocks.Add(codeBlock);

        // Pindahkan parent ke commandContainer
        codeBlock.transform.SetParent(commandContainer);

        // Force rebuild layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandContainer);

        StartCoroutine(ShowValidDropFeedback());

        Debug.Log($"➕ Blok {codeBlock.blockType} ditambahkan ke loop. Total: {codeBlocks.Count}");
    }

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

    public void Clear()
    {
        if (selectedFunction != null)
        {
            selectedFunction.Clear();
        }
        selectedFunction = null;
        foreach (Transform child in commandContainer)
        {
            Destroy(child.gameObject);
        }
    }
}

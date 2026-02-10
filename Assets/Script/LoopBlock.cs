using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoopBlock : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private Image blockImage;
    //[SerializeField]
    //private RectTransform container;
    [SerializeField]
    private ContentSizeFitter contentFitter;
    [SerializeField]
    private HorizontalLayoutGroup layoutGroup;
    [SerializeField]
    private InputFieldButton repeatInput;
    [SerializeField]
    private Button button;

    private List<CodeBlock> codeBlocks = new List<CodeBlock>();
    public List<CodeBlock> CodeBlocks => codeBlocks;

    public int RepeatCount => repeatInput.CurrentValue;

    private bool isCodeBlock = false;

    private void Awake()
    {
        button.onClick.AddListener(AllowAddBlock);

    }

    public void SetAsCodeBlock()
    {
        repeatInput.gameObject.SetActive(true);
        nameText.gameObject.SetActive(false);
        layoutGroup.enabled = true;
        contentFitter.enabled = true;
        button.enabled = true;
        isCodeBlock = true;
    }

    private void AllowAddBlock()
    {
        blockImage.color = Color.green;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isCodeBlock)
        {
            return;
        }

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CodeBlock codeBlock = dropped.GetComponent<CodeBlock>();
        if (codeBlock != null)
        {
            // Cek batas maksimum
            if (GameManager.instance.GetCurrentBlockCount() >= GameManager.instance.GetMaxBlocks())
            {
                Debug.LogWarning($"Maksimum {10} blok tercapai!");
                GameManager.instance.ShowInvalidDropFeedback();
                //ReturnBlockToPanel(codeBlock);
                return;
            }

            // Simpan parent asli
            //if (!blockOriginalParents.ContainsKey(codeBlock))
            //{
            //    blockOriginalParents[codeBlock] = codeBlock.transform.parent;
            //}

            // Tambahkan ke list
            if (!codeBlocks.Contains(codeBlock))
            {
                codeBlocks.Add(codeBlock);
                codeBlock.transform.SetParent(transform);
                if (codeBlock.blockType == "loop")
                {
                    var loopBlock = codeBlock.GetComponent<LoopBlock>();
                    loopBlock.SetAsCodeBlock();
                }

                // Reset posisi
                RectTransform rt = codeBlock.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(150, 80);

                GameManager.instance.RefreshList();

                Debug.Log($"Blok ditambahkan: {codeBlock.blockType} (Total: {codeBlocks.Count})");

                //runButton.enabled = codeBlocks.Count > 0;

                GameManager.instance.ShowValidDropFeedback();
            }
        }
    }
}

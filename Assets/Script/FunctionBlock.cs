using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunctionBlock : MonoBehaviour
{
    [SerializeField]
    private Button button;

    private List<CodeBlock> codeBlocks = new();
    public List<CodeBlock> CodeBlocks => codeBlocks;

    private bool isCodeBlock = false;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    public void SetAsCodeBlock()
    {
        isCodeBlock = true;
    }

    private void OnButtonClick()
    {
        if (isCodeBlock)
        {
            GameManager.instance.SetSelectedFunction(this);
        }
    }

    public void Clear()
    {
        codeBlocks.Clear();
    }
}

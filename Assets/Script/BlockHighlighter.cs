using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlockHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 0.3f;
    public bool useFlash = true;
    public int flashCount = 1;

    private Image blockImage;
    private Color originalColor;

    void Awake()
    {
        blockImage = GetComponent<Image>();
        if (blockImage != null)
            originalColor = blockImage.color;
    }

    public IEnumerator Highlight()
    {
        if (blockImage == null) yield break;

        if (useFlash)
        {
            for (int i = 0; i < flashCount; i++)
            {
                // Ubah ke warna highlight
                blockImage.color = highlightColor;
                yield return new WaitForSeconds(highlightDuration / 2);

                // Kembali ke warna asli
                blockImage.color = originalColor;
                yield return new WaitForSeconds(highlightDuration / 2);
            }
        }
        else
        {
            blockImage.color = highlightColor;
            yield return new WaitForSeconds(highlightDuration);
            blockImage.color = originalColor;
        }
    }

    public void SetOriginalColor(Color color)
    {
        originalColor = color;
    }
}
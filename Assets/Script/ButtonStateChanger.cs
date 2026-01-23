using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonStateChanger : MonoBehaviour
{
    [Header("Button References")]
    public Button mainButton;
    public Image buttonImage;
    public Text buttonText;

    [Header("Colors")]
    public Color runColor = new Color(0.2f, 0.5f, 1f, 1f);
    public Color repeatColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color runningColor = new Color(0.8f, 0.8f, 0.2f, 1f);

    [Header("Animations")]
    public bool useAnimations = true;
    public float colorChangeDuration = 0.3f;

    private Coroutine colorChangeCoroutine;

    public void ChangeToRunState()
    {
        ChangeState("RUN", runColor, "Klik untuk menjalankan program");
    }

    public void ChangeToRepeatState()
    {
        ChangeState("ULANGI", repeatColor, "Klik untuk mengulang program dari awal");
    }

    public void ChangeToRunningState()
    {
        ChangeState("BERJALAN...", runningColor, "Program sedang dieksekusi");
        if (mainButton != null)
            mainButton.interactable = false;
    }

    public void ChangeToCompletedState()
    {
        ChangeState("SELESAI", Color.green, "Program selesai dieksekusi");
        if (mainButton != null)
            mainButton.interactable = true;
    }

    private void ChangeState(string text, Color color, string tooltip = "")
    {
        if (buttonText != null)
            buttonText.text = text;

        if (buttonImage != null)
        {
            if (useAnimations && colorChangeCoroutine != null)
                StopCoroutine(colorChangeCoroutine);

            if (useAnimations)
                colorChangeCoroutine = StartCoroutine(AnimateColorChange(color));
            else
                buttonImage.color = color;
        }

        // Update tooltip
        if (!string.IsNullOrEmpty(tooltip))
        {
            // Implement tooltip system sesuai kebutuhan
        }
    }

    private IEnumerator AnimateColorChange(Color targetColor)
    {
        if (buttonImage == null) yield break;

        Color startColor = buttonImage.color;
        float elapsed = 0f;

        while (elapsed < colorChangeDuration)
        {
            buttonImage.color = Color.Lerp(startColor, targetColor, elapsed / colorChangeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        buttonImage.color = targetColor;
    }

    // Method untuk diakses dari GameManager
    public void UpdateButtonState(bool isRepeating, bool isRunning)
    {
        if (isRunning)
        {
            ChangeToRunningState();
        }
        else if (isRepeating)
        {
            ChangeToRepeatState();
        }
        else
        {
            ChangeToRunState();
        }
    }
}
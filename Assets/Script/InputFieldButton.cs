using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldButton : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private Button plusButton;
    [SerializeField]
    private Button minusButton;

    [SerializeField]
    private int maxValue = 10;
    [SerializeField]
    private int minValue = 0;

    private int currentValue = 0;

    public int CurrentValue => currentValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentValue = minValue;
        inputField.text = currentValue.ToString();
        plusButton.onClick.AddListener(OnPlusButtonClicked);
        minusButton.onClick.AddListener(OnMinusButtonClicked);
    }

    public void OnPlusButtonClicked()
    {
        currentValue++;
        currentValue = Mathf.Min(currentValue, maxValue);
        inputField.text = currentValue.ToString();
    }

    public void OnMinusButtonClicked()
    {
        currentValue--;
        currentValue = Mathf.Max(currentValue, minValue);
        inputField.text = currentValue.ToString();
    }
}

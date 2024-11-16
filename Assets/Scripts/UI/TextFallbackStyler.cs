using UnityEngine;
using TMPro;

public class InputFieldTextUpdater : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; // Reference to the Input Field component
    [SerializeField] private TextMeshPro placeholderTextSource; // Reference to the placeholder text (default text)
    [SerializeField, Range(0, 255)] private float placeholderAlpha = 128f; // Alpha value for the placeholder text

    private TextMeshPro textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshPro>();

        // Set initial text to the placeholder if it's not already set
        if (string.IsNullOrEmpty(textComponent.text) && placeholderTextSource != null)
        {
            SetTextToPlaceholder();
        }
    }

    void Start()
    {
        if (inputField != null)
        {
            // Add a listener to detect changes in the input field's text
            inputField.onValueChanged.AddListener(HandleInputFieldChange);
        }
    }

    private void HandleInputFieldChange(string newValue)
    {
        // If the input field is empty, set the parent text to the placeholder
        if (string.IsNullOrEmpty(newValue) && placeholderTextSource != null)
        {
            SetTextToPlaceholder();
        }
        else
        {
            // Otherwise, update the text with the current input field value
            textComponent.text = newValue;
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 255f); // Set alpha to fully visible
            textComponent.fontStyle = FontStyles.Normal; // Set font style to normal (remove italics)
        }
    }

    private void SetTextToPlaceholder()
    {
        textComponent.text = placeholderTextSource.text;
        textComponent.color = new Color(placeholderTextSource.color.r, placeholderTextSource.color.g, placeholderTextSource.color.b, placeholderAlpha / 255f); // Set alpha from serialized field
        textComponent.fontStyle = placeholderTextSource.fontStyle; // Keep the placeholder's style (including italics)
    }

    private void OnDestroy()
    {
        // Remove the listener when the script is destroyed to prevent memory leaks
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(HandleInputFieldChange);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class InputFieldResizer : MonoBehaviour
{
    public RectTransform rectTransformComponent;

    public void ResizeText(RectTransform value)
    {
        // Force the layout to be rebuilt to get accurate sizing
        LayoutRebuilder.ForceRebuildLayoutImmediate(value);

        // Check if the parent rectTransform needs resizing
        if (rectTransformComponent.rect.height < value.rect.height)
        {
            rectTransformComponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.rect.height);
        }
    }
}
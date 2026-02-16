using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text maskNameText;

    void Awake()
    {
        if (!icon) icon = GetComponentInChildren<Image>(true);
        if (!maskNameText) maskNameText = GetComponentInChildren<TMP_Text>(true);
    }

    // ⭐ ให้ UIInventoryCarousel เรียกอันนี้
    public void SetData(Sprite sprite, string nameText = "")
    {
        // icon
        if (icon)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        // name
        if (maskNameText)
            maskNameText.text = nameText ?? "";
    }
}

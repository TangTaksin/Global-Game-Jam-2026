using UnityEngine;
using UnityEngine.UI;

public class MaskSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject highlight;

    public void SetIcon(Sprite sprite)
    {
        if (!icon) return;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    public void SetSelected(bool selected)
    {
        if (highlight) highlight.SetActive(selected);
    }
}

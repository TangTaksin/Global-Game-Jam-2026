using UnityEngine;
using UnityEngine.UI;

public class MaskSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void SetIcon(Sprite sprite)
    {
        if (!icon) return;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

}

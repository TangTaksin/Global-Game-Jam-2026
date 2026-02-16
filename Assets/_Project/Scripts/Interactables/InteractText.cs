using UnityEngine;
using TMPro; // สำหรับใช้ TextMeshPro
using DG.Tweening; // สำหรับ Animation

public class InteractText : MonoBehaviour, IInteractable
{
    [Header("UI Refs")]
    [SerializeField] private GameObject textPanel; // ตัว Object ที่เป็นพื้นหลังข้อความ
    [SerializeField] private TextMeshProUGUI contentText; // ตัวหนังสือ UI

    [Header("Settings")]
    [TextArea(3, 5)]
    [SerializeField] private string message = "ใส่ข้อความที่นี่...";
    [SerializeField] private float displayDuration = 2.5f;

    public Vector3 position => transform.position;

    [SerializeField] private bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    private CanvasGroup canvasGroup;
    private Tween hideTween;

    void Awake()
    {
        if (textPanel != null)
        {
            // ตรวจสอบหรือเพิ่ม CanvasGroup เพื่อใช้ Fade
            canvasGroup = textPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = textPanel.AddComponent<CanvasGroup>();

            textPanel.SetActive(false);
            canvasGroup.alpha = 0;
        }
    }

    public void Interact(object interacter)
    {
        if (!isInteractable || textPanel == null) return;

        ShowText();
    }

    private void ShowText()
    {
        hideTween?.Kill();

        // บังคับเปิด Panel ก่อนจัดการข้อความ
        textPanel.SetActive(true);

        // 1. ใส่ข้อความ
        contentText.text = message;

        // 2. บังคับให้ TMP คำนวณ Mesh ใหม่ทันที (แก้บัคข้อความไม่ขึ้น)
        contentText.ForceMeshUpdate();

        // 3. แถม: ทำเอฟเฟกต์พิมพ์ทีละตัว (Typewriter) ให้ดูโปรขึ้น
        contentText.maxVisibleCharacters = 0;
        DOTween.To(() => contentText.maxVisibleCharacters, x => contentText.maxVisibleCharacters = x, message.Length, 0.5f);

        // --- Animation ---
        textPanel.transform.localScale = Vector3.one * 0.8f;
        textPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.2f);

        hideTween = DOVirtual.DelayedCall(displayDuration, HideText);
    }

    private void HideText()
    {
        canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            textPanel.SetActive(false);
        });
    }
}
using System;
using UnityEngine;
using DG.Tweening; // ต้องเพิ่มอันนี้ครับ

public class MaskItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] MaskData _maskData;
    [SerializeField] bool _disableAfterCollected = true;

    public Vector3 position => transform.position;

    [SerializeField] bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    private SpriteRenderer sr;
    private Tween idleTween; // เก็บ Reference ไว้สำหรับเคลียร์ Memory

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyWorldSprite();
    }

    void Start()
    {
        // ทำ Animation ลอยขึ้นลง (Idle)
        StartIdleAnimation();
    }

    private void ApplyWorldSprite()
    {
        if (!sr || _maskData == null) return;
        sr.sprite = _maskData.world_sprite;
    }

    private void StartIdleAnimation()
    {
        // สั่งให้ขยับขึ้น 0.2 unit และวนลูปไปกลับแบบ Smooth
        idleTween = transform.DOMoveY(transform.position.y + 0.2f, 1.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Interact(object interacter)
    {
        if (!isInteractable) return;

        var p_inter = interacter as PlayerInteractor;
        if (p_inter == null) return;
        
        var m_inv = p_inter.transform.parent.GetComponent<MaskInventory>();
        if (m_inv == null) return;

        m_inv.AddMask(_maskData);
        
        // หยุดการกดซ้ำ
        isInteractable = false;

        // --- DOTween Pickup Effect ---
        // หยุด Idle Animation ก่อน
        idleTween.Kill();

        // เล่น Animation เล็กน้อยก่อนหายไป (เด้งขึ้นแล้วย่อตัว)
        Sequence pickupSequence = DOTween.Sequence();
        
        pickupSequence.Append(transform.DOMoveY(transform.position.y + 0.5f, 0.3f).SetEase(Ease.OutBack));
        pickupSequence.Join(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        
        pickupSequence.OnComplete(() => {
            if (_disableAfterCollected)
                gameObject.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        // ป้องกัน Memory Leak
        idleTween.Kill();
    }
}
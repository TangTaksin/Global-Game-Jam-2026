using UnityEngine;
using DG.Tweening;

public class MaskAnim2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement movement;

    [Header("Current Mask")]
    [SerializeField] private MaskData currentMask;

    // =============================
    // Bobbing
    // =============================
    [Header("Bobbing")]
    [SerializeField] private float bobAmount = 0.04f;
    [SerializeField] private float bobSpeed = 0.35f;

    // =============================
    // Rotate Y Equip Animation
    // =============================
    [Header("Rotate Y 360 Equip")]
    [SerializeField] private float spinDegrees = 360f;
    [SerializeField] private float spinTime = 0.35f;
    [SerializeField] private float overshoot = 15f;
    [SerializeField] private float settleTime = 0.12f;

    [SerializeField] private Ease spinEase = Ease.OutCubic;
    [SerializeField] private Ease settleEase = Ease.OutBack;

    private Tween bobTween;
    private Sequence equipSeq;

    private bool facingLeft;

    private const float DIR_EPS = 0.01f;

    // =====================================================

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!movement) movement = GetComponentInParent<PlayerMovement>();

        ApplyMask(currentMask);

        if (movement)
        {
            float dir = movement.CurrentDirection;
            facingLeft = dir < 0f;
            sr.flipX = facingLeft;
        }
    }

    // =====================================================
    // Flip
    // =====================================================

    void Update()
    {
        if (!sr || !movement || !sr.enabled) return;

        float dir = movement.CurrentDirection;
        if (Mathf.Abs(dir) <= DIR_EPS) return;

        bool newFacingLeft = dir < 0f;
        if (newFacingLeft == facingLeft) return;

        facingLeft = newFacingLeft;
        sr.flipX = facingLeft;
    }

    // =====================================================
    // Equip
    // =====================================================

    public void Equip(MaskData mask)
    {
        currentMask = mask;
        ApplyMask(mask);

        if (sr && sr.enabled)
            AnimateSpinY();
    }

    private void ApplyMask(MaskData mask)
    {
        if (!sr) return;

        if (mask == null || mask.sprite == null)
        {
            sr.enabled = false;
            sr.sprite = null;
            StopAllTweens();
            return;
        }

        sr.enabled = true;
        sr.sprite = mask.sprite;

        StartBob();
    }

    // =====================================================
    // ⭐ Rotate Y 360 Animation
    // =====================================================

    private void AnimateSpinY()
    {
        equipSeq?.Kill();

        float dirSign = facingLeft ? -1f : 1f;

        transform.localRotation = Quaternion.identity;

        equipSeq = DOTween.Sequence();

        equipSeq
            // 🌀 หมุนแกน Y 360
            .Append(transform.DOLocalRotate(
                new Vector3(0, spinDegrees * dirSign, 0),
                spinTime,
                RotateMode.FastBeyond360
            ).SetEase(spinEase))

            // ↺ เกินนิด
            .Append(transform.DOLocalRotate(
                new Vector3(0, -overshoot * dirSign, 0),
                settleTime * 0.6f
            ))

            // → กลับ 0
            .Append(transform.DOLocalRotate(
                Vector3.zero,
                settleTime
            ).SetEase(settleEase));
    }

    // =====================================================
    // Bobbing
    // =====================================================

    private void StartBob()
    {
        if (bobTween != null && bobTween.IsActive()) return;

        bobTween = transform
            .DOLocalMoveY(bobAmount, bobSpeed)
            .SetRelative()
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    // =====================================================
    // Cleanup
    // =====================================================

    private void StopAllTweens()
    {
        bobTween?.Kill();
        bobTween = null;

        equipSeq?.Kill();
        equipSeq = null;
    }

    void OnDisable()
    {
        StopAllTweens();
    }
}

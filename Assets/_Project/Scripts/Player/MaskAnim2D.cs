using UnityEngine;
using DG.Tweening;

public class MaskAnim2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private AudioEventChannelSO audioChannel;

    [Header("Clips")]
    [SerializeField] private AudioClip equipSfx;
    [SerializeField] private AudioClip failSfx;


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
    private float spinDegrees = 360f;
    private float spinTime = 0.35f;
    private float overshoot = 15f;
    private float settleTime = 0.12f;

    private Ease spinEase = Ease.OutCubic;
    private Ease settleEase = Ease.OutBack;


    // =============================
    // Fail Head Pop
    // =============================
    [Header("Fail Head Pop")]
    private float popHeight = 1.2f;
    private float popTime = 0.25f;

    private float spinSpeed = 720f;
    private float fallTime = 0.35f;

    private float fadeTime = 0.15f;
    private Ease popEase = Ease.OutQuad;
    private Ease fallEase = Ease.InQuad;

    [Header("Respawn Fade In")]
    private float respawnDelay = 0.25f;
    private float respawnFadeTime = 0.25f;



    private Tween bobTween;
    private Sequence equipSeq;
    private Sequence failSeq;
    private Vector3 initialLocalPos;


    private bool facingLeft;

    private const float DIR_EPS = 0.01f;

    // =====================================================

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!movement) movement = GetComponentInParent<PlayerMovement>();

        initialLocalPos = transform.localPosition;

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
        {
            AnimateSpinY();
            audioChannel.RaiseSfx(equipSfx, 1, 1);
        }



    }

    private void ApplyMask(MaskData mask)
    {
        if (!sr) return;

        if (mask == null || mask.player_sprite == null)
        {
            sr.enabled = false;
            sr.sprite = null;
            StopAllTweens();
            return;
        }

        sr.enabled = true;
        sr.sprite = mask.player_sprite;
        sr.color = Color.white;

        StartBob();
    }

    // =====================================================
    // ⭐ Rotate Y 360 Animation
    // =====================================================

    private void AnimateSpinY()
    {
        equipSeq?.Kill();

        if (audioChannel != null && equipSfx != null)
            audioChannel.RaiseSfx(equipSfx, 1f, 1f);

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

    public void AnimateFailHeadPop()
    {
        if (!sr) return;

        // 1. หยุดทุกอย่างและบังคับกลับไปตำแหน่งเริ่มต้นทันทีเพื่อกันตำแหน่งเพี้ยน
        StopAllTweens();
        failSeq?.Kill();
        transform.localPosition = initialLocalPos; // Reset ตำแหน่งก่อนเริ่มท่าใหม่
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // 2. เล่นเสียง
        if (audioChannel != null && failSfx != null)
            audioChannel.RaiseSfx(failSfx, 1, 1);

        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
        sr.enabled = true;

        float dirSign = facingLeft ? -1f : 1f;

        failSeq = DOTween.Sequence();

        failSeq
            // ⬆ POP ขึ้น (อ้างอิงจาก initialLocalPos เสมอ)
            .Append(transform.DOLocalMoveY(initialLocalPos.y + popHeight, popTime).SetEase(popEase))
            .Join(transform.DOLocalRotate(new Vector3(0, spinSpeed * dirSign, 0), popTime, RotateMode.FastBeyond360))

            // ⬇ ตกลง
            .Append(transform.DOLocalMoveY(initialLocalPos.y - 0.25f, fallTime).SetEase(fallEase))
            .Join(transform.DOLocalRotate(new Vector3(0, spinSpeed * 0.6f * dirSign, 0), fallTime, RotateMode.FastBeyond360))

            // 💨 fade out
            .Append(sr.DOFade(0f, fadeTime))

            // ⏳ รอ
            .AppendInterval(respawnDelay)

            // Reset ทุกอย่างกลับค่าเริ่มต้นก่อน Fade In
            .AppendCallback(() =>
            {
                transform.localPosition = initialLocalPos;
                transform.localRotation = Quaternion.identity;
            })

            // ✨ fade in กลับมา
            .Append(sr.DOFade(1f, respawnFadeTime))

            .OnComplete(() =>
            {
                StartBob();
            });
    }

    public void AnimateMaskRemove()
    {
        if (!sr) return;

        StopAllTweens(); // หยุด bob / equip

        failSeq?.Kill();

        Vector3 startPos = transform.localPosition;

        // reset
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
        sr.enabled = true;

        float dirSign = facingLeft ? -1f : 1f;

        failSeq = DOTween.Sequence();

        failSeq
            // =====================
            // ⬆ POP ขึ้น
            // =====================
            .Append(transform.DOLocalMoveY(startPos.y + popHeight, popTime).SetEase(popEase))
            .Join(transform.DOLocalRotate(
                new Vector3(0, spinSpeed * dirSign, 0),
                popTime,
                RotateMode.FastBeyond360))

            // =====================
            // ⬇ ตกลง
            // =====================
            .Append(transform.DOLocalMoveY(startPos.y - 0.25f, fallTime).SetEase(fallEase))
            .Join(transform.DOLocalRotate(
                new Vector3(0, spinSpeed * 0.6f * dirSign, 0),
                fallTime,
                RotateMode.FastBeyond360))

            // =====================
            // 💨 fade out
            // =====================
            .Append(sr.DOFade(0f, fadeTime))

            // =====================
            // ⏳ รอ
            // =====================
            .AppendInterval(respawnDelay)

            // reset position กลับก่อน fade in
            .AppendCallback(() =>
            {
                transform.localPosition = startPos;
                transform.localRotation = Quaternion.identity;
            });
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

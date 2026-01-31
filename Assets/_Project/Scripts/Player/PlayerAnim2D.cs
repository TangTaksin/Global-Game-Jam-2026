using UnityEngine;

public class PlayerAnim2D : MonoBehaviour
{
    public enum AnimState { Idle, Walk }

    [Header("Frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;

    [Header("FPS")]
    [SerializeField, Min(0.1f)] private float idleFps = 6f;
    [SerializeField, Min(0.1f)] private float walkFps = 10f;

    [Header("Movement detect")]
    [SerializeField] private float walkThreshold = 0.05f;

    [Header("WebGL / Timing")]
    [Tooltip("กัน dt กระโดด (เช่น alt-tab / browser throttling)")]
    [SerializeField] private float maxDeltaTime = 0.1f; // 0.05-0.15 แล้วแต่เกม

    [Tooltip("ใช้ unscaledDeltaTime (ไม่โดน timeScale)")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Refs (optional)")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement movement;

    private AnimState state = AnimState.Idle;
    private Sprite[] frames;
    private float fps;

    private double animTime;          // double กัน drift (ดีมากกับ WebGL)
    private int lastFrameIndex = -1;

    private const float DIR_EPS = 0.01f;

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        if (!movement) movement = GetComponentInParent<PlayerMovement>();

        SetState(AnimState.Idle, resetTime: true);
        ApplyFrame(0);
    }

    void Update()
    {
        if (!sr || !movement) return;

        float dir = movement.CurrentDirection;

        // Flip เฉพาะตอนมีทิศจริง
        if (Mathf.Abs(dir) > DIR_EPS)
            sr.flipX = dir < 0f;

        bool isWalking = Mathf.Abs(dir) > walkThreshold;
        SetState(isWalking ? AnimState.Walk : AnimState.Idle, resetTime: false);

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // WebGL: dt แกว่งบ่อย -> clamp กันกระโดดเฟรม
        if (dt > maxDeltaTime) dt = maxDeltaTime;
        if (dt < 0f) dt = 0f;

        Tick(dt);
    }

    private void SetState(AnimState newState, bool resetTime)
    {
        if (newState == state && frames != null) return;

        state = newState;

        frames = (state == AnimState.Walk) ? walkFrames : idleFrames;
        fps = Mathf.Max(0.1f, (state == AnimState.Walk) ? walkFps : idleFps);

        if (resetTime) animTime = 0.0;

        // เปลี่ยนคลิป -> บังคับอัปเดตเฟรมทันที
        lastFrameIndex = -1;

        if (frames == null || frames.Length == 0)
        {
            sr.sprite = null;
        }
    }

    private void Tick(float dt)
    {
        if (frames == null || frames.Length == 0) return;

        animTime += dt;

        // ป้องกัน animTime โตไม่สิ้นสุด (ช่วยความเสถียรระยะยาว)
        double clipLenSec = frames.Length / (double)fps;
        if (clipLenSec > 0.0 && animTime >= clipLenSec)
            animTime = animTime % clipLenSec;

        int frameIndex = (int)(animTime * fps) % frames.Length;

        if (frameIndex != lastFrameIndex)
            ApplyFrame(frameIndex);
    }

    private void ApplyFrame(int index)
    {
        lastFrameIndex = index;
        sr.sprite = frames[index];
    }
}

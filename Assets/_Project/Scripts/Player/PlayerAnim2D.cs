using UnityEngine;

public class PlayerAnim2D : MonoBehaviour
{
    [Header("Frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;

    [Header("FPS")]
    [SerializeField, Min(1f)] private float idleFps = 6f;
    [SerializeField, Min(1f)] private float walkFps = 10f;

    [Header("Movement detect")]
    [SerializeField] private float walkThreshold = 0.05f;

    [Header("Refs (optional)")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement movement;

    private Sprite[] currentFrames;
    private int frameIndex;
    private float timer;

    void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (movement == null) movement = GetComponentInParent<PlayerMovement>();

        // เริ่มต้นเป็น idle
        SetClip(idleFrames, true);
    }

    void Update()
    {
        if (sr == null || movement == null) return;

        float dir = movement.CurrentDirection;
        bool isWalking = Mathf.Abs(dir) > walkThreshold;

        // Flip
        if (Mathf.Abs(dir) > walkThreshold)
            sr.flipX = dir < 0f;

        // เลือกคลิป
        EnsureClip(isWalking ? walkFrames : idleFrames);

        // เล่นเฟรม
        float fps = isWalking ? walkFps : idleFps;
        if (currentFrames == null || currentFrames.Length == 0) return;

        timer += Time.deltaTime;
        float frameTime = 1f / fps;

        while (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex = (frameIndex + 1) % currentFrames.Length;
            sr.sprite = currentFrames[frameIndex];
        }
    }

    private void EnsureClip(Sprite[] frames)
    {
        if (currentFrames == frames) return;
        SetClip(frames, true);
    }

    private void SetClip(Sprite[] frames, bool reset)
    {
        currentFrames = frames;

        if (reset)
        {
            frameIndex = 0;
            timer = 0f;
        }

        if (currentFrames != null && currentFrames.Length > 0 && sr != null)
            sr.sprite = currentFrames[0];
    }
}

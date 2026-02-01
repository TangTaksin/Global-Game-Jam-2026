using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class MoveToTarget : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private Ease moveEase = Ease.OutBack;

    [Header("2D")]
    [SerializeField] private bool rotateToTarget = true;
    [SerializeField] private bool cuteSquashEffect = true;

    [Header("Events")]
    public UnityEvent OnReachedTarget;

    private Tween moveTween;
    private Tween rotateTween;
    private Tween scaleTween;

    public void StartMoving()
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("No target point assigned!");
            return;
        }

        // ✅ กัน Tween ซ้อน (สำคัญมาก)
        KillTweens();

        Vector3 targetPos = new Vector3(
            targetPoint.position.x,
            targetPoint.position.y,
            transform.position.z   // ล็อค Z สำหรับ 2D
        );

        Sequence seq = DOTween.Sequence();

        // =====================
        // หมุนแบบ 2D (แกน Z เท่านั้น)
        // =====================
        if (rotateToTarget)
        {
            Vector2 dir = targetPos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rotateTween = transform.DORotate(
                new Vector3(0, 0, angle),
                0.25f
            );
        }

        // =====================
        // เอฟเฟคน่ารัก squash ก่อนพุ่ง
        // =====================
        if (cuteSquashEffect)
        {
            scaleTween = transform.DOScale(
                new Vector3(1.15f, 0.85f, 1f),
                0.12f
            ).SetLoops(2, LoopType.Yoyo);
        }

        // =====================
        // Move
        // =====================
        moveTween = transform.DOMove(targetPos, duration)
            .SetEase(moveEase);

        seq.Append(moveTween);

        // =====================
        // เด้งตอนถึง (cute bounce)
        // =====================
        if (cuteSquashEffect)
        {
            seq.Append(
                transform.DOScale(1.2f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
            );
        }

        seq.OnComplete(() =>
        {
            OnReachedTarget?.Invoke();
        });
    }

    public void SetTarget(Transform newTarget)
    {
        targetPoint = newTarget;
    }

    private void KillTweens()
    {
        moveTween?.Kill();
        rotateTween?.Kill();
        scaleTween?.Kill();
    }

    private void OnDisable()
    {
        KillTweens(); // กัน memory leak / tween ค้าง
    }
}

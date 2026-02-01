using UnityEngine;
using DG.Tweening; // อย่าลืมเพิ่ม namespace นี้ครับ

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private bool spawnOnStart;

    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private float jumpPower = 1.5f;
    [SerializeField] private float punchAmount = 0.2f;

    void Start()
    {
        if (spawnOnStart) Spawn();
    }

    public void Spawn()
    {
        if (objectToSpawn == null) return;

        // 1. สร้าง Instance
        GameObject instance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

        // 2. ตั้งค่าเริ่มต้นให้ Scale เป็น 0 ก่อน (เพื่อให้ดูเหมือนขยายออกมา)
        Vector3 targetScale = instance.transform.localScale;
        instance.transform.localScale = Vector3.zero;

        // 3. เริ่มทำ Animation ด้วย Sequence
        Sequence spawnSeq = DOTween.Sequence();

        spawnSeq.Append(instance.transform.DOScale(targetScale, spawnDuration).SetEase(Ease.OutBack)) // ขยายจนเท่าเดิมแบบเด้งนิดๆ
                .Join(instance.transform.DOJump(instance.transform.position, jumpPower, 1, spawnDuration).SetEase(Ease.OutQuad)) // กระโดดขึ้น
                .Join(instance.transform.DOPunchRotation(new Vector3(0, 0, 15), spawnDuration)) // หมุนส่ายๆ เล็กน้อย
                .OnComplete(() => {
                    // เมื่อเด้งเสร็จแล้ว อาจจะเพิ่มเอฟเฟกต์สั่นเล็กน้อยเพื่อจบงาน
                    instance.transform.DOPunchScale(new Vector3(punchAmount, punchAmount, punchAmount), 0.2f);
                });
    }
}
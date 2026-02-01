using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MaskTable : MonoBehaviour, IInteractable
{
    [SerializeField] private TextMeshPro reqMaskNumText;

    public Vector3 position => transform.position;

    [SerializeField] private bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    [SerializeField] private MaskData[] requiredMasks;

    private Dictionary<MaskData, int> requiredCount = new Dictionary<MaskData, int>();
    private Dictionary<MaskData, int> submittedCount = new Dictionary<MaskData, int>();

    private int totalRequired = 0;
    private int totalSubmitted = 0;

    private MaskInventory mask_inv;
    private bool isCompleted = false;

    public UnityEvent ConditionMetEvent;

    void Awake() => Init();

    private void Init()
    {
        requiredCount.Clear();
        submittedCount.Clear();
        totalRequired = 0;
        totalSubmitted = 0;

        if (requiredMasks != null)
        {
            foreach (var m in requiredMasks)
            {
                if (m == null) continue;

                if (!requiredCount.ContainsKey(m)) requiredCount[m] = 0;
                requiredCount[m]++;

                if (!submittedCount.ContainsKey(m)) submittedCount[m] = 0;

                totalRequired++;
            }
        }

        UpdateText();
    }

    public void Interact(object interacter)
    {
        if (isCompleted || !isInteractable) return;
        if (totalRequired <= 0) return;

        if (mask_inv == null)
        {
            var inter = interacter as PlayerInteractor;
            if (inter != null && inter.transform.parent != null)
                mask_inv = inter.transform.parent.GetComponent<MaskInventory>();
        }

        if (mask_inv == null || mask_inv.MaskList == null || mask_inv.MaskList.Count == 0) return;

        // ดึง Current Mask มาเช็ค
        var currentMask = mask_inv.MaskList[mask_inv.CurrentMaskIndex];
        if (currentMask == null) return;

        // --- ส่วนที่เพิ่มใหม่: ดึงสคริปต์แอนิเมชันจากลูกของผู้เล่น ---
        MaskAnim2D maskAnim = mask_inv.GetComponentInChildren<MaskAnim2D>();

        // กรณีที่ 1: หน้ากากนี้ "ไม่เกี่ยวข้อง" กับโต๊ะนี้เลย หรือ "ส่งครบจำนวนที่ต้องการ" ไปแล้ว
        if (!requiredCount.ContainsKey(currentMask) || submittedCount[currentMask] >= requiredCount[currentMask])
        {
            if (maskAnim != null)
                maskAnim.AnimateFailHeadPop(); // 🚨 เล่นท่าส่ายหัว/เด้งหลุด
            return;
        }

        // ✅ กรณีที่ 2: หน้ากากถูกต้อง (Logic เดิมของคุณ)
        submittedCount[currentMask]++;
        totalSubmitted++;

        // ถ้าอยากให้ตอนส่งสำเร็จมีท่าทางด้วย สามารถเรียก maskAnim.AnimateMaskRemove() ตรงนี้ได้
        if (maskAnim != null)
            maskAnim.AnimateMaskRemove();

        mask_inv.RemoveMask(currentMask);
        UpdateText();

        if (totalSubmitted >= totalRequired)
        {
            isCompleted = true;
            isInteractable = false;
            ConditionMetEvent?.Invoke();
        }
    }

    private void UpdateText()
    {
        if (reqMaskNumText)
            reqMaskNumText.text = $"{totalSubmitted}/{totalRequired}";
    }
}

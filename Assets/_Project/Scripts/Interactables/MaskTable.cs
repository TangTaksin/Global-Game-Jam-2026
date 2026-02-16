using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MaskTable : MonoBehaviour, IInteractable
{
    // --- Enum สำหรับเลือกโหมดการส่งหน้ากาก ---
    public enum InventoryAction { RemoveOnSubmit, KeepInInventory }

    [Header("Settings")]
    [SerializeField] private InventoryAction _onSubmitAction = InventoryAction.RemoveOnSubmit;
    [SerializeField] private TextMeshPro reqMaskNumText;
    [SerializeField] private MaskData[] requiredMasks;

    [Header("Events")]
    public UnityEvent ConditionMetEvent;

    public Vector3 position => transform.position;

    [SerializeField] private bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    private Dictionary<MaskData, int> requiredCount = new Dictionary<MaskData, int>();
    private Dictionary<MaskData, int> submittedCount = new Dictionary<MaskData, int>();

    private int totalRequired = 0;
    private int totalSubmitted = 0;

    private MaskInventory mask_inv;
    private bool isCompleted = false;

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
        // 1. Check ถ้าเสร็จแล้ว หรือโต๊ะ Lock อยู่ ให้หยุดทำงาน
        if (isCompleted || !isInteractable) return;
        if (totalRequired <= 0) return;

        // 2. ดึง MaskInventory จาก Player
        if (mask_inv == null)
        {
            var inter = interacter as PlayerInteractor;
            if (inter != null && inter.transform.parent != null)
                mask_inv = inter.transform.parent.GetComponent<MaskInventory>();
        }

        if (mask_inv == null || mask_inv.MaskList == null || mask_inv.MaskList.Count == 0) return;

        // 3. เช็คหน้ากากที่สวมใส่อยู่ปัจจุบัน
        var currentMask = mask_inv.MaskList[mask_inv.CurrentMaskIndex];
        if (currentMask == null) return;

        // ดึง Script Animation ของหน้ากากบนตัวผู้เล่น
        MaskAnim2D maskAnim = mask_inv.GetComponentInChildren<MaskAnim2D>();

        // 4. กรณีหน้ากากไม่ตรงเงื่อนไข (ไม่ใช่ที่ต้องการ หรือส่งชนิดนี้ครบแล้ว)
        if (!requiredCount.ContainsKey(currentMask) || submittedCount[currentMask] >= requiredCount[currentMask])
        {
            if (maskAnim != null) maskAnim.AnimateFailHeadPop();
            return;
        }

        // ✅ 5. กรณีหน้ากากถูกต้อง
        submittedCount[currentMask]++;
        totalSubmitted++;

        // จัดการเรื่อง Animation และ Inventory ตาม Enum ที่เลือกไว้
        if (_onSubmitAction == InventoryAction.RemoveOnSubmit)
        {
            if (maskAnim != null) maskAnim.AnimateMaskRemove();
            mask_inv.RemoveMask(currentMask);
        }
        else
        {
            // ถ้าเลือกแบบ Keep ให้เล่นท่า Spin (Equip) เพื่อบอกว่าผ่าน
            if (maskAnim != null) maskAnim.Equip(currentMask);
        }

        UpdateText();

        // 6. เช็คว่าส่งครบทั้งหมดหรือยัง
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
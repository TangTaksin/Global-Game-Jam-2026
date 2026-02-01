using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PeopleMaskTrigger : MonoBehaviour, IInteractable
{
    public enum InventoryAction { RemoveOnSubmit, KeepInInventory }

    [System.Serializable]
    public class MaskPhase
    {
        public string phaseName = "New Phase";
        public MaskData[] requiredMasks;
        public UnityEvent OnPhaseCompleted; // Event เฉพาะของแต่ละชั้น
    }

    [Header("Settings")]
    [SerializeField] private InventoryAction _onSubmitAction = InventoryAction.RemoveOnSubmit;
    [SerializeField] private TextMeshPro reqMaskNumText;

    [Header("Multi-Phase Configuration")]
    [SerializeField] private List<MaskPhase> phases = new List<MaskPhase>();
    private int currentPhaseIndex = 0;

    [Header("Global Events")]
    public UnityEvent AllPhasesCompletedEvent;

    public Vector3 position => transform.position;

    [SerializeField] private bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    private Dictionary<MaskData, int> requiredCount = new Dictionary<MaskData, int>();
    private Dictionary<MaskData, int> submittedCount = new Dictionary<MaskData, int>();

    private int totalRequired = 0;
    private int totalSubmitted = 0;
    private bool allPhasesFinished = false;

    private MaskInventory mask_inv;

    void Awake() => InitPhase(0);

    // ทำการ Load เงื่อนไขของ Phase ที่กำหนด
    private void InitPhase(int index)
    {
        if (phases == null || index >= phases.Count)
        {
            allPhasesFinished = true;
            isInteractable = false;
            return;
        }

        currentPhaseIndex = index;
        requiredCount.Clear();
        submittedCount.Clear();
        totalRequired = 0;
        totalSubmitted = 0;

        MaskPhase currentPhase = phases[currentPhaseIndex];

        if (currentPhase.requiredMasks != null)
        {
            foreach (var m in currentPhase.requiredMasks)
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
        if (allPhasesFinished || !isInteractable) return;
        if (totalRequired <= 0) return;

        if (mask_inv == null)
        {
            var inter = interacter as PlayerInteractor;
            if (inter != null && inter.transform.parent != null)
                mask_inv = inter.transform.parent.GetComponent<MaskInventory>();
        }

        if (mask_inv == null || mask_inv.MaskList == null || mask_inv.MaskList.Count == 0) return;

        var currentMask = mask_inv.MaskList[mask_inv.CurrentMaskIndex];
        if (currentMask == null) return;

        MaskAnim2D maskAnim = mask_inv.GetComponentInChildren<MaskAnim2D>();

        // เช็คเงื่อนไขหน้ากากใน Phase ปัจจุบัน
        if (!requiredCount.ContainsKey(currentMask) || submittedCount[currentMask] >= requiredCount[currentMask])
        {
            if (maskAnim != null) maskAnim.AnimateFailHeadPop();
            return;
        }

        // ✅ ผ่านเงื่อนไขชิ้นนี้
        submittedCount[currentMask]++;
        totalSubmitted++;

        if (_onSubmitAction == InventoryAction.RemoveOnSubmit)
        {
            if (maskAnim != null) maskAnim.AnimateMaskRemove();
            mask_inv.RemoveMask(currentMask);
        }
        else
        {
            if (maskAnim != null) maskAnim.Equip(currentMask);
        }

        UpdateText();

        // ✅ เช็คว่าจบ Phase ปัจจุบันหรือยัง
        if (totalSubmitted >= totalRequired)
        {
            CompleteCurrentPhase();
        }
    }

    private void CompleteCurrentPhase()
    {
        // รัน Event ของ Phase นั้นๆ (เช่น สั่งให้ ObjectSpawner ทำงาน)
        phases[currentPhaseIndex].OnPhaseCompleted?.Invoke();

        int nextIndex = currentPhaseIndex + 1;

        if (nextIndex < phases.Count)
        {
            // ถ้ายังมี Phase ต่อไป ให้เริ่ม Phase ใหม่
            Debug.Log($"Phase {currentPhaseIndex} Done! Loading Phase {nextIndex}");
            InitPhase(nextIndex);
        }
        else
        {
            // ถ้าหมดทุก Phase แล้ว
            allPhasesFinished = true;
            isInteractable = false;
            AllPhasesCompletedEvent?.Invoke();
            Debug.Log("All Phases Completed!");
        }
    }

    private void UpdateText()
    {
        if (reqMaskNumText)
            reqMaskNumText.text = $"{totalSubmitted}/{totalRequired}";
    }
}

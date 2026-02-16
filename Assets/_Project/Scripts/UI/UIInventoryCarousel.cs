using System.Collections.Generic;
using UnityEngine;


public class UIInventoryCarousel : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private bool billboardToCamera = true;

    [Header("References")]
    [SerializeField] private MaskInventory inventory;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Slot UI (3 Slots)")]
    [SerializeField] private MaskSlotUI leftSlot;
    [SerializeField] private MaskSlotUI centerSlot;
    [SerializeField] private MaskSlotUI rightSlot;

    [Header("Visibility")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeSpeed = 12f;

    [Header("Visual Effects")]
    [SerializeField] private bool dimSideSlots = true;
    [SerializeField][Range(0f, 1f)] private float sideAlpha = 0.55f;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip openInventorySfx;
    [SerializeField] private AudioClip closeInventorySfx;
    [SerializeField] private AudioClip switchSfx;
    [SerializeField] private AudioEventChannelSO audioChannel;

    private Camera mainCamera;
    private float targetAlpha;
    private List<MaskData> cachedMaskData;

    // Cache slot canvas groups to avoid GetComponent calls
    private CanvasGroup leftSlotCG;
    private CanvasGroup centerSlotCG;
    private CanvasGroup rightSlotCG;

    private const float ALPHA_THRESHOLD = 0.99f;

    #region Unity Lifecycle

    private void Awake()
    {
        mainCamera = Camera.main;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        CacheSlotCanvasGroups();
        SetVisibleInstant(false);
        ApplyEmptySlots();
    }

    private void OnEnable()
    {
        if (inventory == null) return;

        inventory.OnToggle += HandleToggleInventory;
        inventory.OnChanged += HandleInventoryChanged;
        inventory.OnIndexChanged += HandleIndexChanged;

        // Initialize with current state
        HandleInventoryChanged(inventory.MaskList);
        HandleIndexChanged(inventory.CurrentMaskIndex);
    }

    private void OnDisable()
    {
        if (inventory == null) return;

        inventory.OnToggle -= HandleToggleInventory;
        inventory.OnChanged -= HandleInventoryChanged;
        inventory.OnIndexChanged -= HandleIndexChanged;
    }

    private void LateUpdate()
    {
        UpdatePosition();
        UpdateBillboard();
        UpdateFade();
    }

    #endregion

    #region Update Methods

    private void UpdatePosition()
    {
        if (target != null)
            transform.position = target.position + worldOffset;
    }

    private void UpdateBillboard()
    {
        if (billboardToCamera && mainCamera != null)
            transform.forward = mainCamera.transform.forward;
    }

    private void UpdateFade()
    {
        if (!canvasGroup || !useFade) return;

        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.unscaledDeltaTime
        );

        bool isVisible = canvasGroup.alpha > ALPHA_THRESHOLD;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    #endregion

    #region Event Handlers

    private void HandleInventoryChanged(List<MaskData> newData)
    {
        cachedMaskData = newData;
        RefreshSlots(inventory != null ? inventory.CurrentMaskIndex : 0);
    }

    private void HandleIndexChanged(int index)
    {
        RefreshSlots(index);

        if (audioChannel != null && switchSfx != null && targetAlpha > 0.5f)
        {
            audioChannel.RaiseSfx(switchSfx, 1f, 1f);
        }
    }

    private void HandleToggleInventory(bool isOpen)
    {
        if (audioChannel != null)
        {
            AudioClip clipToPlay = isOpen ? openInventorySfx : closeInventorySfx;
            if (clipToPlay != null)
            {
                audioChannel.RaiseSfx(clipToPlay, 1f, 1f);
            }
        }
        if (canvasGroup == null)
        {
            gameObject.SetActive(isOpen);
            return;
        }

        if (useFade)
        {
            targetAlpha = isOpen ? 1f : 0f;
            canvasGroup.blocksRaycasts = isOpen;
            canvasGroup.interactable = isOpen;
        }
        else
        {
            SetVisibleInstant(isOpen);
        }
    }

    #endregion

    #region Slot Management

    private void RefreshSlots(int centerIndex)
    {
        int count = cachedMaskData?.Count ?? 0;

        if (count <= 0)
        {
            ApplyEmptySlots();
            return;
        }

        // Calculate wrapped indices
        int center = WrapIndex(centerIndex, count);
        int left = WrapIndex(center - 1, count);
        int right = WrapIndex(center + 1, count);

        // Update slot icons
        SetData(leftSlot, cachedMaskData[left]);
        SetData(centerSlot, cachedMaskData[center]);
        SetData(rightSlot, cachedMaskData[right]);

        // Update slot alpha
        float leftAlpha = dimSideSlots ? sideAlpha : 1f;
        float rightAlpha = dimSideSlots ? sideAlpha : 1f;

        SetSlotAlpha(leftSlotCG, leftAlpha);
        SetSlotAlpha(centerSlotCG, 1f);
        SetSlotAlpha(rightSlotCG, rightAlpha);
    }

    private void ApplyEmptySlots()
    {

        SetData(leftSlot, null);
        SetData(centerSlot, null);
        SetData(rightSlot, null);

        SetSlotAlpha(leftSlotCG, 0f);
        SetSlotAlpha(centerSlotCG, 0f);
        SetSlotAlpha(rightSlotCG, 0f);
    }

    private void SetData(MaskSlotUI slot, MaskData data)
    {
        if (slot == null) return;

        slot.SetData(data?.inventory_sprite, data?.name_mask);

    }

    private void CacheSlotCanvasGroups()
    {
        leftSlotCG = GetOrAddCanvasGroup(leftSlot);
        centerSlotCG = GetOrAddCanvasGroup(centerSlot);
        rightSlotCG = GetOrAddCanvasGroup(rightSlot);
    }

    private static CanvasGroup GetOrAddCanvasGroup(MaskSlotUI slot)
    {
        if (slot == null) return null;

        CanvasGroup cg = slot.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = slot.gameObject.AddComponent<CanvasGroup>();

        return cg;
    }

    private static void SetSlotAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }

    #endregion

    #region Visibility

    private void SetVisibleInstant(bool visible)
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(visible);
            return;
        }

        float alpha = visible ? 1f : 0f;
        canvasGroup.alpha = alpha;
        targetAlpha = alpha;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    #endregion

    #region Utilities

    private static int WrapIndex(int index, int count)
    {
        int wrapped = index % count;
        return wrapped < 0 ? wrapped + count : wrapped;
    }

    #endregion
}
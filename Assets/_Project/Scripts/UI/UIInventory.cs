using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private bool billboardToCamera = true;

    [Header("Refs")]
    [SerializeField] private MaskInventory inventory;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI Build")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private MaskSlotUI slotPrefab;
    [SerializeField, Min(0)] private int maxSlots = 0;

    [Header("Show/Hide")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeSpeed = 12f;

    private Camera cam;
    private float targetAlpha;
    private readonly List<MaskSlotUI> slots = new List<MaskSlotUI>();

    void Awake()
    {
        cam = Camera.main;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        SetVisibleInstant(false);
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnToggle += OnToggleInventory;
            inventory.OnChanged += Rebuild;              // ✅ Rebuild(List<MaskData>)
            // inventory.OnIndexChanged += RefreshSelection;

            // initial draw
            Rebuild(inventory.MaskList);
            // RefreshSelection(inventory.CurrentMaskIndex);
        }
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnToggle -= OnToggleInventory;
            inventory.OnChanged -= Rebuild;
            // inventory.OnIndexChanged -= RefreshSelection;
        }
    }

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + worldOffset;

        if (billboardToCamera && cam != null)
            transform.forward = cam.transform.forward;

        if (!canvasGroup) return;

        if (useFade)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.unscaledDeltaTime
            );

            bool interact = canvasGroup.alpha > 0.99f;
            canvasGroup.interactable = interact;
            canvasGroup.blocksRaycasts = interact;
        }
    }

    // =========================
    // Toggle show/hide
    // =========================

    private void OnToggleInventory(bool open)
    {
        if (!canvasGroup)
        {
            gameObject.SetActive(open);
            return;
        }

        if (!useFade)
        {
            SetVisibleInstant(open);
            return;
        }

        targetAlpha = open ? 1f : 0f;

        canvasGroup.blocksRaycasts = open;
        canvasGroup.interactable = open;
    }

    private void SetVisibleInstant(bool visible)
    {
        if (!canvasGroup)
        {
            gameObject.SetActive(visible);
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        targetAlpha = canvasGroup.alpha;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    // =========================
    // Build slots from inventory
    // =========================

    private void Rebuild(List<MaskData> datas)
    {
        if (contentParent == null || slotPrefab == null)
            return;

        // clear old
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] != null) Destroy(slots[i].gameObject);
        slots.Clear();

        int count = (datas != null) ? datas.Count : 0;
        if (maxSlots > 0) count = Mathf.Min(count, maxSlots);

        int selectedIndex = (inventory != null) ? inventory.CurrentMaskIndex : -1;

        for (int i = 0; i < count; i++)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            var data = datas[i];

            slot.SetIcon(data != null ? data.inventory_sprite : null);
            // slot.SetSelected(i == selectedIndex);

            slots.Add(slot);
        }
    }

    // private void RefreshSelection(int index)
    // {
    //     for (int i = 0; i < slots.Count; i++)
    //         slots[i].SetSelected(i == index);
    // }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInventory : MonoBehaviour
{
    MaskAnim2D _maskAnim;

    [Header("Data")]
    [SerializeField] private List<MaskData> _maskList = new List<MaskData>();
    public List<MaskData> MaskList => _maskList;

    [Header("State")]
    [SerializeField] private int _currentMaskIndex;
    public int CurrentMaskIndex => _currentMaskIndex;

    [SerializeField] private bool _isInventoryOpened;
    public bool IsInventoryOpened => _isInventoryOpened;

    [Header("Input Action Names")]
    [SerializeField] private string navigateActionName = "Move";
    [SerializeField] private string toggleInventoryActionName = "ToggleInventory";

    private InputAction navigateAction;
    private InputAction toggleInventoryAction;
    private bool _inputEnabled = true;
    public event Action<bool> OnToggle;
    public event Action<int> OnIndexChanged;
    public event Action<List<MaskData>> OnChanged;

    private const float AXIS_DEADZONE = 0.5f;

    void Awake()
    {
        Init();
        ClampIndex();
    }

    void Start()
    {
        // ✅ ยิง initial state ตอน Start (UI ส่วนมากจะ subscribe แล้ว)
        OnChanged?.Invoke(_maskList);
        OnIndexChanged?.Invoke(_currentMaskIndex);
        OnToggle?.Invoke(_isInventoryOpened);
    }

    void OnEnable()
    {
        StartListeningForInput();
    }

    void OnDisable()
    {
        StopListeningForInput();
    }

    private void Init()
    {
        _maskAnim = GetComponentInChildren<MaskAnim2D>();

        navigateAction = InputSystem.actions.FindAction(navigateActionName);
        toggleInventoryAction = InputSystem.actions.FindAction(toggleInventoryActionName);

#if UNITY_EDITOR
        if (navigateAction == null) Debug.LogWarning($"[MaskInventory] Action '{navigateActionName}' not found.", this);
        if (toggleInventoryAction == null) Debug.LogWarning($"[MaskInventory] Action '{toggleInventoryActionName}' not found.", this);
#endif
    }

    private void StartListeningForInput()
    {
        if (navigateAction != null)
        {
            navigateAction.performed += SwitchMask;
            navigateAction.Enable();
        }

        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.started += ToggleInventory;
            toggleInventoryAction.Enable();
        }
    }

    private void StopListeningForInput()
    {
        if (navigateAction != null)
        {
            navigateAction.performed -= SwitchMask;
            navigateAction.Disable();
        }

        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.started -= ToggleInventory;
            toggleInventoryAction.Disable();
        }
    }

    public void Enableinput(bool value)
    {
        _inputEnabled = value;
    }

    public void ToggleInventory(InputAction.CallbackContext ctx)
    {
        if (!_inputEnabled)
            return;
        
        _isInventoryOpened = !_isInventoryOpened;
        OnToggle?.Invoke(_isInventoryOpened);

        //change mask
        // เมื่อปิด Inventory ให้ยืนยันการใส่หน้ากากใบที่เลือกอยู่
        if (!_isInventoryOpened)
        {
            EquipCurrentMask();
        }
    }

    private void EquipCurrentMask()
    {
        if (_maskList.Count > 0 && _currentMaskIndex < _maskList.Count)
        {
            MaskData selectedMask = _maskList[_currentMaskIndex];
            _maskAnim = GetComponentInChildren<MaskAnim2D>();
            if (_maskAnim != null) _maskAnim.Equip(selectedMask);
        }
    }

    public void SwitchMask(InputAction.CallbackContext ctx)
    {
        if (!_isInventoryOpened) return;
        if (_maskList == null || _maskList.Count == 0) return;

        // NOTE: ถ้า Move ของคุณเป็น Vector2 ต้องเปลี่ยนไป ReadValue<Vector2>()
        float axis = ctx.ReadValue<float>();

        int step = axis > AXIS_DEADZONE ? 1 : axis < -AXIS_DEADZONE ? -1 : 0;
        if (step == 0) return;

        _currentMaskIndex += step;

        if (_currentMaskIndex > _maskList.Count - 1)
            _currentMaskIndex = 0;
        else if (_currentMaskIndex < 0)
            _currentMaskIndex = _maskList.Count - 1;

        OnIndexChanged?.Invoke(_currentMaskIndex);
    }

    public void AddMask(MaskData mask)
    {
        if (mask == null) return;
        if (_maskList.Contains(mask)) return;

        _maskList.Add(mask);
        ClampIndex();

        OnChanged?.Invoke(_maskList);
        OnIndexChanged?.Invoke(_currentMaskIndex);
    }

    public void RemoveMask(MaskData mask)
    {
        if (mask == null) return;

        if (_maskList.Remove(mask))
        {
            ClampIndex();
            OnChanged?.Invoke(_maskList);
            OnIndexChanged?.Invoke(_currentMaskIndex);
        }
    }

    private void ClampIndex()
    {
        if (_maskList == null || _maskList.Count == 0)
        {
            _currentMaskIndex = 0;
            return;
        }

        _currentMaskIndex = Mathf.Clamp(_currentMaskIndex, 0, _maskList.Count - 1);
    }

    // optional: ให้ UI เรียกเพื่อรีเฟรชเอง
    public void ForceRefresh()
    {
        OnChanged?.Invoke(_maskList);
        OnIndexChanged?.Invoke(_currentMaskIndex);
        OnToggle?.Invoke(_isInventoryOpened);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInventory : MonoBehaviour
{
    public List<MaskData> _maskList = new List<MaskData>();
    public List<MaskData> MaskList => _maskList;
    int _currentMaskIndex;
    public int CurrentMaskIndex => _currentMaskIndex;

    bool _isInventoryOpened;
    public bool IsInventoryOpened => _isInventoryOpened;

    InputAction navigateAction;
    InputAction toggleIventoryAction;
    private bool _inputEnabled = true;

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        StartListeningForInput();
    }

    void OnDisable()
    {
        StopListeningForInput();
    }


    void Init()
    {
        _maskList.Add(null);

        navigateAction = InputSystem.actions.FindAction("Move");
        toggleIventoryAction = InputSystem.actions.FindAction("ToggleInventory");
    }

    void StartListeningForInput()
    {
        navigateAction.performed += SwitchMask;
        toggleIventoryAction.started += ToggleInventory;
    }

    void StopListeningForInput()
    {
        navigateAction.performed -= SwitchMask;
        toggleIventoryAction.started -= ToggleInventory;
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

        if (!_isInventoryOpened) // closing
        {
            // wear selected mask

        }
    }

    public void SwitchMask(InputAction.CallbackContext ctx)
    {
        if (!_isInventoryOpened)
            return;

        var axis = ctx.ReadValue<float>();
        _currentMaskIndex += (int) axis;
        
        if (_currentMaskIndex > _maskList.Count-1)
        {
            _currentMaskIndex = 0;
        }
        else if (_currentMaskIndex < 0)
        {
            _currentMaskIndex = _maskList.Count-1;
        }
    }

    public void AddMask(MaskData mask)
    {
        _maskList.Add(mask);
    }

    public void RemoveMask(MaskData mask)
    {
        _maskList.Remove(mask);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening; // เพิ่ม DOTween เพื่อความสมูท

public class PlayerInteractor : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private CanvasGroup interactUI;
    [SerializeField] private float fadeDuration = 0.15f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioEventChannelSO audioChannel;
    [SerializeField] private AudioClip uiPopSfx;       // เสียงตอนปุ่ม Interact เด้งขึ้นมา
    [SerializeField] private AudioClip interactSfx;    // เสียงตอนกด Interact สำเร็จ

    InputAction interactAction;
    List<IInteractable> _interactableList = new List<IInteractable>();
    IInteractable _selectedInteractable;
    IInteractable _lastSelected; // ไว้เช็คเพื่อเล่นเสียง Discovery แค่ครั้งเดียว
    bool _inputEnabled = true;

    void Awake() => Init();

    private void OnEnable() => StartListeningForInputs();
    private void OnDisable() => StopListeningForInputs();

    void Update()
    {
        DecideSelectedInteractable();
        UpdateUI(); 
    }

    void UpdateUI()
    {
        if (interactUI == null) return;

        bool shouldShow = _selectedInteractable != null && _inputEnabled;
        float targetAlpha = shouldShow ? 1 : 0;

        // ถ้าสถานะเปลี่ยน (มีการพบวัตถุใหม่)
        if (shouldShow && _selectedInteractable != _lastSelected)
        {
            // เล่นเสียง Discovery
            if (audioChannel != null && uiPopSfx != null)
                audioChannel.RaiseSfx(uiPopSfx, 1f, Random.Range(0.95f, 1.05f));

            // ทำ UI Animation เล็กๆ (Punch Scale)
            interactUI.transform.DOKill();
            interactUI.transform.localScale = Vector3.one * 0.8f;
            interactUI.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            
            _lastSelected = _selectedInteractable;
        }
        else if (!shouldShow)
        {
            _lastSelected = null;
        }

        // ปรับค่า Alpha แบบนุ่มนวล
        interactUI.alpha = Mathf.Lerp(interactUI.alpha, targetAlpha, Time.deltaTime * 15f);
        interactUI.blocksRaycasts = shouldShow;
    }

    public void Enableinput(bool value) => _inputEnabled = value;

    void Init()
    {
        if (interactUI != null) interactUI.alpha = 0;
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void StartListeningForInputs() => interactAction.started += OnInteractInput;
    void StopListeningForInputs() => interactAction.started -= OnInteractInput;

    void OnTriggerEnter2D(Collider2D collision)
    {
        var interact = collision.GetComponent<IInteractable>();
        if (interact != null) _interactableList.Add(interact);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var interact = collision.GetComponent<IInteractable>();
        if (_interactableList.Contains(interact))
        {
            if (_selectedInteractable == interact) _selectedInteractable = null;
            _interactableList.Remove(interact);
        }
    }

    void OnInteractInput(InputAction.CallbackContext context)
    {
        if (!_inputEnabled || _selectedInteractable == null) return;

        // 🔊 เล่นเสียง Interact
        if (audioChannel != null && interactSfx != null)
            audioChannel.RaiseSfx(interactSfx, 1f, 1f);

        _selectedInteractable.Interact(this);
    }

    void DecideSelectedInteractable()
    {
        if (_interactableList.Count == 0)
        {
            _selectedInteractable = null;
            return;
        }

        float ClosestDistance = float.MaxValue;
        IInteractable bestSelection = null;

        foreach (var interact in _interactableList)
        {
            if (!interact.isInteractable) continue;

            float distance = (interact.position - transform.position).sqrMagnitude;
            if (distance < ClosestDistance)
            {
                ClosestDistance = distance;
                bestSelection = interact;
            }
        }
        _selectedInteractable = bestSelection;
    }
}
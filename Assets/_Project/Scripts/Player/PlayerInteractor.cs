using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    InputAction interactAction;

    List<IInteractable> _interactableList = new List<IInteractable>();
    IInteractable _selectedInteractable;
    int debug_interactable_count;

    void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        StartListeningForInputs();
    }

    private void OnDisable()
    {
        StopListeningForInputs();
    }

    void Update()
    {
         DecideSelectedInteractable();
    }


    void Init()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void StartListeningForInputs()
    {
        interactAction.started += OnInteractInput;
    }

    void StopListeningForInputs()
    {
        interactAction.started -= OnInteractInput;

    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        var interact = collision.GetComponent<IInteractable>();

        if (interact is IInteractable)
        {
            _interactableList.Add(interact);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var interact = collision.GetComponent<IInteractable>();

        if (_interactableList.Contains(interact))
        {
            if (_selectedInteractable == interact)
                _selectedInteractable = null;

            _interactableList.Remove(interact);

        }

        if (_interactableList.Count == 0)
        {
            _selectedInteractable = null;
        }
    }

    
    void OnInteractInput(InputAction.CallbackContext context)
    {
        print("attempting to interact");
        
        if (_selectedInteractable != null)
        {
            print("interaction successful!");
            _selectedInteractable.Interact(this);
        }
    }

    void DecideSelectedInteractable()
    {
        debug_interactable_count = _interactableList.Count;

        if (_interactableList.Count == 0)
            return;

        float ClosestDistance = float.MaxValue;
        Vector3 currentPosition = transform.position;

        foreach (var interact in _interactableList)
        {
            if (!interact.isInteractable)
                continue;

            Vector3 DifferenceToTarget = interact.position - currentPosition;
            float DistanceToTarget = DifferenceToTarget.sqrMagnitude;

            if (DistanceToTarget < ClosestDistance)
            {
                ClosestDistance = DistanceToTarget;
                _selectedInteractable = interact;
            }
        }
    }
}

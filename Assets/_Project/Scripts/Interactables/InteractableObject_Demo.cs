using UnityEngine;

public class InteractableObject_Demo : MonoBehaviour, IInteractable
{
    public Vector3 position => transform.position;

    bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    public void Interact(object interacter)
    {
        print(string.Format("{0} interacted with {1}.", interacter, this));
    }
}

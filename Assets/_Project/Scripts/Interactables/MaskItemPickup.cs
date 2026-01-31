using System;
using UnityEngine;

public class MaskItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] MaskData _maskData;
    [SerializeField] bool _disableAfterCollected = true;

    public Vector3 position => transform.position;

    [SerializeField] bool _isInteractable = true;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        ApplyWorldSprite();
    }

    private void ApplyWorldSprite()
    {
        if (!sr || _maskData == null) return;

        sr.sprite = _maskData.world_sprite;
    }

    public void Interact(object interacter)
    {
        var p_inter = interacter as PlayerInteractor;
        var m_inv = p_inter.transform.parent.GetComponent<MaskInventory>();

        m_inv.AddMask(_maskData);

        if (_disableAfterCollected)
            gameObject.SetActive(false);
    }
}

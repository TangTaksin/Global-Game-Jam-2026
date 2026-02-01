using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class MaskTable : MonoBehaviour, IInteractable
{
    [SerializeField] TextMeshPro reqMaskNumText;

    public Vector3 position => transform.position;

    [SerializeField] private bool _isInteractable;
    public bool isInteractable { get => _isInteractable; set => _isInteractable = value; }

    [SerializeField] MaskData[] requiredMasks;
    Dictionary<MaskData, bool> MasksCheck = new Dictionary<MaskData, bool>();

    int matchMasksCount;
    MaskInventory mask_inv;

    public UnityEvent ConditionMetEvent;

    void Awake()
    {
         Init();
    }

    void Init()
    {
        foreach (var m in requiredMasks)
        {
            MasksCheck[m] = false;
        }

        TextUpdate();
    }

    public void Interact(object interacter)
    {
        matchMasksCount = 0;

        if (!mask_inv)
        {
            var inter = interacter as PlayerInteractor;
            mask_inv = inter.transform.parent.GetComponent<MaskInventory>();
        }

        if (mask_inv.MaskList.Count < 1)
            return;

        var _mask = mask_inv.MaskList[mask_inv.CurrentMaskIndex];

        if (requiredMasks.Contains(_mask))
        {
            MasksCheck[_mask] = true;
        }
        
        foreach (var condt in MasksCheck)
        {
            if (condt.Value)
                matchMasksCount++;
        }

        TextUpdate();

        if (matchMasksCount >= requiredMasks.Length)
        {
            ConditionMetEvent.Invoke();

            foreach (var m in requiredMasks)
            {
                if (mask_inv.MaskList.Contains(m))
                {
                    mask_inv.RemoveMask(m);
                }
            }
        }
    }

    void TextUpdate()
    {
        if (reqMaskNumText)
            reqMaskNumText.text = string.Format("{0}/{1}", matchMasksCount, requiredMasks.Length);
        
    }
}

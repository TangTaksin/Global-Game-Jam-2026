using System.Linq.Expressions;
using UnityEngine;

public class PlayerState_Inventory : PlayerState
{
    public override void EnterState()
    {
        _playerManager.p_interacter.Enableinput(false);
        _playerManager.p_mover.Enableinput(false);
        _playerManager.m_inventory.Enableinput(true);
    }

    public override void UpdateState()
    {
       if (!_playerManager.m_inventory.IsInventoryOpened)
        {
            _playerManager.ChangeState<PlayerState_Normal>();
        }
    }
}

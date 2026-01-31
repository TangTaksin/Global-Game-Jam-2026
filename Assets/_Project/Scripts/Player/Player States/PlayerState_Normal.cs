using UnityEngine;

public class PlayerState_Normal : PlayerState
{
    public override void EnterState()
    {
        _playerManager.p_interacter.Enableinput(true);
        _playerManager.p_mover.Enableinput(true);
        _playerManager.m_inventory.Enableinput(true);
    }

    public override void UpdateState()
    {
       if ( _playerManager.m_inventory.IsInventoryOpened)
        {
            _playerManager.ChangeState<PlayerState_Inventory>();
        }
    }
}

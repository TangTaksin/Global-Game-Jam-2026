using UnityEngine;

public class PlayerState : IState
{
    protected PlayerManager _playerManager;

    public void Init(PlayerManager p_manager)
    {
        _playerManager = p_manager;
    }

    public virtual void EnterState()
    {
        
    }

    public virtual void ExitState()
    {
        
    }

    public virtual void UpdateState()
    {
        
    }
}
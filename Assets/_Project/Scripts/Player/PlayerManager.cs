using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerMovement _pMover;
    [SerializeField] PlayerInteractor _pInteracter;
    [SerializeField] MaskInventory _mInventory;

    public PlayerMovement p_mover => _pMover;
    public PlayerInteractor p_interacter => _pInteracter;
    public MaskInventory m_inventory => _mInventory;
    

    private readonly Dictionary<Type, PlayerState> _states = new Dictionary<Type, PlayerState>(16);
    public Type CurrentStateType => _currentState?.GetType(); // debug
    private PlayerState _currentState;


    [SerializeField] bool debug_normal_state_on_start = true;


    void Awake()
    {
         Init();
    }

    void Init()
    {
        // register all possible state 
        RegisterState(new PlayerState_Normal());
        RegisterState(new PlayerState_Inventory());

        if (debug_normal_state_on_start)
        {
            ChangeState<PlayerState_Normal>();
        }
    }

    public void RegisterState(PlayerState stateToRegister)
    {
        if (stateToRegister == null) 
            return;

        stateToRegister.Init(this);
        Type typeOfState = stateToRegister.GetType();
        _states[typeOfState] = stateToRegister;
    }

    private void Update()
    {
        _currentState?.UpdateState();
    }


    public void ChangeState(PlayerState next)
    {
        if (next == null) return;
        if (ReferenceEquals(_currentState, next)) return;

        _currentState?.ExitState();
        _currentState = next;
        _currentState.EnterState();
    }

    public void ChangeState<T>() where T : PlayerState
    {
        Type t = typeof(T);

        if (!_states.TryGetValue(t, out PlayerState next))
        {
            Debug.LogError($"State not registered: {t.Name}. Register it before switching.");
            return;
        }

        ChangeState(next);
    }
}
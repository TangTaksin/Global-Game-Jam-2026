using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputAction moveAction;

    Rigidbody2D _charBody;

    [SerializeField] float baseSpeed = 5f;
    float direction;

    void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        StartListeningForInputs();
    }

    void Init()
    {
        _charBody = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
    }

    void StartListeningForInputs()
    {
        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<float>();
    }

    void MovementProcess()
    {
        var velo = direction*baseSpeed*Time.deltaTime;
        if (Mathf.Abs(direction) > 0)
            _charBody.position += new Vector2(velo, 0);
    }

    void Update()
    {
        MovementProcess();
    }

    private void OnDisable()
    {
        StopListeningForInputs();
    }

    void StopListeningForInputs()
    {
        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputAction moveAction;

    Rigidbody2D _charBody;

    [SerializeField] float baseSpeed = 5f;
    float direction;
    public float CurrentDirection => direction;
    bool _inputEnabled = true;

    [Header("Ground Check")]
    [SerializeField] Transform _groundCheckPoint;
    [SerializeField] float _checkRadius = 0.2f;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _groundedDelay = 0.15f; // ระยะเวลา Delay (วินาที)

    float _groundCounter; // ตัวนับเวลา
    bool _isGrounded;


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

    public void Enableinput(bool value)
    {
        _inputEnabled = value;

        if (!_inputEnabled)
        {
            direction = 0;
        }
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (!_inputEnabled)
            return;
        direction = context.ReadValue<float>();
    }

    void MovementProcess()
    {
        if (_groundCounter <= 0) return;

        var velo = direction * baseSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(direction) > 0)
            _charBody.position += new Vector2(velo, 0);
        // Vector2 next = _charBody.position + new Vector2(velo, 0f);
        // _charBody.MovePosition(next);
    }

    void FixedUpdate()
    {
        // 1. เช็คพื้นจริง
        _isGrounded = Physics2D.OverlapCircle(_groundCheckPoint.position, _checkRadius, _groundLayer);

        // 2. จัดการ Delay Timer
        if (_isGrounded)
        {
            _groundCounter = _groundedDelay; // รีเซ็ตเวลาให้เต็มเมื่อแตะพื้น
        }
        else
        {
            _groundCounter -= Time.fixedDeltaTime; // ลดเวลาลงเมื่อลอยอยู่กลางอากาศ
        }

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

    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheckPoint.position, _checkRadius);
        }
    }
}

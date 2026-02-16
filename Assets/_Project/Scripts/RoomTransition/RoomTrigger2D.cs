using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomTrigger2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Room targetRoom;
    [SerializeField] private string targetSpawnId = "spawn";

    [Header("Safety")]
    [SerializeField] private bool oneShot = false;
    [SerializeField] private float cooldown = 0.2f;

    private float last;
    private bool used;
    private BoxCollider2D boxCol;

    void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && used) return;
        if (Time.time - last < cooldown) return;

        if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            // Using the preprocessor for version compatibility too!
            #if UNITY_2023_1_OR_NEWER
                rb.linearDamping = 0f;
            #else
                rb.drag = 0f;
            #endif
        }

        used = true;
        last = Time.time;

        RoomManager.I.SwitchTo(targetRoom, targetSpawnId);
    }

    // --- GIZMOS SECTION ---
    // These only run in the Scene View, so we wrap them for the compiler
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Now UnityEditor.Selection won't cause build errors
        if (UnityEditor.Selection.activeGameObject == gameObject) return;
        DrawTriggerGizmo(new Color(0f, 1f, 0f, 0.15f), Color.clear);
    }

    private void OnDrawGizmosSelected()
    {
        DrawTriggerGizmo(new Color(0f, 1f, 0f, 0.7f), new Color(0f, 1f, 0f, 0.1f));
        
        // Bonus: Draw a line to the target room if it's assigned
        if (targetRoom != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetRoom.transform.position);
        }
    }

    private void DrawTriggerGizmo(Color wireColor, Color fillColor)
    {
        if (boxCol == null) boxCol = GetComponent<BoxCollider2D>();
        if (boxCol == null) return;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = fillColor;
        Gizmos.DrawCube(boxCol.offset, boxCol.size);

        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(boxCol.offset, boxCol.size);

        Gizmos.matrix = oldMatrix;
    }
#endif
}
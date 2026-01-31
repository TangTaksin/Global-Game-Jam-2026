using UnityEngine;

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && used) return;
        if (Time.time - last < cooldown) return;

        used = true;
        last = Time.time;

        RoomManager.I.SwitchTo(targetRoom, targetSpawnId);
    }
}

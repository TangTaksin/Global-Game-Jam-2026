using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager I { get; private set; }

    [Header("Rooms")]
    [SerializeField] private Room startRoom;
    [SerializeField] private string startSpawnId = "spawn";

    [Tooltip("Optional: assign all rooms here to avoid FindObjectsByType (faster & cleaner).")]
    [SerializeField] private Room[] allRooms;

    [Header("Fade (optional)")]
    [SerializeField] private CanvasGroup fade;
    [SerializeField, Min(0f)] private float fadeTime = 0.15f;

    [Header("Cinemachine (v3.1.5)")]
    [SerializeField] private CinemachineCamera vcam;                // Cinemachine 3 camera
    [SerializeField] private CinemachineConfiner2D confiner2D;      // Confiner on that camera
    [SerializeField, Min(0)] private int confinerWarmupFrames = 2;  // 1–2 frames is usually enough

    private Room currentRoom;
    private bool switching;

    private GameObject player;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;

        // Cache player reference once (no repeated Find)
        player = GameObject.FindGameObjectWithTag("Player");

        // Make fade not block clicks if you use UI
        if (fade != null) fade.blocksRaycasts = false;
    }

    IEnumerator Start()
    {
        SetFadeInstant(0f);

        // If user didn’t assign rooms, fallback to finding them once at startup
        if (allRooms == null || allRooms.Length == 0)
            allRooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Disable all rooms, then enable start room
        for (int i = 0; i < allRooms.Length; i++)
        {
            if (allRooms[i] != null)
                allRooms[i].gameObject.SetActive(false);
        }

        if (startRoom == null) yield break;

        startRoom.gameObject.SetActive(true);
        currentRoom = startRoom;

        MovePlayerToSpawn(startRoom, startSpawnId);

        yield return UpdateConfinerForRoom(startRoom);
    }

    public void SwitchTo(Room nextRoom, string spawnId)
    {
        if (switching || nextRoom == null || nextRoom == currentRoom) return;
        StartCoroutine(CoSwitch(nextRoom, spawnId));
    }

    private IEnumerator CoSwitch(Room nextRoom, string spawnId)
    {
        switching = true;

        // Fade out
        if (fade != null && fadeTime > 0f)
            yield return Fade(0f, 1f);

        // Switch rooms
        if (currentRoom != null) currentRoom.gameObject.SetActive(false);
        nextRoom.gameObject.SetActive(true);
        currentRoom = nextRoom;

        // Move player
        MovePlayerToSpawn(nextRoom, spawnId);

        // Update confiner for new room
        yield return UpdateConfinerForRoom(nextRoom);

        // Fade in
        if (fade != null && fadeTime > 0f)
            yield return Fade(1f, 0f);

        switching = false;
    }

    private void MovePlayerToSpawn(Room room, string spawnId)
    {
        if (player == null || room == null) return;

        var sp = room.FindSpawn(spawnId);
        if (sp != null)
            player.transform.position = sp.position;
    }

    private IEnumerator UpdateConfinerForRoom(Room room)
    {
        if (room == null || confiner2D == null) yield break;

        // Warm-up frames so newly-activated colliders are ready
        for (int i = 0; i < confinerWarmupFrames; i++)
            yield return null;

        // Find the first PolygonCollider2D inside the room (include inactive children)
        var poly = room.GetComponentInChildren<PolygonCollider2D>(true);
        if (poly == null)
        {
            Debug.LogError($"[RoomManager] No PolygonCollider2D found in room '{room.name}' for Confiner2D.");
            yield break;
        }

        // Assign & invalidate cache (Cinemachine 3.x needs this)
        confiner2D.BoundingShape2D = poly;
        confiner2D.InvalidateBoundingShapeCache();

        // Wait for bake to complete
        while (!confiner2D.BoundingShapeIsBaked)
            yield return null;

        // Force camera to re-evaluate immediately
        if (vcam != null)
            vcam.PreviousStateIsValid = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        fade.alpha = to;
    }

    private void SetFadeInstant(float a)
    {
        if (fade != null) fade.alpha = a;
    }
}

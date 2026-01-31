using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private SpawnPoint[] spawns;

    void Reset()
    {
        spawns = GetComponentsInChildren<SpawnPoint>(true);
    }

    public Transform FindSpawn(string id)
    {
        if (spawns == null || spawns.Length == 0)
            spawns = GetComponentsInChildren<SpawnPoint>(true);

        foreach (var s in spawns)
            if (s != null && s.id == id) return s.transform;

        return null;
    }
}

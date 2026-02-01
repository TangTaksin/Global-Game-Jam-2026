using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] bool spawnOnStart;

    public void Spawn()
    {
        var instance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
    }
}

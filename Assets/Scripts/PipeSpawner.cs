using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] GameObject pipePrefab;
    [SerializeField] float spawnInterval = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnPipe), 1f, spawnInterval);
    }

    void SpawnPipe()
    {
        float randomY = Random.Range(-2f, 2f);

        Instantiate(
            pipePrefab,
            new Vector3(10f, randomY, 0),
            Quaternion.identity);
    }
}
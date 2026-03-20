using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AxeZombSpawnController : MonoBehaviour
{
    [SerializeField] private GameObject axeZombPrefab;

    public int axeZombPerSpawn = 1;
    public int currentAxeZombPerSpawn;

    public float delayBetweenZombSpawn = 1f; // Delay between spawning each zombie in one spawn cycle

    public float spawnInterval = 5f; // Spawn zombies every X seconds
    public float spawnCountIncreaseInterval = 60f; // Increase zombies per spawn every X seconds
    public int spawnCountIncreaseAmount = 1;

    public float spawnTimer = 0f;
    public float spawnCountIncreaseTimer = 0f;

    public List<AxeZomb> currentAxeZombAlive; 

    private bool isSpawning;

    private void Start()
    {
        currentAxeZombPerSpawn = Mathf.Max(1, axeZombPerSpawn);
        currentAxeZombAlive = new List<AxeZomb>();
    }

    private void TryStartSpawnCycle()
    {
        if (isSpawning)
        {
            return;
        }

        StartCoroutine(SpawnAxeZombsRoutine());
    }

    private IEnumerator SpawnAxeZombsRoutine()
    {
        isSpawning = true;

        if (axeZombPrefab == null)
        {
            Debug.LogError("AxeZombSpawnController: Missing axeZombPrefab reference on " + gameObject.name + ". Assign the prefab in the Inspector.");
            enabled = false;
            isSpawning = false;
            yield break;
        }

        for (int i = 0; i < currentAxeZombPerSpawn; i++)
        {
            // Instantiate a new AxeZomb at the spawner's position
            GameObject newAxeZomb = Instantiate(axeZombPrefab, transform.position, Quaternion.identity);
            AxeZomb axeZombEnemy = newAxeZomb.GetComponent<AxeZomb>();
            if (axeZombEnemy != null)
            {
                currentAxeZombAlive.Add(axeZombEnemy);
            }
            else
            {
                Debug.LogWarning("AxeZombSpawnController: Spawned prefab has no AxeZomb component.");
            }

            yield return new WaitForSeconds(delayBetweenZombSpawn);
        }

        isSpawning = false;
    }

    private void Update()
    {
        // Remove destroyed enemies so tracking reflects what's currently alive.
        currentAxeZombAlive.RemoveAll(enemy => enemy == null);

        spawnTimer += Time.deltaTime;
        spawnCountIncreaseTimer += Time.deltaTime;

        if (spawnCountIncreaseTimer >= spawnCountIncreaseInterval)
        {
            currentAxeZombPerSpawn += Mathf.Max(1, spawnCountIncreaseAmount);
            spawnCountIncreaseTimer -= spawnCountIncreaseInterval;
        }

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer -= spawnInterval;
            TryStartSpawnCycle();
        }
    }
}

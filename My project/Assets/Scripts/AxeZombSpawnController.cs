using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AxeZombSpawnController : MonoBehaviour
{
    [SerializeField] private GameObject axeZombPrefab;

    public int AxeZombPerWave = 5;
    public int currentAxeZombPerWave;

    public float delayBetweenZombSpawn = 1f; // Delay between spawning each zombie in a wave

    public int currentWave = 0;
    public float waveDelay = 5f; // Delay between waves

    public bool inCooldown;
    public float cooldownTimer = 0f;

    public List<AxeZomb> currentAxeZombAlive; 

    private bool isSpawningWave;

    private void Start()
    {
        currentAxeZombPerWave = AxeZombPerWave;
        currentAxeZombAlive = new List<AxeZomb>();
    }

    private void StartNextWave()
    {
        if (isSpawningWave)
        {
            return;
        }

        StartCoroutine(StartNextWaveRoutine());
    }

    private IEnumerator StartNextWaveRoutine()
    {
        isSpawningWave = true;

        // Wait between waves to give players breathing room.
        inCooldown = true;
        cooldownTimer = 0f;

        while (cooldownTimer < waveDelay)
        {
            cooldownTimer += Time.deltaTime;
            yield return null;
        }

        inCooldown = false;

        currentWave++;
        currentAxeZombPerWave = AxeZombPerWave + (currentWave - 1) * 2; // Increase zombies per wave
        yield return StartCoroutine(SpawnAxeZombs());

        isSpawningWave = false;
    }

    private IEnumerator SpawnAxeZombs()
    {
        if (axeZombPrefab == null)
        {
            Debug.LogError("AxeZombSpawnController: Missing axeZombPrefab reference.");
            yield break;
        }

        for (int i = 0; i < currentAxeZombPerWave; i++)
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
                Debug.LogWarning("AxeZombSpawnController: Spawned prefab has no Enemy component.");
            }

            yield return new WaitForSeconds(delayBetweenZombSpawn);
        }
    }

    private void Update()
    {
        // Remove destroyed enemies so wave progression can continue.
        currentAxeZombAlive.RemoveAll(enemy => enemy == null);

        if (!inCooldown && !isSpawningWave && currentAxeZombAlive.Count == 0)
        {
            StartNextWave();
        }
    }
}

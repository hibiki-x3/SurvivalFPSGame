using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }
    public static event Action PlayerDied;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float hitInvulnerability = 0.2f;
    [SerializeField] private bool autoReloadOnDeath = false;
    [SerializeField] private float deathReloadDelay = 1.8f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private float lastDamageTime = -999f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureOnLevelPlayer()
    {
        if (SceneManager.GetActiveScene().name != "Level")
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    private void Start()
    {
        HUDManager.Instance?.SetHealth(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        if (Time.time < lastDamageTime + hitInvulnerability)
        {
            return;
        }

        lastDamageTime = Time.time;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        HUDManager.Instance?.SetHealth(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void SetHealth(int health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, maxHealth);
        IsDead = CurrentHealth <= 0;
        HUDManager.Instance?.SetHealth(CurrentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        HUDManager.Instance?.SetHealth(CurrentHealth, maxHealth);
    }

    private void HandleDeath()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        MovementScript mouseLook = GetComponentInChildren<MovementScript>();
        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }

        WeaponManager weaponManager = GetComponentInChildren<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PlayerDied?.Invoke();

        if (autoReloadOnDeath)
        {
            StartCoroutine(ReloadLevelAfterDelay());
        }
    }

    private IEnumerator ReloadLevelAfterDelay()
    {
        yield return new WaitForSecondsRealtime(deathReloadDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

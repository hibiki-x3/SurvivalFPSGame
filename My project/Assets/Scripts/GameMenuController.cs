using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CharacterController playerCharacterController;

    [Header("Scene Names (Optional)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "Main";

    private const string SaveKey = "survival_fps_save";

    private static bool hasPendingLoad;
    private static SaveData pendingData;
    private static bool bootstrapRegistered;

    [Serializable]
    private class SaveData
    {
        public int sceneBuildIndex;

        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;

        public float playerRotX;
        public float playerRotY;
        public float playerRotZ;

        public int totalRifleAmmo;
        public int totalPistolAmmo;
        public int activeWeaponSlotIndex;

        public int slot0Bullets;
        public int slot1Bullets;
        public int slot2Bullets;

        public int score;

        public bool hasAxeSpawnState;
        public int axeCurrentPerSpawn;
        public float axeSpawnTimer;
        public float axeSpawnIncreaseTimer;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        if (bootstrapRegistered)
        {
            return;
        }

        SceneManager.sceneLoaded += EnsureControllerOnSceneLoad;
        bootstrapRegistered = true;
    }

    private static void EnsureControllerOnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (string.Equals(scene.name, "MainMenu", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindObjectOfType<GameMenuController>() != null)
        {
            return;
        }

        GameObject runtimeControllerObject = new GameObject("GameMenuControllerRuntime");
        runtimeControllerObject.AddComponent<GameMenuController>();
    }

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void Start()
    {
        if (hasPendingLoad && pendingData != null)
        {
            StartCoroutine(ApplyPendingDataWhenReady());
        }
    }

    private IEnumerator ApplyPendingDataWhenReady()
    {
        for (int i = 0; i < 30; i++)
        {
            AutoAssignReferences();
            if (playerTransform != null)
            {
                break;
            }

            yield return null;
        }

        if (hasPendingLoad && pendingData != null)
        {
            ApplySaveData(pendingData);
            hasPendingLoad = false;
            pendingData = null;
        }
    }

    public void SaveGame()
    {
        AutoAssignReferences();

        SaveData data = CaptureSaveData();
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Game saved.");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.LogWarning("No saved game found.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogError("Saved data is invalid.");
            return;
        }

        Time.timeScale = 1f;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene != data.sceneBuildIndex)
        {
            hasPendingLoad = true;
            pendingData = data;
            SceneManager.LoadScene(data.sceneBuildIndex);
            return;
        }

        ApplySaveData(data);
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private SaveData CaptureSaveData()
    {
        SaveData data = new SaveData
        {
            sceneBuildIndex = SceneManager.GetActiveScene().buildIndex,
            activeWeaponSlotIndex = 0,
            slot0Bullets = -1,
            slot1Bullets = -1,
            slot2Bullets = -1,
            score = 0
        };

        if (playerTransform != null)
        {
            Vector3 p = playerTransform.position;
            Vector3 r = playerTransform.eulerAngles;

            data.playerPosX = p.x;
            data.playerPosY = p.y;
            data.playerPosZ = p.z;

            data.playerRotX = r.x;
            data.playerRotY = r.y;
            data.playerRotZ = r.z;
        }

        if (WeaponManager.Instance != null)
        {
            data.totalRifleAmmo = WeaponManager.Instance.totalRifleAmmo;
            data.totalPistolAmmo = WeaponManager.Instance.totalPistolAmmo;
            data.activeWeaponSlotIndex = GetActiveSlotIndex();

            data.slot0Bullets = GetSlotBullets(0);
            data.slot1Bullets = GetSlotBullets(1);
            data.slot2Bullets = GetSlotBullets(2);
        }

        if (HUDManager.Instance != null)
        {
            data.score = HUDManager.Instance.Score;
        }

        AxeZombSpawnController spawnController = FindObjectOfType<AxeZombSpawnController>();
        if (spawnController != null)
        {
            AxeZombSpawnController.SpawnCycleState spawnState = spawnController.GetSpawnCycleState();
            data.hasAxeSpawnState = true;
            data.axeCurrentPerSpawn = spawnState.currentPerSpawn;
            data.axeSpawnTimer = spawnState.spawnTimer;
            data.axeSpawnIncreaseTimer = spawnState.spawnIncreaseTimer;
        }

        return data;
    }

    private void ApplySaveData(SaveData data)
    {
        AutoAssignReferences();

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.totalRifleAmmo = data.totalRifleAmmo;
            WeaponManager.Instance.totalPistolAmmo = data.totalPistolAmmo;

            SetSlotBullets(0, data.slot0Bullets);
            SetSlotBullets(1, data.slot1Bullets);
            SetSlotBullets(2, data.slot2Bullets);

            if (WeaponManager.Instance.weaponSlots != null && data.activeWeaponSlotIndex >= 0 && data.activeWeaponSlotIndex < WeaponManager.Instance.weaponSlots.Count)
            {
                WeaponManager.Instance.SwitchActiveSlot(data.activeWeaponSlotIndex);
            }
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.SetScore(data.score);
        }

        if (playerTransform != null)
        {
            bool hadController = playerCharacterController != null;
            if (hadController)
            {
                playerCharacterController.enabled = false;
            }

            playerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            playerTransform.rotation = Quaternion.Euler(data.playerRotX, data.playerRotY, data.playerRotZ);

            if (hadController)
            {
                playerCharacterController.enabled = true;
            }
        }

        if (data.hasAxeSpawnState)
        {
            AxeZombSpawnController spawnController = FindObjectOfType<AxeZombSpawnController>();
            if (spawnController != null)
            {
                spawnController.SetSpawnCycleState(new AxeZombSpawnController.SpawnCycleState
                {
                    currentPerSpawn = data.axeCurrentPerSpawn,
                    spawnTimer = data.axeSpawnTimer,
                    spawnIncreaseTimer = data.axeSpawnIncreaseTimer
                });
            }
        }

        Debug.Log("Game loaded.");
    }

    private void AutoAssignReferences()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerCharacterController == null && playerTransform != null)
        {
            playerCharacterController = playerTransform.GetComponent<CharacterController>();
        }
    }

    private int GetActiveSlotIndex()
    {
        if (WeaponManager.Instance == null || WeaponManager.Instance.weaponSlots == null)
        {
            return 0;
        }

        for (int i = 0; i < WeaponManager.Instance.weaponSlots.Count; i++)
        {
            if (WeaponManager.Instance.weaponSlots[i] == WeaponManager.Instance.activeWeaponSlot)
            {
                return i;
            }
        }

        return 0;
    }

    private int GetSlotBullets(int slotIndex)
    {
        if (WeaponManager.Instance == null || WeaponManager.Instance.weaponSlots == null)
        {
            return -1;
        }

        if (slotIndex < 0 || slotIndex >= WeaponManager.Instance.weaponSlots.Count)
        {
            return -1;
        }

        GameObject slot = WeaponManager.Instance.weaponSlots[slotIndex];
        if (slot == null || slot.transform.childCount == 0)
        {
            return -1;
        }

        Weapon weapon = slot.GetComponentInChildren<Weapon>();
        return weapon != null ? weapon.bulletsLeft : -1;
    }

    private void SetSlotBullets(int slotIndex, int bullets)
    {
        if (bullets < 0 || WeaponManager.Instance == null || WeaponManager.Instance.weaponSlots == null)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= WeaponManager.Instance.weaponSlots.Count)
        {
            return;
        }

        GameObject slot = WeaponManager.Instance.weaponSlots[slotIndex];
        if (slot == null || slot.transform.childCount == 0)
        {
            return;
        }

        Weapon weapon = slot.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            weapon.bulletsLeft = Mathf.Clamp(bullets, 0, weapon.magazineSize);
        }
    }
}

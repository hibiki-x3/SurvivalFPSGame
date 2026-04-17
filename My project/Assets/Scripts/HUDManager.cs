using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections;

public class HUDManager : MonoBehaviour
{

    public static HUDManager Instance{ get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    [Header("Score")]
    public TextMeshProUGUI scoreTMPText;
    public Text scoreLegacyText;

    [Header("Health")]
    public Image healthFillImage;
    public Text healthValueText;

    public Sprite emptySlot;
    public Sprite greySlot;

    private static Sprite runtimeSolidSprite;
    private Image damageFlashImage;
    private Text damageDirectionText;
    private Coroutine damageFlashRoutine;
    private Text killFeedText;
    private Coroutine killFeedRoutine;
    private int currentKillStreak;
    private float lastKillTime = -999f;

    [Header("Combat Feed")]
    [SerializeField] private float streakWindowSeconds = 3.5f;
    [SerializeField] private float killFeedDuration = 1.2f;

    public int Score { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        EnsureHealthUI();
        EnsureDamageFeedbackUI();
        EnsureKillFeedUI();

        UpdateScoreText();
    }

    private void OnEnable()
    {
        PlayerHealth.PlayerDamaged += HandlePlayerDamaged;
    }

    private void OnDisable()
    {
        PlayerHealth.PlayerDamaged -= HandlePlayerDamaged;
    }

    private void Start()
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateScoreText();
    }

    public void RegisterKill(int points)
    {
        AddScore(points);

        if (Time.time <= lastKillTime + streakWindowSeconds)
        {
            currentKillStreak++;
        }
        else
        {
            currentKillStreak = 1;
        }

        lastKillTime = Time.time;
        ShowKillFeed(currentKillStreak);
    }

    public void SetScore(int score)
    {
        Score = Mathf.Max(0, score);
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        string scoreLabel = "Score: " + Score.ToString();

        if (scoreTMPText != null)
        {
            scoreTMPText.text = scoreLabel;
        }

        if (scoreLegacyText != null)
        {
            scoreLegacyText.text = scoreLabel;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        int safeMax = Mathf.Max(1, maxHealth);
        int safeCurrent = Mathf.Clamp(currentHealth, 0, safeMax);
        float healthRatio = safeCurrent / (float)safeMax;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthRatio;
            healthFillImage.color = Color.Lerp(new Color(0.82f, 0.18f, 0.18f, 1f), new Color(0.09f, 0.81f, 0.25f, 1f), healthRatio);
        }

        if (healthValueText != null)
        {
            healthValueText.text = $"HP {safeCurrent}/{safeMax}";
        }
    }

    private void Update()
{
    WeaponManager weaponManager = WeaponManager.Instance;
    if (weaponManager == null)
    {
        return;
    }

    if (weaponManager.activeWeaponSlot == null && weaponManager.weaponSlots != null && weaponManager.weaponSlots.Count > 0)
    {
        weaponManager.activeWeaponSlot = weaponManager.weaponSlots[0];
    }

    if (weaponManager.activeWeaponSlot == null)
    {
        return;
    }

    Weapon activeWeapon = weaponManager.activeWeaponSlot.GetComponentInChildren<Weapon>();
    // Get the other slot
    GameObject unActiveSlot = GetUnActiveWeaponSlot();
    Weapon unActiveWeapon = unActiveSlot != null ? unActiveSlot.GetComponentInChildren<Weapon>() : null;

    if (activeWeapon != null)
    {
        // 1. Always update Active Weapon Info
        magazineAmmoUI.text = $"{activeWeapon.bulletsLeft}";
        totalAmmoUI.text = $"{weaponManager.CheckAmmoLeftFor(activeWeapon.thisWeaponModel)}";

        Weapon.WeaponModel model = activeWeapon.thisWeaponModel;
        ammoTypeUI.sprite = (Sprite)GetAmmoSprite(model);
        activeWeaponUI.sprite = (Sprite)GetWeaponSprite(model);

        // 2. Handle the Secondary Weapon Slot separately
        if (unActiveWeapon != null)
        {
            unActiveWeaponUI.sprite = (Sprite)GetWeaponSprite(unActiveWeapon.thisWeaponModel);
        }
        else
        {
            unActiveWeaponUI.sprite = emptySlot;
        }
    }
    else
    {
        // 3. If NO weapon is active at all, clear everything
        magazineAmmoUI.text = "";
        totalAmmoUI.text = "";
        ammoTypeUI.sprite = emptySlot;
        activeWeaponUI.sprite = emptySlot;
        unActiveWeaponUI.sprite = emptySlot;
    }
    
    if(weaponManager.grenades <= 0)
        {
            lethalUI.sprite = greySlot;
        }



}

    private object GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.PistolM1911:
                return Resources.Load<GameObject>("PistolM1911_Weapon").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M16:
                return Resources.Load<GameObject>("M16_Weapon").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.Uzi:
                return Resources.Load<GameObject>("Uzi_Weapon").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M249:
                return Resources.Load<GameObject>("M249_Weapon").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private object GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.PistolM1911:
                return Resources.Load<GameObject>("Pistol_Ammo").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M16:
                return Resources.Load<GameObject>("Rifle_Ammo").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.Uzi:
                return Resources.Load<GameObject>("Smg_Ammo").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M249:
                return Resources.Load<GameObject>("Machine_Gun_Ammo").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach(GameObject weaponSlot in WeaponManager.Instance.weaponSlots)
        {
            if(weaponSlot != WeaponManager.Instance.activeWeaponSlot)
            {
                return weaponSlot;
            }
        }
        return null;
    }


    internal void UpdateThrowables(Throwable.ThrowableType throwable)
    {
        switch (throwable)
        {
            case Throwable.ThrowableType.Grenade:
                lethalAmountUI.text = $"{WeaponManager.Instance.grenades}";
                lethalUI.sprite = Resources.Load<GameObject>("Grenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }
    }

    private void EnsureHealthUI()
    {
        if (healthFillImage != null && healthValueText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Transform existingRoot = canvas.transform.Find("HPBar");
        if (existingRoot == null)
        {
            existingRoot = CreateHealthBarRoot(canvas.transform);
        }

        RectTransform existingRootRect = existingRoot as RectTransform;
        if (existingRootRect != null)
        {
            ApplyHealthBarLayout(existingRootRect);
        }

        if (existingRoot == null)
        {
            return;
        }

        Transform fillTransform = existingRoot.Find("Fill");
        if (fillTransform != null)
        {
            healthFillImage = fillTransform.GetComponent<Image>();
        }

        Transform valueTextTransform = existingRoot.Find("Value");
        if (valueTextTransform != null)
        {
            healthValueText = valueTextTransform.GetComponent<Text>();
        }
    }

    private void EnsureDamageFeedbackUI()
    {
        if (damageFlashImage != null && damageDirectionText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Transform existingFlash = canvas.transform.Find("DamageFlash");
        if (existingFlash == null)
        {
            GameObject flashObject = new GameObject("DamageFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            flashObject.transform.SetParent(canvas.transform, false);

            RectTransform flashRect = flashObject.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            damageFlashImage = flashObject.GetComponent<Image>();
            damageFlashImage.sprite = GetRuntimeSolidSprite();
            damageFlashImage.color = new Color(0.9f, 0.1f, 0.1f, 0f);
            damageFlashImage.raycastTarget = false;
        }
        else
        {
            damageFlashImage = existingFlash.GetComponent<Image>();
        }

        Transform existingDirection = canvas.transform.Find("DamageDirection");
        if (existingDirection == null)
        {
            GameObject directionObject = new GameObject("DamageDirection", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            directionObject.transform.SetParent(canvas.transform, false);

            RectTransform dirRect = directionObject.GetComponent<RectTransform>();
            dirRect.anchorMin = new Vector2(0.5f, 0.5f);
            dirRect.anchorMax = new Vector2(0.5f, 0.5f);
            dirRect.pivot = new Vector2(0.5f, 0.5f);
            dirRect.anchoredPosition = new Vector2(0f, 36f);
            dirRect.sizeDelta = new Vector2(120f, 36f);

            damageDirectionText = directionObject.GetComponent<Text>();
            damageDirectionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            damageDirectionText.alignment = TextAnchor.MiddleCenter;
            damageDirectionText.fontSize = 26;
            damageDirectionText.color = new Color(1f, 0.4f, 0.4f, 0f);
            damageDirectionText.text = "";
        }
        else
        {
            damageDirectionText = existingDirection.GetComponent<Text>();
        }
    }

    private void EnsureKillFeedUI()
    {
        if (killFeedText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Transform existing = canvas.transform.Find("KillFeedText");
        if (existing == null)
        {
            GameObject feedObject = new GameObject("KillFeedText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            feedObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = feedObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.65f);
            rect.anchorMax = new Vector2(0.5f, 0.65f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(380f, 56f);

            killFeedText = feedObject.GetComponent<Text>();
            killFeedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            killFeedText.alignment = TextAnchor.MiddleCenter;
            killFeedText.fontSize = 28;
            killFeedText.color = new Color(1f, 0.96f, 0.72f, 0f);
            killFeedText.text = string.Empty;
        }
        else
        {
            killFeedText = existing.GetComponent<Text>();
        }
    }

    private void ShowKillFeed(int streak)
    {
        EnsureKillFeedUI();
        if (killFeedText == null)
        {
            return;
        }

        string label = streak >= 3 ? "KILL STREAK x" + streak : "KILL +10";
        killFeedText.text = label;
        killFeedText.fontSize = streak >= 3 ? 34 : 28;
        killFeedText.color = streak >= 3
            ? new Color(1f, 0.64f, 0.35f, 1f)
            : new Color(1f, 0.96f, 0.72f, 1f);

        if (killFeedRoutine != null)
        {
            StopCoroutine(killFeedRoutine);
        }

        killFeedRoutine = StartCoroutine(AnimateKillFeed());
    }

    private IEnumerator AnimateKillFeed()
    {
        yield return new WaitForSecondsRealtime(killFeedDuration);

        if (killFeedText == null)
        {
            yield break;
        }

        float timer = 0f;
        const float fadeTime = 0.25f;
        Color start = killFeedText.color;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeTime;

            Color c = start;
            c.a = Mathf.Lerp(start.a, 0f, t);
            killFeedText.color = c;
            yield return null;
        }

        Color done = killFeedText.color;
        done.a = 0f;
        killFeedText.color = done;
        killFeedText.text = string.Empty;
        killFeedRoutine = null;
    }

    private void HandlePlayerDamaged(float normalizedDamage, Vector3 sourcePosition, bool hasSource)
    {
        EnsureDamageFeedbackUI();
        if (damageFlashImage == null)
        {
            return;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(AnimateDamageFeedback(normalizedDamage, sourcePosition, hasSource));
    }

    private IEnumerator AnimateDamageFeedback(float normalizedDamage, Vector3 sourcePosition, bool hasSource)
    {
        float flashStrength = Mathf.Clamp01(0.2f + normalizedDamage * 0.8f);
        Color flashColor = damageFlashImage.color;
        flashColor.a = 0.38f * flashStrength;
        damageFlashImage.color = flashColor;

        if (damageDirectionText != null)
        {
            damageDirectionText.text = ResolveDamageDirection(sourcePosition, hasSource);
            Color dirColor = damageDirectionText.color;
            dirColor.a = 0.95f;
            damageDirectionText.color = dirColor;
        }

        float timer = 0f;
        const float fadeTime = 0.35f;
        Color startFlash = damageFlashImage.color;
        Color startDirection = damageDirectionText != null ? damageDirectionText.color : Color.clear;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeTime;

            Color fadeFlash = startFlash;
            fadeFlash.a = Mathf.Lerp(startFlash.a, 0f, t);
            damageFlashImage.color = fadeFlash;

            if (damageDirectionText != null)
            {
                Color fadeDir = startDirection;
                fadeDir.a = Mathf.Lerp(startDirection.a, 0f, t);
                damageDirectionText.color = fadeDir;
            }

            yield return null;
        }

        Color doneFlash = damageFlashImage.color;
        doneFlash.a = 0f;
        damageFlashImage.color = doneFlash;

        if (damageDirectionText != null)
        {
            Color doneDir = damageDirectionText.color;
            doneDir.a = 0f;
            damageDirectionText.color = doneDir;
            damageDirectionText.text = string.Empty;
        }

        damageFlashRoutine = null;
    }

    private string ResolveDamageDirection(Vector3 sourcePosition, bool hasSource)
    {
        if (!hasSource || Camera.main == null)
        {
            return "!";
        }

        Vector3 toSource = sourcePosition - Camera.main.transform.position;
        toSource.y = 0f;
        if (toSource.sqrMagnitude < 0.01f)
        {
            return "!";
        }

        toSource.Normalize();
        float signedAngle = Vector3.SignedAngle(Camera.main.transform.forward, toSource, Vector3.up);

        if (Mathf.Abs(signedAngle) <= 35f)
        {
            return "^";
        }

        return signedAngle > 0f ? ">" : "<";
    }

    private Transform CreateHealthBarRoot(Transform parent)
    {
        GameObject root = new GameObject("HPBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        root.transform.SetParent(parent, false);

        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = new Vector2(24f, -24f);
        rootRect.sizeDelta = new Vector2(280f, 28f);
        ApplyHealthBarLayout(rootRect);

        Image rootImage = root.GetComponent<Image>();
        Sprite builtInSprite = GetRuntimeSolidSprite();
        rootImage.sprite = builtInSprite;
        rootImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fill.transform.SetParent(root.transform, false);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);

        Image fillImage = fill.GetComponent<Image>();
        fillImage.sprite = builtInSprite;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;
        fillImage.color = new Color(0.09f, 0.81f, 0.25f, 1f);

        GameObject value = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform valueRect = value.GetComponent<RectTransform>();
        value.transform.SetParent(root.transform, false);
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        Text valueText = value.GetComponent<Text>();
        valueText.alignment = TextAnchor.MiddleCenter;
        valueText.fontSize = 16;
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueText.color = new Color(0.94f, 0.98f, 0.94f, 1f);
        valueText.text = "HP 100/100";

        return root.transform;
    }

    private static Sprite GetRuntimeSolidSprite()
    {
        if (runtimeSolidSprite != null)
        {
            return runtimeSolidSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        runtimeSolidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        return runtimeSolidSprite;
    }


    private void ApplyHealthBarLayout(RectTransform rootRect)
    {
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.anchoredPosition = new Vector2(20f, 20f);
        rootRect.sizeDelta = new Vector2(180f, 16f);
    }
}

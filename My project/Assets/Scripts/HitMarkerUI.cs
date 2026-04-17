using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitMarkerUI : MonoBehaviour
{
    public static HitMarkerUI Instance { get; private set; }

    [SerializeField] private float showDuration = 0.08f;
    [SerializeField] private float fadeDuration = 0.05f;
    [SerializeField] private int fontSize = 11;

    private CanvasGroup markerGroup;
    private Text markerText;
    private Coroutine animationRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject root = new GameObject("HitMarkerUI");
        DontDestroyOnLoad(root);
        root.AddComponent<HitMarkerUI>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUi();
    }

    public void ShowHitMarker()
    {
        EnsureUi();
        if (markerGroup == null)
        {
            return;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateHitMarker());
    }

    private IEnumerator AnimateHitMarker()
    {
        markerGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(showDuration);

        float timer = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            markerGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }

        markerGroup.alpha = 0f;
        animationRoutine = null;
    }

    private void EnsureUi()
    {
        if (markerGroup != null && markerText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("HitMarkerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        Transform existing = canvas.transform.Find("HitMarker");
        if (existing == null)
        {
            GameObject markerObject = new GameObject("HitMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(Text));
            markerObject.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = markerObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(16f, 16f);

            markerGroup = markerObject.GetComponent<CanvasGroup>();
            markerGroup.alpha = 0f;

            markerText = markerObject.GetComponent<Text>();
            markerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            markerText.text = "X";
            markerText.fontSize = fontSize;
            markerText.alignment = TextAnchor.MiddleCenter;
            markerText.color = new Color(1f, 1f, 1f, 0.95f);
        }
        else
        {
            markerGroup = existing.GetComponent<CanvasGroup>();
            if (markerGroup == null)
            {
                markerGroup = existing.gameObject.AddComponent<CanvasGroup>();
            }

            markerText = existing.GetComponent<Text>();
            if (markerText == null)
            {
                markerText = existing.gameObject.AddComponent<Text>();
            }

            markerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            markerText.text = "X";
            markerText.fontSize = fontSize;
            markerText.alignment = TextAnchor.MiddleCenter;
            markerText.color = new Color(1f, 1f, 1f, 0.95f);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenuRuntime : MonoBehaviour
{
    private const string LevelSceneName = "Level";

    private GameMenuController gameMenuController;

    private bool showStartMenu = true;
    private bool showPauseMenu;
    private bool showGameOverMenu;

    private Rect menuRect;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle subtitleStyle;

    private static Texture2D overlayTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != LevelSceneName)
        {
            return;
        }

        if (FindAnyObjectByType<LevelMenuRuntime>() != null)
        {
            return;
        }

        GameObject runtimeMenu = new GameObject("LevelMenuRuntime");
        runtimeMenu.AddComponent<LevelMenuRuntime>();
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != LevelSceneName)
        {
            Destroy(gameObject);
            return;
        }

        gameMenuController = GetComponent<GameMenuController>();
        if (gameMenuController == null)
        {
            gameMenuController = FindAnyObjectByType<GameMenuController>();
        }

        if (gameMenuController == null)
        {
            gameMenuController = gameObject.AddComponent<GameMenuController>();
        }

        menuRect = new Rect(0f, 0f, 460f, 420f);
    }

    private void Start()
    {
        PlayerHealth.PlayerDied += HandlePlayerDied;
        EnterMenuState(startMenu: true);
    }

    private void OnDestroy()
    {
        PlayerHealth.PlayerDied -= HandlePlayerDied;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != LevelSceneName)
        {
            showPauseMenu = false;
            showStartMenu = false;
            showGameOverMenu = false;
            return;
        }

        PlayerHealth playerHealth = PlayerHealth.Instance;
        if (!showGameOverMenu && playerHealth != null && playerHealth.CurrentHealth <= 0)
        {
            EnterGameOverState();
            return;
        }

        if (showGameOverMenu)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (showStartMenu)
        {
            return;
        }

        if (showPauseMenu)
        {
            ResumeGameplay();
        }
        else
        {
            EnterMenuState(startMenu: false);
        }
    }

    private void OnGUI()
    {
        if (!showStartMenu && !showPauseMenu && !showGameOverMenu)
        {
            return;
        }

        EnsureGuiStyles();

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), overlayTexture, ScaleMode.StretchToFill);
        GUI.color = previousColor;

        menuRect.x = (Screen.width - menuRect.width) * 0.5f;
        menuRect.y = (Screen.height - menuRect.height) * 0.5f;

        string windowTitle = showGameOverMenu ? "GAME OVER" : (showStartMenu ? "LEVEL START" : "PAUSED");
        menuRect = GUI.ModalWindow(1717, menuRect, DrawMenuWindow, windowTitle);

        GUI.Label(
            new Rect(menuRect.x + 20f, menuRect.y + 48f, menuRect.width - 40f, 36f),
            showGameOverMenu
                ? "You were defeated. Choose what to do next."
                : (showStartMenu ? "Prepare and begin when ready." : "Select an action."),
            subtitleStyle
        );
    }

    private void DrawMenuWindow(int windowId)
    {
        float y = 96f;
        const float x = 40f;
        const float width = 380f;
        const float height = 52f;
        const float spacing = 14f;

        if (showGameOverMenu)
        {
            if (GUI.Button(new Rect(x, y, width, height), "Retry Level", buttonStyle))
            {
                Time.timeScale = 1f;
                gameMenuController.RetryLevel();
            }

            y += height + spacing;

            if (GUI.Button(new Rect(x, y, width, height), "Load Save", buttonStyle))
            {
                Time.timeScale = 1f;
                gameMenuController.LoadGame();
            }

            y += height + spacing;

            if (GUI.Button(new Rect(x, y, width, height), "Main Menu", buttonStyle))
            {
                Time.timeScale = 1f;
                gameMenuController.ExitToMainMenu();
            }

            return;
        }

        if (showStartMenu)
        {
            if (GUI.Button(new Rect(x, y, width, height), "Start Level", buttonStyle))
            {
                ResumeGameplay();
            }

            y += height + spacing;

            if (GUI.Button(new Rect(x, y, width, height), "Load Save", buttonStyle))
            {
                gameMenuController.LoadGame();
                ResumeGameplay();
            }

            y += height + spacing;

            if (GUI.Button(new Rect(x, y, width, height), "Back To Main Menu", buttonStyle))
            {
                gameMenuController.ExitToMainMenu();
            }

            return;
        }

        if (GUI.Button(new Rect(x, y, width, height), "Continue", buttonStyle))
        {
            ResumeGameplay();
        }

        y += height + spacing;

        if (GUI.Button(new Rect(x, y, width, height), "Save", buttonStyle))
        {
            gameMenuController.SaveGame();
        }

        y += height + spacing;

        if (GUI.Button(new Rect(x, y, width, height), "Load", buttonStyle))
        {
            gameMenuController.LoadGame();
            ResumeGameplay();
        }

        y += height + spacing;

        if (GUI.Button(new Rect(x, y, width, height), "Retry Level", buttonStyle))
        {
            gameMenuController.RetryLevel();
        }

        y += height + spacing;

        if (GUI.Button(new Rect(x, y, width, height), "Main Menu", buttonStyle))
        {
            gameMenuController.ExitToMainMenu();
        }
    }

    private void EnterMenuState(bool startMenu)
    {
        showGameOverMenu = false;
        showStartMenu = startMenu;
        showPauseMenu = !startMenu;

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ResumeGameplay()
    {
        showGameOverMenu = false;
        showStartMenu = false;
        showPauseMenu = false;

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void HandlePlayerDied()
    {
        if (SceneManager.GetActiveScene().name != LevelSceneName)
        {
            return;
        }

        EnterGameOverState();
    }

    private void EnterGameOverState()
    {
        showGameOverMenu = true;
        showStartMenu = false;
        showPauseMenu = false;

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void EnsureGuiStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.94f, 1f, 0.95f) }
            };
        }

        if (subtitleStyle == null)
        {
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.84f, 0.9f, 0.97f, 0.95f) }
            };
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 52f
            };
        }

        if (overlayTexture == null)
        {
            overlayTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            overlayTexture.SetPixel(0, 0, Color.white);
            overlayTexture.Apply();
        }
    }
}

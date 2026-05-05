using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_BossClearMessage : MonoBehaviour
{
    private const string SkipStartMenuKey = "Metroidvania.SkipStartMenu";
    private static bool skipStartMenuOnNextLoad;

    [Header("Boss")]
    [SerializeField] private CharacterStats bossStats;
    [SerializeField] private float bossClearMenuDelay = 1.15f;

    [Header("Player")]
    [SerializeField] private CharacterStats playerStats;
    [SerializeField] private float playerDeathMenuDelay = 1f;

    [Header("Start menu")]
    [SerializeField] private GameObject startMenuRoot;
    [SerializeField] private Sprite startBackgroundSprite;
    [SerializeField] private string startTitle = "METROIDVANIA DEMO";
    [SerializeField] private Button startButton;
    [SerializeField] private Button startQuitButton;

    [Header("Pause / clear menu")]
    [SerializeField] private GameObject messageRoot;
    [SerializeField] private Text messageText;
    [SerializeField] private string pauseMessage = "\u6e38\u620f\u6682\u505c";
    [SerializeField] private string clearMessage = "BOSS \u5df2\u51fb\u8d25";
    [SerializeField] private string deathMessage = "\u6311\u6218\u5931\u8d25";
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private bool bossSubscribed;
    private bool playerSubscribed;
    private bool gameStarted;
    private bool pauseMenuOpen;
    private bool bossDefeated;
    private bool playerDefeated;
    private float bossClearMenuTime = -1;
    private float playerDeathMenuTime = -1;
    private Coroutine bossClearCoroutine;
    private Coroutine playerDeathCoroutine;
    private Font cachedFont;

    private void Awake()
    {
        EnsureEventSystem();
        EnsureMenus();
    }

    private void Start()
    {
        Subscribe();

        bool shouldSkipStartMenu = skipStartMenuOnNextLoad || PlayerPrefs.GetInt(SkipStartMenuKey, 0) == 1;
        skipStartMenuOnNextLoad = false;
        PlayerPrefs.DeleteKey(SkipStartMenuKey);

        if (shouldSkipStartMenu)
        {
            StartGame();
        }
        else
        {
            ShowStartMenu();
        }
    }

    private void OnEnable()
    {
        EnsureMenus();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        if (playerDefeated)
        {
            if (playerDeathMenuTime > 0 && Time.unscaledTime >= playerDeathMenuTime && messageRoot != null && !messageRoot.activeInHierarchy)
                ShowPlayerDeathMenu();

            if (Input.GetKeyDown(KeyCode.R))
                RestartGame();

            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();

            return;
        }

        if (bossDefeated)
        {
            if (bossClearMenuTime > 0 && Time.unscaledTime >= bossClearMenuTime && messageRoot != null && !messageRoot.activeInHierarchy)
                ShowBossClearMenu();

            if (Input.GetKeyDown(KeyCode.R))
                RestartGame();

            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuOpen)
                ResumeGame();
            else
                ShowPauseMenu();
        }

        if (pauseMenuOpen && Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

    public void Setup(CharacterStats stats, GameObject root, Text text)
    {
        Unsubscribe();

        bossStats = stats;
        messageRoot = root;
        messageText = text;

        EnsureMenus();
        Subscribe();
        RefreshBossState();
        RefreshPlayerState();
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void EnsureMenus()
    {
        Transform canvasTransform = GetCanvasTransform();

        EnsureStartMenu(canvasTransform);
        EnsurePauseMenu(canvasTransform);

        if (!Application.isPlaying)
            return;

        if (startMenuRoot != null && !gameStarted)
            startMenuRoot.SetActive(true);

        if (messageRoot != null && !pauseMenuOpen && !bossDefeated && !playerDefeated)
            messageRoot.SetActive(false);
    }

    private Transform GetCanvasTransform()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.transform : transform;
    }

    private void EnsureStartMenu(Transform canvasTransform)
    {
        if (startMenuRoot == null)
        {
            Transform existing = canvasTransform.Find("StartMenu");
            if (existing != null)
                startMenuRoot = existing.gameObject;
        }

        if (startMenuRoot == null)
        {
            startMenuRoot = new GameObject("StartMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            startMenuRoot.transform.SetParent(canvasTransform, false);
        }

        RectTransform rootRect = startMenuRoot.GetComponent<RectTransform>();
        StretchToFullScreen(rootRect);
        startMenuRoot.transform.SetAsLastSibling();

        Image rootImage = startMenuRoot.GetComponent<Image>();
        rootImage.sprite = null;
        rootImage.color = Color.black;
        rootImage.raycastTarget = true;

        Image backgroundImage = EnsureStartBackground(startMenuRoot.transform);
        backgroundImage.transform.SetAsFirstSibling();

        EnsureShade(startMenuRoot.transform);
        Text title = EnsureText(startMenuRoot.transform, "Title", startTitle, 54, new Vector2(0, 78), new Vector2(760, 90));
        title.color = new Color(1f, .93f, .75f, 1);
        title.fontStyle = FontStyle.Bold;

        startButton = EnsureButton(startButton, startMenuRoot.transform, "StartButton", new Vector2(0, -30), "\u5f00\u59cb\u6e38\u620f", StartGame);
        startQuitButton = EnsureButton(startQuitButton, startMenuRoot.transform, "QuitButton", new Vector2(0, -88), "\u9000\u51fa\u6e38\u620f", QuitGame);
    }

    private Image EnsureStartBackground(Transform parent)
    {
        Transform existing = parent.Find("BackgroundImage");
        GameObject backgroundObject = existing != null ? existing.gameObject : new GameObject("BackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(parent, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        StretchToFullScreen(backgroundRect);

        Image image = backgroundObject.GetComponent<Image>();
        image.sprite = startBackgroundSprite;
        image.color = startBackgroundSprite != null ? Color.white : new Color(.02f, .025f, .04f, 1);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;

        return image;
    }

    private void EnsurePauseMenu(Transform canvasTransform)
    {
        if (messageRoot == null)
        {
            Transform existing = canvasTransform.Find("BossClearMessage");
            if (existing != null)
                messageRoot = existing.gameObject;
        }

        if (messageRoot == null)
        {
            messageRoot = new GameObject("BossClearMessage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            messageRoot.transform.SetParent(canvasTransform, false);
        }

        RectTransform rootRect = messageRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(.5f, .5f);
        rootRect.anchorMax = new Vector2(.5f, .5f);
        rootRect.pivot = new Vector2(.5f, .5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(520, 238);
        messageRoot.transform.SetAsLastSibling();

        Image panelImage = messageRoot.GetComponent<Image>();
        panelImage.color = new Color(0, 0, 0, .72f);

        if (messageText == null)
        {
            Transform existing = messageRoot.transform.Find("BossClearMessageText");
            if (existing != null)
                messageText = existing.GetComponent<Text>();
        }

        if (messageText == null)
            messageText = EnsureText(messageRoot.transform, "BossClearMessageText", pauseMessage, 38, new Vector2(0, 42), new Vector2(480, 70));

        RectTransform textRect = messageText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(.5f, .5f);
        textRect.anchorMax = new Vector2(.5f, .5f);
        textRect.pivot = new Vector2(.5f, .5f);
        textRect.anchoredPosition = new Vector2(0, 74);
        textRect.sizeDelta = new Vector2(480, 70);
        messageText.font = GetDefaultFont();
        messageText.fontSize = 38;
        messageText.fontStyle = FontStyle.Bold;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = new Color(1f, .93f, .75f, 1);
        messageText.text = pauseMessage;

        resumeButton = EnsureButton(resumeButton, messageRoot.transform, "ResumeButton", new Vector2(0, 12), "\u7ee7\u7eed\u6e38\u620f", ResumeGame);
        restartButton = EnsureButton(restartButton, messageRoot.transform, "RestartButton", new Vector2(0, -42), "\u91cd\u65b0\u5f00\u59cb", RestartGame);
        quitButton = EnsureButton(quitButton, messageRoot.transform, "QuitButton", new Vector2(0, -96), "\u9000\u51fa\u6e38\u620f", QuitGame);
    }

    private void EnsureShade(Transform parent)
    {
        Transform existing = parent.Find("Shade");
        GameObject shadeObject = existing != null ? existing.gameObject : new GameObject("Shade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadeObject.transform.SetParent(parent, false);
        shadeObject.transform.SetSiblingIndex(Mathf.Min(1, parent.childCount - 1));

        RectTransform shadeRect = shadeObject.GetComponent<RectTransform>();
        StretchToFullScreen(shadeRect);

        Image image = shadeObject.GetComponent<Image>();
        image.color = new Color(0, 0, 0, .48f);
        image.raycastTarget = false;
    }

    private Text EnsureText(Transform parent, string objectName, string content, int fontSize, Vector2 anchoredPosition, Vector2 size)
    {
        Transform existing = parent.Find(objectName);
        Text text = existing != null ? existing.GetComponent<Text>() : null;

        if (text == null)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<Text>();
        }

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(.5f, .5f);
        rectTransform.anchorMax = new Vector2(.5f, .5f);
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        text.text = content;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    private Button EnsureButton(Button button, Transform parent, string buttonName, Vector2 anchoredPosition, string label, UnityAction action)
    {
        if (button == null)
        {
            Transform existing = parent.Find(buttonName);
            if (existing != null)
                button = existing.GetComponent<Button>();
        }

        if (button == null)
        {
            GameObject buttonObject = new GameObject(buttonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            button = buttonObject.GetComponent<Button>();
        }

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(.5f, .5f);
        rectTransform.anchorMax = new Vector2(.5f, .5f);
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(180, 42);

        Image image = button.GetComponent<Image>();
        image.color = new Color(.12f, .12f, .15f, .94f);

        Text text = button.GetComponentInChildren<Text>();
        if (text == null)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(button.transform, false);
            text = textObject.GetComponent<Text>();
        }

        RectTransform textRect = text.GetComponent<RectTransform>();
        StretchToFullScreen(textRect);

        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = GetDefaultFont();
        text.fontSize = 22;
        text.color = Color.white;
        text.raycastTarget = false;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        return button;
    }

    private void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private Font GetDefaultFont()
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 32);

        if (cachedFont != null)
            return cachedFont;

        try
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    private void Subscribe()
    {
        FindPlayerStatsIfNeeded();

        if (!bossSubscribed && bossStats != null)
        {
            bossStats.onHealthChanged += RefreshBossState;
            bossSubscribed = true;
        }

        if (!playerSubscribed && playerStats != null)
        {
            playerStats.onHealthChanged += RefreshPlayerState;
            playerSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (bossSubscribed && bossStats != null)
            bossStats.onHealthChanged -= RefreshBossState;

        if (playerSubscribed && playerStats != null)
            playerStats.onHealthChanged -= RefreshPlayerState;

        bossSubscribed = false;
        playerSubscribed = false;
    }

    private void FindPlayerStatsIfNeeded()
    {
        if (playerStats != null)
            return;

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            playerStats = PlayerManager.instance.player.GetComponent<CharacterStats>();

        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
    }

    private void RefreshBossState()
    {
        if (bossStats == null || bossStats.currentHealth > 0 || bossDefeated)
            return;

        bossDefeated = true;
        bossClearMenuTime = Time.unscaledTime + bossClearMenuDelay;

        if (bossClearCoroutine != null)
            StopCoroutine(bossClearCoroutine);

        bossClearCoroutine = StartCoroutine(ShowBossClearMenuAfterDelay());
    }

    private IEnumerator ShowBossClearMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(bossClearMenuDelay);
        ShowBossClearMenu();
    }

    private void RefreshPlayerState()
    {
        if (playerStats == null || playerStats.currentHealth > 0 || playerDefeated)
            return;

        playerDefeated = true;
        playerDeathMenuTime = Time.unscaledTime + playerDeathMenuDelay;

        if (playerDeathCoroutine != null)
            StopCoroutine(playerDeathCoroutine);

        playerDeathCoroutine = StartCoroutine(ShowPlayerDeathMenuAfterDelay());
    }

    private IEnumerator ShowPlayerDeathMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(playerDeathMenuDelay);
        ShowPlayerDeathMenu();
    }

    private void ShowStartMenu()
    {
        gameStarted = false;
        pauseMenuOpen = false;
        bossDefeated = false;
        playerDefeated = false;
        bossClearMenuTime = -1;
        playerDeathMenuTime = -1;
        Time.timeScale = 0;

        if (startMenuRoot != null)
            startMenuRoot.SetActive(true);

        if (messageRoot != null)
            messageRoot.SetActive(false);
    }

    public void StartGame()
    {
        gameStarted = true;
        pauseMenuOpen = false;
        Time.timeScale = 1;

        if (startMenuRoot != null)
            startMenuRoot.SetActive(false);

        if (messageRoot != null)
            messageRoot.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        if (!gameStarted || bossDefeated || playerDefeated)
            return;

        pauseMenuOpen = true;
        Time.timeScale = 0;

        if (messageText != null)
            messageText.text = pauseMessage;

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(true);

        if (messageRoot != null)
            messageRoot.SetActive(true);
    }

    private void ResumeGame()
    {
        if (bossDefeated || playerDefeated)
            return;

        pauseMenuOpen = false;
        Time.timeScale = 1;

        if (messageRoot != null)
            messageRoot.SetActive(false);
    }

    private void ShowBossClearMenu()
    {
        gameStarted = true;
        pauseMenuOpen = false;
        bossClearMenuTime = -1;
        Time.timeScale = 0;

        if (startMenuRoot != null)
            startMenuRoot.SetActive(false);

        if (messageText != null)
            messageText.text = clearMessage;

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

        if (messageRoot != null)
            messageRoot.SetActive(true);
    }

    private void ShowPlayerDeathMenu()
    {
        gameStarted = true;
        pauseMenuOpen = false;
        playerDeathMenuTime = -1;
        Time.timeScale = 0;

        if (startMenuRoot != null)
            startMenuRoot.SetActive(false);

        if (messageText != null)
            messageText.text = deathMessage;

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

        if (messageRoot != null)
            messageRoot.SetActive(true);
    }

    public void RestartGame()
    {
        skipStartMenuOnNextLoad = true;
        PlayerPrefs.SetInt(SkipStartMenuKey, 1);
        PlayerPrefs.Save();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

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
    [SerializeField] private float menuInputBlockDuration = .2f;

    [Header("Start menu")]
    [SerializeField] private GameObject startMenuRoot;
    [SerializeField] private Sprite startBackgroundSprite;
    [SerializeField] private string startTitle = "METROIDVANIA DEMO";
    [SerializeField] private Button startButton;
    [SerializeField] private Button startQuitButton;

    [Header("Opening hint")]
    [SerializeField] private TextMesh openingHintText;
    [SerializeField] private string openingHintMessage = "A / D \u79fb\u52a8   Space \u8df3\u8dc3   Shift \u51b2\u523a\n\u5de6\u952e\u653b\u51fb   Q \u5f39\u53cd   \u53f3\u952e\u98de\u5251   R \u9ed1\u6d1e";
    [SerializeField] private Vector3 openingHintWorldOffset = new Vector3(0, 5.2f, 0);
    [SerializeField] private float openingHintCharacterSize = .1f;
    [SerializeField] private float openingHintDuration = 20;
    [SerializeField] private float openingHintFadeDuration = 1.2f;

    [Header("Pause / clear menu")]
    [SerializeField] private GameObject messageRoot;
    [SerializeField] private GameObject bossClearBackgroundRoot;
    [SerializeField] private Sprite bossClearBackgroundSprite;
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
    private Coroutine openingHintCoroutine;
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
        EnsureOpeningHint();
        EnsureBossClearBackground(canvasTransform);
        EnsurePauseMenu(canvasTransform);

        if (!Application.isPlaying)
            return;

        if (startMenuRoot != null && !gameStarted)
            startMenuRoot.SetActive(true);

        if (openingHintText != null && !gameStarted)
            openingHintText.gameObject.SetActive(false);

        if (messageRoot != null && !pauseMenuOpen && !bossDefeated && !playerDefeated)
            messageRoot.SetActive(false);

        if (bossClearBackgroundRoot != null && !bossDefeated)
            bossClearBackgroundRoot.SetActive(false);
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

    private void EnsureBossClearBackground(Transform canvasTransform)
    {
        if (bossClearBackgroundRoot == null)
        {
            Transform existing = canvasTransform.Find("BossClearBackground");
            if (existing != null)
                bossClearBackgroundRoot = existing.gameObject;
        }

        if (bossClearBackgroundRoot == null)
        {
            bossClearBackgroundRoot = new GameObject("BossClearBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bossClearBackgroundRoot.transform.SetParent(canvasTransform, false);
        }

        RectTransform rootRect = bossClearBackgroundRoot.GetComponent<RectTransform>();
        StretchToFullScreen(rootRect);

        Image image = bossClearBackgroundRoot.GetComponent<Image>();
        Sprite backgroundSprite = bossClearBackgroundSprite != null ? bossClearBackgroundSprite : startBackgroundSprite;
        image.sprite = backgroundSprite;
        image.color = backgroundSprite != null ? Color.white : new Color(.015f, .018f, .03f, 1);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;

        EnsureBossClearBackgroundShade(bossClearBackgroundRoot.transform);
    }

    private void EnsureBossClearBackgroundShade(Transform parent)
    {
        Transform existing = parent.Find("VictoryShade");
        GameObject shadeObject = existing != null ? existing.gameObject : new GameObject("VictoryShade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadeObject.transform.SetParent(parent, false);

        RectTransform shadeRect = shadeObject.GetComponent<RectTransform>();
        StretchToFullScreen(shadeRect);

        Image image = shadeObject.GetComponent<Image>();
        image.color = new Color(0, 0, 0, .38f);
        image.raycastTarget = false;
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

    private void EnsureOpeningHint()
    {
        if (openingHintText == null)
        {
            GameObject existing = GameObject.Find("OpeningHint");
            if (existing != null)
                openingHintText = existing.GetComponent<TextMesh>();
        }

        if (openingHintText == null)
        {
            GameObject hintObject = new GameObject("OpeningHint");
            openingHintText = hintObject.AddComponent<TextMesh>();
        }

        openingHintText.text = openingHintMessage;
        openingHintText.font = GetDefaultFont();
        openingHintText.fontSize = 48;
        openingHintText.fontStyle = FontStyle.Bold;
        openingHintText.anchor = TextAnchor.MiddleCenter;
        openingHintText.alignment = TextAlignment.Center;
        openingHintText.characterSize = openingHintCharacterSize;
        openingHintText.color = new Color(1f, .95f, .78f, .92f);

        MeshRenderer hintRenderer = openingHintText.GetComponent<MeshRenderer>();

        if (hintRenderer != null)
        {
            if (openingHintText.font != null)
                hintRenderer.sharedMaterial = openingHintText.font.material;

            hintRenderer.sortingLayerName = "Background";
            hintRenderer.sortingOrder = 20;
        }
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
        button.onClick.AddListener(() =>
        {
            AudioManager.PlaySFX(SfxType.MenuClick, .65f);

            if (action != null)
                action.Invoke();
        });
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

        if (bossClearBackgroundRoot != null)
            bossClearBackgroundRoot.SetActive(false);

        if (messageRoot != null)
            messageRoot.SetActive(false);

        HideOpeningHint();
    }

    public void StartGame()
    {
        gameStarted = true;
        pauseMenuOpen = false;
        Time.timeScale = 1;
        BlockPlayerActionInput();

        if (startMenuRoot != null)
            startMenuRoot.SetActive(false);

        if (bossClearBackgroundRoot != null)
            bossClearBackgroundRoot.SetActive(false);

        if (messageRoot != null)
            messageRoot.SetActive(false);

        ShowOpeningHint();
    }

    private void ShowPauseMenu()
    {
        if (!gameStarted || bossDefeated || playerDefeated)
            return;

        pauseMenuOpen = true;
        Time.timeScale = 0;

        if (messageText != null)
            messageText.text = pauseMessage;

        if (bossClearBackgroundRoot != null)
            bossClearBackgroundRoot.SetActive(false);

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(true);

        if (messageRoot != null)
        {
            messageRoot.SetActive(true);
            BringMessageLayerToFront();
        }
    }

    private void ResumeGame()
    {
        if (bossDefeated || playerDefeated)
            return;

        pauseMenuOpen = false;
        Time.timeScale = 1;
        BlockPlayerActionInput();

        if (messageRoot != null)
            messageRoot.SetActive(false);

        if (bossClearBackgroundRoot != null)
            bossClearBackgroundRoot.SetActive(false);
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

        if (bossClearBackgroundRoot != null)
        {
            bossClearBackgroundRoot.SetActive(true);
            bossClearBackgroundRoot.transform.SetAsLastSibling();
        }

        if (messageRoot != null)
        {
            messageRoot.SetActive(true);
            BringMessageLayerToFront();
        }

        HideOpeningHint();
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

        if (bossClearBackgroundRoot != null)
            bossClearBackgroundRoot.SetActive(false);

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

        if (messageRoot != null)
        {
            messageRoot.SetActive(true);
            BringMessageLayerToFront();
        }

        HideOpeningHint();
    }

    private void ShowOpeningHint()
    {
        if (openingHintText == null)
            return;

        if (openingHintCoroutine != null)
            StopCoroutine(openingHintCoroutine);

        openingHintText.text = openingHintMessage;
        PositionOpeningHint();
        openingHintText.color = new Color(1f, .95f, .78f, .92f);
        openingHintText.gameObject.SetActive(true);
        openingHintCoroutine = StartCoroutine(HideOpeningHintAfterDelay());
    }

    private void PositionOpeningHint()
    {
        if (openingHintText == null)
            return;

        Player player = null;

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;

        if (player == null)
            player = FindObjectOfType<Player>();

        if (player != null)
            openingHintText.transform.position = player.transform.position + openingHintWorldOffset;
    }

    private void HideOpeningHint()
    {
        if (openingHintCoroutine != null)
        {
            StopCoroutine(openingHintCoroutine);
            openingHintCoroutine = null;
        }

        if (openingHintText != null)
            openingHintText.gameObject.SetActive(false);
    }

    private IEnumerator HideOpeningHintAfterDelay()
    {
        yield return new WaitForSeconds(openingHintDuration);

        if (openingHintText == null)
            yield break;

        Color hintColor = openingHintText.color;
        float fadeDuration = Mathf.Max(.01f, openingHintFadeDuration);
        float fadeTime = 0;

        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            hintColor.a = Mathf.Lerp(.92f, 0, fadeTime / fadeDuration);
            openingHintText.color = hintColor;
            yield return null;
        }

        openingHintText.gameObject.SetActive(false);
        openingHintCoroutine = null;
    }

    private void BringMessageLayerToFront()
    {
        if (messageRoot == null)
            return;

        Transform messageLayer = messageRoot.transform.parent != null ? messageRoot.transform.parent : messageRoot.transform;
        messageLayer.SetAsLastSibling();
        messageRoot.transform.SetAsLastSibling();
    }

    private void BlockPlayerActionInput()
    {
        Player player = null;

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;

        if (player == null)
            player = FindObjectOfType<Player>();

        if (player != null)
            player.BlockActionInputFor(menuInputBlockDuration);
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

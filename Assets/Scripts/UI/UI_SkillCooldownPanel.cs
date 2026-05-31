using UnityEngine;
using UnityEngine.UI;

public class UI_SkillCooldownPanel : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite blackholeIcon;

    [Header("Layout")]
    [SerializeField] private Vector2 anchoredPosition = new Vector2(-34, -34);
    [SerializeField] private Vector2 slotSize = new Vector2(78, 78);
    [SerializeField] private float slotSpacing = 16;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(.04f, .04f, .05f, .78f);
    [SerializeField] private Color cooldownColor = new Color(0, 0, 0, .76f);

    private RectTransform panel;
    private Image swordCooldown;
    private Image blackholeCooldown;
    private static Sprite cooldownSprite;

    private void Awake()
    {
        EnsurePanel();
    }

    private void OnEnable()
    {
        EnsurePanel();
    }

    private void Update()
    {
        SkillManager skills = SkillManager.instance;

        if (skills == null)
            return;

        RefreshCooldown(swordCooldown, skills.sword);
        RefreshCooldown(blackholeCooldown, skills.blackhole);
    }

    private void EnsurePanel()
    {
        if (panel == null)
        {
            Transform existing = transform.Find("SkillCooldownPanel");

            if (existing != null)
                panel = existing.GetComponent<RectTransform>();
        }

        if (panel == null)
        {
            GameObject panelObject = new GameObject("SkillCooldownPanel", typeof(RectTransform));
            panelObject.transform.SetParent(transform, false);
            panel = panelObject.GetComponent<RectTransform>();
        }

        panel.anchorMin = Vector2.one;
        panel.anchorMax = Vector2.one;
        panel.pivot = Vector2.one;
        panel.anchoredPosition = anchoredPosition;
        panel.sizeDelta = new Vector2(slotSize.x * 2 + slotSpacing, slotSize.y);
        panel.SetAsFirstSibling();

        swordCooldown = EnsureSlot(panel, "SwordCooldownSlot", swordIcon, Vector2.zero);
        blackholeCooldown = EnsureSlot(panel, "BlackholeCooldownSlot", blackholeIcon, new Vector2(slotSize.x + slotSpacing, 0));
    }

    private Image EnsureSlot(Transform parent, string slotName, Sprite icon, Vector2 position)
    {
        Transform existing = parent.Find(slotName);
        GameObject slotObject = existing != null
            ? existing.gameObject
            : new GameObject(slotName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        slotObject.transform.SetParent(parent, false);

        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        slotRect.anchorMin = Vector2.zero;
        slotRect.anchorMax = Vector2.zero;
        slotRect.pivot = Vector2.zero;
        slotRect.anchoredPosition = position;
        slotRect.sizeDelta = slotSize;

        Image background = slotObject.GetComponent<Image>();
        background.sprite = GetCooldownSprite();
        background.color = panelColor;
        background.raycastTarget = false;

        Image iconImage = EnsureImage(slotObject.transform, "Icon");
        Stretch(iconImage.rectTransform, 6);
        iconImage.sprite = icon;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        Image cooldownImage = EnsureImage(slotObject.transform, "Cooldown");
        Stretch(cooldownImage.rectTransform, 0);
        cooldownImage.sprite = GetCooldownSprite();
        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillOrigin = (int)Image.Origin360.Top;
        cooldownImage.fillClockwise = false;
        cooldownImage.fillAmount = 0;
        cooldownImage.color = cooldownColor;
        cooldownImage.raycastTarget = false;
        cooldownImage.transform.SetAsLastSibling();
        return cooldownImage;
    }

    private Image EnsureImage(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);

        if (existing != null)
            return existing.GetComponent<Image>();

        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        return imageObject.GetComponent<Image>();
    }

    private void RefreshCooldown(Image image, Skill skill)
    {
        if (image != null)
            image.fillAmount = skill != null ? skill.GetCooldownFillAmount() : 0;
    }

    private void Stretch(RectTransform rectTransform, float padding)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.offsetMin = new Vector2(padding, padding);
        rectTransform.offsetMax = new Vector2(-padding, -padding);
    }

    private Sprite GetCooldownSprite()
    {
        if (cooldownSprite != null)
            return cooldownSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cooldownSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        cooldownSprite.hideFlags = HideFlags.HideAndDontSave;
        return cooldownSprite;
    }
}

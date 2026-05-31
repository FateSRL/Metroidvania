using UnityEngine;

public class Blackhole_Skill : Skill
{
    [Header("Blackhole info")]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float maxSize = 12;
    [SerializeField] private float growSpeed = 3;
    [SerializeField] private float shrinkSpeed = 6;
    [SerializeField] private float blackholeDuration = 3;
    [SerializeField] private int damageTimes = 10;

    [Header("Visuals")]
    [SerializeField] private Sprite blackholeFxSprite;
    [SerializeField] private RuntimeAnimatorController blackholeFxAnimator;
    [SerializeField] private Vector2 blackholeFxOffset = new Vector2(.01f, -.02f);
    [SerializeField] private float blackholeFxScale = .2f;

    private Blackhole_Skill_Controller currentBlackhole;
    private static Sprite runtimeBlackholeSprite;
    private bool cooldownWaitsForLanding;

    protected override void Start()
    {
        base.Start();
        cooldownTimer = -1;
    }

    protected override void Update()
    {
        base.Update();

        if (!cooldownWaitsForLanding || player == null || !player.IsGroundDetected())
            return;

        cooldownWaitsForLanding = false;
        StartCooldown();
    }

    public override bool IsReady()
    {
        return base.IsReady() && currentBlackhole == null && !cooldownWaitsForLanding;
    }

    public override bool CanUseSkill()
    {
        if (!IsReady())
        {
            Debug.Log("Skill is on cooldown");
            return false;
        }

        UseSkill();
        return true;
    }

    public override void UseSkill()
    {
        base.UseSkill();

        GameObject newBlackHole = blackHolePrefab != null
            ? Instantiate(blackHolePrefab, player.transform.position, Quaternion.identity)
            : CreateRuntimeBlackhole();

        currentBlackhole = newBlackHole.GetComponent<Blackhole_Skill_Controller>();

        if (currentBlackhole == null)
        {
            Debug.LogError("Blackhole_Skill: blackHolePrefab is missing Blackhole_Skill_Controller.", newBlackHole);
            Destroy(newBlackHole);
            return;
        }

        currentBlackhole.SetupBlackhole(maxSize, growSpeed, shrinkSpeed, blackholeDuration, damageTimes);
    }

    public bool SkillCompleted()
    {
        if (currentBlackhole == null)
            return false;

        if (currentBlackhole.playerCanExitState)
        {
            currentBlackhole = null;
            cooldownWaitsForLanding = true;
            return true;
        }

        return false;
    }

    public float GetBlackholeRadius()
    {
        return maxSize / 2;
    }

    private GameObject CreateRuntimeBlackhole()
    {
        GameObject newBlackHole = new GameObject("Blackhole");
        newBlackHole.transform.position = player.transform.position;
        newBlackHole.transform.localScale = Vector3.zero;

        SpriteRenderer spriteRenderer = newBlackHole.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetRuntimeBlackholeSprite();
        spriteRenderer.color = Color.black;
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 20;

        CircleCollider2D collider = newBlackHole.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = .5f;

        CreateRuntimeBlackholeFx(newBlackHole.transform);
        newBlackHole.AddComponent<Blackhole_Skill_Controller>();
        return newBlackHole;
    }

    private void CreateRuntimeBlackholeFx(Transform parent)
    {
        GameObject fx = new GameObject("BlackholeFX");
        fx.transform.SetParent(parent);
        fx.transform.localPosition = blackholeFxOffset;
        fx.transform.localRotation = Quaternion.identity;
        fx.transform.localScale = Vector3.one * blackholeFxScale;

        SpriteRenderer spriteRenderer = fx.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = blackholeFxSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 19;

        if (blackholeFxAnimator != null)
        {
            Animator animator = fx.AddComponent<Animator>();
            animator.runtimeAnimatorController = blackholeFxAnimator;
        }
    }

    private Sprite GetRuntimeBlackholeSprite()
    {
        if (runtimeBlackholeSprite != null)
            return runtimeBlackholeSprite;

        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        texture.hideFlags = HideFlags.HideAndDontSave;

        Vector2 center = new Vector2((textureSize - 1) * .5f, (textureSize - 1) * .5f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (textureSize * .5f);
                float alpha = distance <= .9f ? 1 : Mathf.InverseLerp(1, .9f, distance);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();
        runtimeBlackholeSprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(.5f, .5f), textureSize);
        runtimeBlackholeSprite.hideFlags = HideFlags.HideAndDontSave;
        return runtimeBlackholeSprite;
    }
}

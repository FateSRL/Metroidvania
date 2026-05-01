using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Skill : Skill
{
    private const int DefaultNumberOfDots = 20;
    private const float DefaultSpaceBetweenDots = .05f;
    private const float RuntimeDotScale = .16f;

    [Header("Skill info")]
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private Vector2 launchDir;
    [SerializeField] private float swordGravity;
    [SerializeField] private float returnSpeed = 24;

    [Header("Aim dots")]
    [SerializeField] private int numberOfDots;
    [SerializeField] private float spaceBetweenDots;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotsParent;
    [SerializeField] private Color dotColor = new Color(1, 1, 1, .8f);

    private GameObject[] dots;
    private Vector2 finalDir;
    private static Sprite runtimeDotSprite;

    protected override void Start()
    {
        base.Start();
        GenerateDots();
        DotsActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (player == null)
            return;

        if (Input.GetKeyUp(KeyCode.Mouse1))
            finalDir = GetLaunchDirection();

        if (Input.GetKey(KeyCode.Mouse1))
            UpdateDotsPosition();
    }

    public void CreateSword()
    {
        if (player == null)
            player = PlayerManager.instance.player;

        if (player.sword != null)
            return;

        if (skillPrefab == null)
        {
            Debug.LogError("Sword_Skill: skillPrefab is not assigned in the Inspector.", this);
            return;
        }

        if (finalDir == Vector2.zero)
            finalDir = GetLaunchDirection();

        GameObject newSword = Instantiate(skillPrefab, player.transform.position, transform.rotation);
        Sword_Skill_Controller newSwordScript = newSword.GetComponent<Sword_Skill_Controller>();

        if (newSwordScript == null)
        {
            Debug.LogError("Sword_Skill: skillPrefab is missing Sword_Skill_Controller.", newSword);
            return;
        }

        newSwordScript.SetupSword(finalDir, swordGravity, player, returnSpeed);
        player.AssignNewSword(newSword);
        DotsActive(false);
    }

    public Vector2 AimDirection()
    {
        if (player == null || Camera.main == null)
            return Vector2.right;

        Vector2 playerPosition = player.transform.position;
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z - player.transform.position.z);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 direction = mousePosition - playerPosition;
        return direction;
    }

    public void DotsActive(bool _isActive)
    {
        if (dots == null || dots.Length == 0)
            GenerateDots();

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
                dots[i].SetActive(_isActive);
        }

        if (_isActive)
            UpdateDotsPosition();
    }

    private void GenerateDots()
    {
        int dotsCount = numberOfDots > 0 ? numberOfDots : DefaultNumberOfDots;

        if (dots != null && dots.Length == dotsCount)
            return;

        Transform parent = dotsParent != null ? dotsParent : CreateRuntimeDotsParent();
        dots = new GameObject[dotsCount];

        for (int i = 0; i < dots.Length; i++)
        {
            dots[i] = dotPrefab != null
                ? Instantiate(dotPrefab, player.transform.position, Quaternion.identity, parent)
                : CreateRuntimeDot(parent);
            dots[i].SetActive(false);
        }
    }

    private Transform CreateRuntimeDotsParent()
    {
        GameObject parent = new GameObject("Sword Aim Dots");
        parent.transform.SetParent(transform);
        parent.transform.localPosition = Vector3.zero;
        dotsParent = parent.transform;
        return dotsParent;
    }

    private GameObject CreateRuntimeDot(Transform parent)
    {
        GameObject dot = new GameObject("Aim Dot");
        dot.transform.SetParent(parent);
        dot.transform.localScale = Vector3.one * RuntimeDotScale;

        SpriteRenderer renderer = dot.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRuntimeDotSprite();
        renderer.color = dotColor;
        renderer.sortingLayerName = "Player";
        renderer.sortingOrder = 10;

        return dot;
    }

    private Sprite GetRuntimeDotSprite()
    {
        if (runtimeDotSprite != null)
            return runtimeDotSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        runtimeDotSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        runtimeDotSprite.hideFlags = HideFlags.HideAndDontSave;
        return runtimeDotSprite;
    }

    private Vector2 GetLaunchDirection()
    {
        Vector2 aimDirection = AimDirection().normalized;

        if (aimDirection == Vector2.zero)
            aimDirection = Vector2.right * player.facingDir;

        return new Vector2(aimDirection.x * launchDir.x, aimDirection.y * launchDir.y);
    }

    private void UpdateDotsPosition()
    {
        if (dots == null || dots.Length == 0)
            return;

        float spacing = spaceBetweenDots > 0 ? spaceBetweenDots : DefaultSpaceBetweenDots;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
                dots[i].transform.position = DotsPosition(i * spacing);
        }
    }

    private Vector2 DotsPosition(float time)
    {
        Vector2 position = (Vector2)player.transform.position
            + GetLaunchDirection() * time
            + .5f * Physics2D.gravity * swordGravity * (time * time);

        return position;
    }
}

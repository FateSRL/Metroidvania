using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerPhaseLaser : MonoBehaviour
{
    private const float LockBeforeFireTime = .25f;

    private LineRenderer lineRenderer;
    private Transform target;
    private Vector2 startPosition;
    private Vector2 targetOffset;
    private Vector2 fireDirection;
    private Vector2 lineStart;
    private Vector2 lineEnd;
    private float length;
    private float warningTime;
    private float fireTime;
    private float warningWidth;
    private float fireWidth;
    private int damage;
    private int playerMask;
    private CharacterStats ownerStats;
    private bool fired;
    private bool locked;
    private readonly HashSet<Player> damagedPlayers = new HashSet<Player>();

    public void Setup(Vector2 startPosition, Transform target, Vector2 targetOffset, float length, float warningTime, float fireTime, float warningWidth, float fireWidth, int damage, LayerMask whatIsPlayer, CharacterStats ownerStats)
    {
        this.startPosition = startPosition;
        this.target = target;
        this.targetOffset = targetOffset;
        this.length = length;
        this.warningTime = warningTime;
        this.fireTime = fireTime;
        this.warningWidth = warningWidth;
        this.fireWidth = fireWidth;
        this.damage = damage;
        this.playerMask = whatIsPlayer.value != 0 ? whatIsPlayer.value : LayerMask.GetMask("Player");
        this.ownerStats = ownerStats;

        BuildRenderer();
        StartCoroutine(LaserRoutine());
    }

    private void BuildRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingLayerName = "Player";
        lineRenderer.sortingOrder = 999;
        lineRenderer.startWidth = warningWidth;
        lineRenderer.endWidth = warningWidth;
        lineRenderer.startColor = new Color(1f, .05f, .03f, .45f);
        lineRenderer.endColor = new Color(1f, .05f, .03f, .45f);
        UpdateLine();
    }

    private IEnumerator LaserRoutine()
    {
        float lockTime = Mathf.Min(LockBeforeFireTime, warningTime);
        float trackingTime = Mathf.Max(0, warningTime - lockTime);

        while (trackingTime > 0)
        {
            trackingTime -= Time.deltaTime;
            UpdateLine();
            yield return null;
        }

        LockLaser();

        while (lockTime > 0)
        {
            lockTime -= Time.deltaTime;
            yield return null;
        }

        yield return FireLaser();
        Destroy(gameObject);
    }

    private void UpdateLine()
    {
        if (locked)
            return;

        Vector2 targetPosition = GetTargetPosition();
        fireDirection = (targetPosition - startPosition).normalized;

        if (fireDirection == Vector2.zero)
            fireDirection = Vector2.down;

        float screenLength = GetScreenCoverLength();
        lineStart = targetPosition - fireDirection * screenLength * .5f;
        lineEnd = targetPosition + fireDirection * screenLength * .5f;

        lineRenderer.SetPosition(0, lineStart);
        lineRenderer.SetPosition(1, lineEnd);
    }

    private void LockLaser()
    {
        UpdateLine();
        locked = true;
    }

    private Vector2 GetTargetPosition()
    {
        if (target == null)
            return startPosition + Vector2.down;

        return (Vector2)target.position + targetOffset;
    }

    private IEnumerator FireLaser()
    {
        if (fired)
            yield break;

        fired = true;

        if (!locked)
            LockLaser();

        AudioManager.PlaySFX(SfxType.BossLaser, .3f);

        lineRenderer.startColor = new Color(1f, 0, 0, 1f);
        lineRenderer.endColor = new Color(.75f, 0, 0, 1f);

        float growDuration = Mathf.Min(.18f, fireTime);
        float timer = 0;

        while (timer < fireTime)
        {
            timer += Time.deltaTime;

            float growProgress = growDuration > 0 ? Mathf.Clamp01(timer / growDuration) : 1;
            float currentWidth = Mathf.Lerp(warningWidth, fireWidth, growProgress);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth;

            DamagePlayersInLaser(currentWidth);

            yield return null;
        }
    }

    private void DamagePlayersInLaser(float currentWidth)
    {
        float laserLength = Vector2.Distance(lineStart, lineEnd);
        Vector2 center = (lineStart + lineEnd) * .5f;
        Vector2 boxSize = new Vector2(laserLength, currentWidth);
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angle, playerMask);
        if (hits.Length == 0 && playerMask != 0)
            hits = Physics2D.OverlapBoxAll(center, boxSize, angle);

        for (int i = 0; i < hits.Length; i++)
        {
            Player player = hits[i].GetComponent<Player>();
            if (player == null)
                player = hits[i].GetComponentInParent<Player>();

            if (player == null)
                continue;

            if (damagedPlayers.Contains(player))
                continue;

            int damageAmount = damage;
            if (damageAmount <= 0 && ownerStats != null)
                damageAmount = ownerStats.GetDamage();

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null && playerStats.isInvincible)
                playerStats.MakeInvincible(false);

            player.Damage(lineStart, Mathf.Max(1, damageAmount));
            damagedPlayers.Add(player);
        }
    }

    private float GetScreenCoverLength()
    {
        float screenLength = length;

        if (Camera.main != null && Camera.main.orthographic)
        {
            float height = Camera.main.orthographicSize * 2f;
            float width = height * Camera.main.aspect;
            screenLength = Mathf.Max(screenLength, Mathf.Sqrt(width * width + height * height) + 12f);
        }

        return screenLength;
    }

    private void OnDestroy()
    {
        if (lineRenderer != null && lineRenderer.material != null)
            Destroy(lineRenderer.material);
    }
}

using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class FallingStoneTrap : MonoBehaviour
{
    private enum StoneState
    {
        Ready,
        Warning,
        Falling,
        Landed,
        Returning,
        Cooldown
    }

    [Header("Movement")]
    [SerializeField] private Transform stoneVisual;
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private float fallDistance = 4f;
    [SerializeField] private float warningTime = .25f;
    [SerializeField] private float fallAcceleration = 45f;
    [SerializeField] private float maxFallSpeed = 22f;
    [SerializeField] private float landedTime = .25f;
    [SerializeField] private float returnSpeed = 4f;
    [SerializeField] private float cooldown = .35f;

    [Header("Damage")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private bool killDuringLanded = true;
    [SerializeField] private bool killDuringReturn;

    [Header("Warning")]
    [SerializeField] private float warningShakeDistance = .04f;
    [SerializeField] private float warningShakeSpeed = 45f;

    private readonly Collider2D[] playerHits = new Collider2D[8];
    private Transform movingTransform;
    private Vector3 startPosition;
    private Vector3 bottomPosition;
    private float currentFallSpeed;
    private bool killedPlayerThisDrop;
    private Coroutine trapRoutine;
    private StoneState state = StoneState.Ready;

    public bool IsReady => state == StoneState.Ready;

    private void Awake()
    {
        movingTransform = stoneVisual != null ? stoneVisual : transform;
        startPosition = movingTransform.position;
        bottomPosition = bottomPoint != null ? bottomPoint.position : startPosition + Vector3.down * fallDistance;

        if (damageCollider == null)
            damageCollider = movingTransform.GetComponent<Collider2D>();

        if (playerLayer.value == 0)
            playerLayer = LayerMask.GetMask("Player");
    }

    private void OnDisable()
    {
        if (trapRoutine != null)
            StopCoroutine(trapRoutine);

        trapRoutine = null;
        state = StoneState.Ready;
    }

    public void Trigger(float delay = 0)
    {
        if (!isActiveAndEnabled || state != StoneState.Ready || trapRoutine != null)
            return;

        trapRoutine = StartCoroutine(TrapRoutine(delay));
    }

    public void ResetTrap()
    {
        if (trapRoutine != null)
            StopCoroutine(trapRoutine);

        currentFallSpeed = 0;
        killedPlayerThisDrop = false;
        movingTransform.position = startPosition;
        state = StoneState.Ready;
        trapRoutine = null;
    }

    private IEnumerator TrapRoutine(float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        killedPlayerThisDrop = false;
        yield return WarningRoutine();
        yield return FallingRoutine();
        yield return LandedRoutine();
        yield return ReturningRoutine();

        state = StoneState.Cooldown;
        if (cooldown > 0)
            yield return new WaitForSeconds(cooldown);

        state = StoneState.Ready;
        trapRoutine = null;
    }

    private IEnumerator WarningRoutine()
    {
        state = StoneState.Warning;
        float timer = 0;

        while (timer < warningTime)
        {
            timer += Time.deltaTime;

            if (warningShakeDistance > 0)
            {
                float shake = Mathf.Sin(Time.time * warningShakeSpeed) * warningShakeDistance;
                movingTransform.position = startPosition + Vector3.right * shake;
            }

            yield return null;
        }

        movingTransform.position = startPosition;
    }

    private IEnumerator FallingRoutine()
    {
        state = StoneState.Falling;
        currentFallSpeed = 0;

        while (Vector3.Distance(movingTransform.position, bottomPosition) > .02f)
        {
            currentFallSpeed = Mathf.Min(maxFallSpeed, currentFallSpeed + fallAcceleration * Time.deltaTime);
            movingTransform.position = Vector3.MoveTowards(movingTransform.position, bottomPosition, currentFallSpeed * Time.deltaTime);
            CheckPlayerOverlap();
            yield return null;
        }

        movingTransform.position = bottomPosition;
        CheckPlayerOverlap();
    }

    private IEnumerator LandedRoutine()
    {
        state = StoneState.Landed;
        float timer = 0;

        while (timer < landedTime)
        {
            timer += Time.deltaTime;
            CheckPlayerOverlap();
            yield return null;
        }
    }

    private IEnumerator ReturningRoutine()
    {
        state = StoneState.Returning;

        while (Vector3.Distance(movingTransform.position, startPosition) > .02f)
        {
            movingTransform.position = Vector3.MoveTowards(movingTransform.position, startPosition, returnSpeed * Time.deltaTime);

            if (killDuringReturn)
                CheckPlayerOverlap();

            yield return null;
        }

        movingTransform.position = startPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillPlayer(collision.collider);
    }

    private void CheckPlayerOverlap()
    {
        if (damageCollider == null || !damageCollider.enabled)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useLayerMask = true;
        filter.useTriggers = true;

        int count = damageCollider.OverlapCollider(filter, playerHits);
        for (int i = 0; i < count; i++)
            TryKillPlayer(playerHits[i]);
    }

    private void TryKillPlayer(Collider2D other)
    {
        if (!IsLethalState() || killedPlayerThisDrop || other == null)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.MakeInvincible(false);
            stats.TakeDamage(stats.GetMaxHealthValue() + 999);
        }
        else
        {
            player.Die();
        }

        killedPlayerThisDrop = true;
    }

    private bool IsLethalState()
    {
        return state == StoneState.Falling ||
               (state == StoneState.Landed && killDuringLanded) ||
               (state == StoneState.Returning && killDuringReturn);
    }

    private void OnDrawGizmosSelected()
    {
        Transform targetTransform = stoneVisual != null ? stoneVisual : transform;
        Vector3 start = Application.isPlaying ? startPosition : targetTransform.position;
        Vector3 end = bottomPoint != null ? bottomPoint.position : start + Vector3.down * fallDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, .12f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end, .16f);
    }
}

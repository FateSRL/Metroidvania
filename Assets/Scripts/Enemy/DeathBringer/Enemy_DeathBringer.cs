using UnityEngine;

public class Enemy_DeathBringer : Enemy
{
    public DeathBringerIdleState idleState { get; private set; }
    public DeathBringerBattleState battleState { get; private set; }
    public DeathBringerAttackState attackState { get; private set; }
    public DeathBringerSpellCastState spellCastState { get; private set; }
    public DeathBringerTeleportState teleportState { get; private set; }
    public DeathBringerDeadState deadState { get; private set; }

    [Header("Boss fight")]
    [SerializeField] private float bossFightDistance = 8f;

    [Header("Phase two")]
    [SerializeField] private float phaseTwoHealthPercent = .5f;
    [SerializeField] private float phaseTwoMoveSpeedBonus = .7f;
    [SerializeField] private float phaseTwoAttackCooldownMultiplier = .75f;

    [Header("Spell cast")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Vector2 spellOffset = new Vector2(1.5f, .8f);
    [SerializeField] private int amountOfSpells = 2;
    [SerializeField] private float spellCooldown = .45f;
    [SerializeField] private float spellStateCooldown = 5f;

    [Header("Teleport")]
    [SerializeField] private BoxCollider2D arena;
    [SerializeField] private Vector2 surroundingCheckSize = new Vector2(2.3f, 2.8f);
    [SerializeField] private float arenaPadding = 2.5f;
    [SerializeField] private float minTeleportDistanceFromPlayer = 3f;
    [SerializeField] private int teleportSearchAttempts = 20;
    [SerializeField] private Vector2 teleportOffsetFromPlayer = new Vector2(4f, 0);
    [SerializeField] private float teleportCooldown = 4f;
    [SerializeField] private float defaultChanceToTeleport = 25f;
    [SerializeField] private float chanceIncreasePerAttack = 15f;

    public bool bossFightBegun { get; private set; }
    public bool phaseTwoActive { get; private set; }
    public int AmountOfSpells => phaseTwoActive ? amountOfSpells + 1 : amountOfSpells;
    public float SpellCooldown => phaseTwoActive ? spellCooldown * .8f : spellCooldown;

    private Transform player;
    private bool isBossDead;
    private float lastTimeCast = -999f;
    private float lastTimeTeleported = -999f;
    private float chanceToTeleport;
    private float defaultAttackCooldown;
    private Bounds fallbackTeleportBounds;

    protected override void Awake()
    {
        base.Awake();

        SetupDefaultFacingDir(-1);

        idleState = new DeathBringerIdleState(this, stateMachine, "Idle", this);
        battleState = new DeathBringerBattleState(this, stateMachine, "Move", this);
        attackState = new DeathBringerAttackState(this, stateMachine, "Attack", this);
        spellCastState = new DeathBringerSpellCastState(this, stateMachine, "SpellCast", this);
        teleportState = new DeathBringerTeleportState(this, stateMachine, "Teleport", this);
        deadState = new DeathBringerDeadState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            player = PlayerManager.instance.player.transform;

        defaultAttackCooldown = attackCooldown;
        chanceToTeleport = defaultChanceToTeleport;
        fallbackTeleportBounds = new Bounds(transform.position + new Vector3(-3f, 2.5f, 0), new Vector3(18f, 8f, 0));
        FacePlayer();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        if (isBossDead)
            return;

        base.Update();
        CheckPhaseTwo();
    }

    public bool ShouldStartBossFight()
    {
        if (bossFightBegun)
            return true;

        if (player == null)
            return false;

        if (Vector2.Distance(player.position, transform.position) <= bossFightDistance)
        {
            bossFightBegun = true;
            return true;
        }

        return false;
    }

    public void FacePlayer()
    {
        if (player == null)
            return;

        if (player.position.x > transform.position.x && facingDir == -1)
            Flip();
        else if (player.position.x < transform.position.x && facingDir == 1)
            Flip();
    }

    public bool CanDoSpellCast()
    {
        return phaseTwoActive && spellPrefab != null && Time.time >= lastTimeCast + spellStateCooldown;
    }

    public bool CanTeleport()
    {
        if (!phaseTwoActive || Time.time < lastTimeTeleported + teleportCooldown)
            return false;

        if (Random.Range(0, 100f) > chanceToTeleport)
            return false;

        chanceToTeleport = defaultChanceToTeleport;
        return true;
    }

    public void IncreaseTeleportChance()
    {
        if (!phaseTwoActive)
            return;

        chanceToTeleport = Mathf.Min(100, chanceToTeleport + chanceIncreasePerAttack);
    }

    public void MarkSpellCastFinished()
    {
        lastTimeCast = Time.time;
    }

    public void CastSpell()
    {
        if (spellPrefab == null || player == null)
            return;

        float xOffset = 0;
        if (player.TryGetComponent(out Rigidbody2D playerRb) && Mathf.Abs(playerRb.velocity.x) > .1f)
            xOffset = player.GetComponent<Player>().facingDir * Mathf.Abs(spellOffset.x);

        Vector3 spellPosition = new Vector3(player.position.x + xOffset, player.position.y + spellOffset.y, 0);
        GameObject newSpell = Instantiate(spellPrefab, spellPosition, Quaternion.identity);

        if (newSpell.TryGetComponent(out DeathBringerSpell_Controller spell))
            spell.SetupSpell(stats);
    }

    public void TeleportNearPlayer()
    {
        if (isBossDead)
            return;

        FindPosition();
    }

    public void FindPosition()
    {
        if (TryFindTeleportPosition(out Vector3 newPosition))
            transform.position = newPosition;

        lastTimeTeleported = Time.time;
        FacePlayer();
    }

    private bool TryFindTeleportPosition(out Vector3 position)
    {
        position = transform.position;

        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider == null || player == null)
            return false;

        float offset = Mathf.Max(minTeleportDistanceFromPlayer, Mathf.Abs(teleportOffsetFromPlayer.x));
        int preferredSide = player.position.x >= transform.position.x ? -1 : 1;

        for (int i = 0; i < teleportSearchAttempts; i++)
        {
            int side = i % 2 == 0 ? preferredSide : -preferredSide;
            float distance = offset + (i / 2) * .75f;
            float targetX = player.position.x + side * distance;

            if (TryBuildTeleportPosition(targetX, bossCollider, out position))
                return true;
        }

        return false;
    }

    private bool TryBuildTeleportPosition(float targetX, Collider2D bossCollider, out Vector3 position)
    {
        position = transform.position;

        Bounds searchBounds = GetTeleportSearchBounds();
        float minX = searchBounds.min.x + arenaPadding;
        float maxX = searchBounds.max.x - arenaPadding;

        targetX = Mathf.Clamp(targetX, minX, maxX);

        if (player != null && Mathf.Abs(targetX - player.position.x) < minTeleportDistanceFromPlayer)
            return false;

        float rayDistance = searchBounds.size.y + 20f;
        float colliderBottomOffset = bossCollider.bounds.min.y - transform.position.y;
        float rayStartY = Mathf.Max(searchBounds.max.y + 10f, player.position.y + 10f, transform.position.y + 10f);

        Vector2 rayStart = new Vector2(targetX, rayStartY);
        RaycastHit2D groundHit = Physics2D.Raycast(rayStart, Vector2.down, rayDistance, whatIsGround);

        if (!groundHit)
            return false;

        Vector3 candidate = new Vector3(targetX, groundHit.point.y - colliderBottomOffset + .03f + teleportOffsetFromPlayer.y, transform.position.z);

        if (!PositionHasRoom(candidate))
            return false;

        position = candidate;
        return true;
    }

    private Bounds GetTeleportSearchBounds()
    {
        if (arena != null && player != null)
        {
            Bounds arenaBounds = arena.bounds;
            bool playerInsideArena = player.position.x >= arenaBounds.min.x && player.position.x <= arenaBounds.max.x;

            if (playerInsideArena)
                return arenaBounds;
        }

        if (player != null)
            return new Bounds(player.position + new Vector3(0, 2.5f, 0), new Vector3(18f, 8f, 0));

        return fallbackTeleportBounds;
    }

    private bool PositionHasRoom(Vector3 position)
    {
        return !Physics2D.OverlapBox(position, surroundingCheckSize, 0, whatIsGround);
    }


    public void MakeVisible(bool visible)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }

    public void PlayDeathAnimation()
    {
        if (anim == null)
            return;

        anim.speed = 1;
        anim.SetBool("Idle", false);
        anim.SetBool("Move", false);
        anim.SetBool("Attack", false);
        anim.SetBool("SpellCast", false);
        anim.SetBool("Teleport", false);
        anim.Play("deathBringerExit", 0, 0);
    }

    public override void Die()
    {
        if (isBossDead)
            return;

        isBossDead = true;

        stateMachine.ChangeState(deadState);

        if (counterImage != null)
            counterImage.SetActive(false);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bossFightDistance);
    }

    private void CheckPhaseTwo()
    {
        if (phaseTwoActive || stats == null)
            return;

        if (stats.currentHealth > stats.GetMaxHealthValue() * phaseTwoHealthPercent)
            return;

        phaseTwoActive = true;
        moveSpeed += phaseTwoMoveSpeedBonus;
        attackCooldown = Mathf.Max(.2f, defaultAttackCooldown * phaseTwoAttackCooldownMultiplier);
    }
}
